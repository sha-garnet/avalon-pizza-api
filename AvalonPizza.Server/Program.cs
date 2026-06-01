using Amazon;
using Amazon.Extensions.NETCore.Setup;
using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Middleware;
using AvalonPizza.Server.Options;
using AvalonPizza.Server.Repositories;
using AvalonPizza.Server.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json;

namespace AvalonPizza.Server;

public class Program
{
    private const string PizzaPolicy = "PizzaFrontendPolicy";

    public static void Main(string[] args)
    {
        // Using the Minimal Hosting Model
        var builder = WebApplication.CreateBuilder(args);


        // AWS Serveless Lambda
        // The Bridge that allows the app to respond to Lambda events
        builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);


        // Logging
        // If running in Lambda, environment variable AWS_LAMBDA_FUNCTION_NAME will exist.
        // Force logs to Console if in Lambda, otherwise use your file path.
        var isLambda = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));
        var loggerConfig = new LoggerConfiguration().WriteTo.Console();
        if (isLambda)
        {
            var logPath = builder.Configuration["LoggingPaths:PizzaLog"]!; // ! Trust Me
            loggerConfig.WriteTo.File(path: logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
        }
        // Configure Serilog
        Log.Logger = loggerConfig.CreateLogger();
        // Tell ASP.NET Core to use Serilog
        builder.Host.UseSerilog();


        // CORS
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();
        // Define the CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(PizzaPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader() // This allows use custom headers (like Authorization tokens)
                      .AllowAnyMethod(); // This allows GET, POST, PUT, and DELETE
            });
        });


        // Redis
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
            options.InstanceName = "AvalonPizza_";
        });


        // Building the Connection String using AWS Systems Manager > Parameter Store
        builder.Configuration.AddSystemsManager("/pizzaapi", new AWSOptions
        {
            Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
        });

        // Get from appsettings.json
        //var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        //    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Get from AWS
        string baseConnectionString = GetRequiredEnvironmentVariable("DB_HOST");

        var user = builder.Configuration["DbUser"];
        var pass = builder.Configuration["DbPassword"];
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            throw new Exception("Failed to retrieve credentials from AWS Parameter Store.");
        }

        // TODO: Now that the API doesn't need to create the database, ensure the database credentials stored in
        // your AWS Systems Manager Parameter Store only grant DML permissions
        // (Data Manipulation Language: CRUD - restrict your API's database user to only SELECT, INSERT, UPDATE, and DELETE permissions)
        // to the AvalonPizza database, and absolutely no DDL permissions (Data Definition Language: Create/Drop/Alter)

        var connectionStringBuilder = new SqlConnectionStringBuilder()
        {
            DataSource = baseConnectionString,
            InitialCatalog = "PizzaStoreDb",
            UserID = user,
            Password = pass,
            Encrypt = true,
            TrustServerCertificate = false
        };

        var connectionString = connectionStringBuilder.ConnectionString;
        builder.Services.AddSingleton(new DatabaseOptions { ConnectionString = connectionString });


        // Add services to the container.
        builder.Services.AddControllers();

        // Dependency Injection list
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IToppingRepository, ToppingRepository>();
        builder.Services.AddScoped<IToppingService, ToppingService>();
        builder.Services.AddScoped<IPizzaSizeRepository, PizzaSizeRepository>();
        builder.Services.AddScoped<IPizzaSizeService, PizzaSizeService>();
        builder.Services.AddScoped<ICacheService, CacheService>();


        // Register AutoMapper
        builder.Services.AddAutoMapper(cfg => { }, typeof(Program));


        // SWAGGER (Swashbuckle)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Generate the Swagger header
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Avalon Pizza API",
                Description = "Dynamically generated API documentation using **Swashbuckle**!"
            });

            // The option to authorize
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "Enter your API Key in the box below. Format: X-Api-Key: YOUR_KEY",
                Name = "X-Api-Key", // The header name your middleware looks for
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            // Apply the Security Requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        },
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Feed XML comments into Swagger UI interface
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });


        // Health Check
        builder.Services.AddHealthChecks()
            .AddSqlServer(
                connectionString: connectionString,
                name: "SQL Server",
                tags: new[] { "db", "sql" })
            .AddRedis(
                redisConnectionString: builder.Configuration.GetConnectionString("RedisConnection")!,
                name: "Redis Cache",
                tags: new[] { "cache", "redis" });



        var app = builder.Build();



        // SWAGGER
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Avalon Pizza API v1");
            });
        }

        // Global Error Handling
        // Placed near the top (before app.MapControllers().). It means it wraps around everything that follows (the Database initializer, the Controllers, etc.).
        // If anything below it fails, your "net" will catch it. This "net" catches any unhandled errors in your API. Relyin on our global middleware
        // TODO mode out and create its own middleware class
        app.UseMiddleware<SqlExceptionMiddleware>();

        // Identifies which endpoint action route match the HTTP incoming path
        app.UseRouting();

        // Useing the CORS Policy: UseCors must be placed AFTER UseRouting (if used) and BEFORE MapControllers
        app.UseCors(PizzaPolicy);

        // AUTHENTICATION: Custom API key middleware
        app.UseMiddleware<ApiKeyMiddleware>();

        // Sets the landing page (localhost:xxxx/)
        app.MapGet("/", () => Results.Content(@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Avalon Pizza API</title>
                <style>
                    body { font-family: sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background-color: #f4f4f9; }
                    .card { background: white; padding: 2rem; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); text-align: center; }
                    h1 { color: #d32f2f; margin-bottom: 1.5rem; }
                    .links { display: flex; gap: 20px; justify-content: center; }
                    a { text-decoration: none; color: #1976d2; font-weight: bold; border: 1px solid #1976d2; padding: 10px 20px; border-radius: 4px; transition: all 0.2s; }
                    a:hover { background-color: #1976d2; color: white; }
                </style>
            </head>
            <body>
                <div class='card'>
                    <h1>Avalon Pizza API</h1>
                    <p>Backend Status: <strong>Online</strong></p>
                    <div class='links'>
                        <a href='/swagger'>API Documentation (Swagger)</a>
                        <a href='/health'>System Health Status</a>
                    </div>
                </div>
            </body>
            </html>", "text/html")
        );

        // Configure the HTTP request pipeline.
        app.MapControllers();

        // Health Check
        // Run your app and navigate to localhost:xxxx/health
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        exception = entry.Value.Exception?.Message ?? "none",
                        duration = entry.Value.Duration.ToString()
                    })
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        });

        app.Run();
    }

    /// <summary>
    /// TODO: Move out of Program.cs
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Critical configuration missing: Environment variable '{name}' was not set.");
        }

        return value;
    }
}