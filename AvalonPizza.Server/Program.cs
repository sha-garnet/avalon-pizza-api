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
using System.Text.RegularExpressions;

namespace AvalonPizza.Server;

public class Program
{
    private const string PizzaPolicy = "PizzaFrontendPolicy";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //builder.WebHost.UseUrls("http://+:5000"); // maybe I need it

        // Logging
        var logPath = builder.Configuration["LoggingPaths:PizzaLog"]!; // ! Trust Me
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(path: logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
        .CreateLogger();
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

        // Building Connection String using AWS Systems Manager > Parameter Store
        builder.Configuration.AddSystemsManager("/pizzaapi", new AWSOptions
        {
            Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
        });

        var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var user = builder.Configuration["DbUser"];
        var pass = builder.Configuration["DbPassword"];
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            throw new Exception("Failed to retrieve credentials from AWS Parameter Store.");
        }

        var connectionStringBuilder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            UserID = user,
            Password = pass,
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

        // SQL Scripting => This runs every time you hit 'Start' in Visual Studio
        DbInitializer.Initialize(connectionString);

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
    /// SQL Scripting / Database Schema Scripting
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(string connectionString)
        {
            Log.Information("Database initialization started using master catalog.");

            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "TablesAndProcs.sql");
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Could not find the SQL script at: {scriptPath}");
            }
            var script = File.ReadAllText(scriptPath);

            // First, connect to 'master' to ensure the database exists
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                // You cannot create a database called AvalonPizza while you are connected to AvalonPizza.
                // You have to connect to master first to run the CREATE DATABASE command.
                InitialCatalog = "master"
            };
            var masterConnection = builder.ConnectionString;

            using (var conn = new SqlConnection(masterConnection))
            {
                conn.Open();

                // SQL scripts with 'GO' commands need to be split because
                // Splitting SQL scripts by the word "GO" is tricky because "GO" can appear inside words like CATEGORY, DOG, or within comments
                // ADO.NET doesn't understand the 'GO' keyword
                string pattern = @"^\s*GO\s*$";
                var batches = Regex.Split(script, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    string trimmedBatch = batch.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedBatch)) continue;

                    using (var cmd = new SqlCommand(trimmedBatch, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Log.Information("Database initialization completed successfully.");
            }
        }
    }
}