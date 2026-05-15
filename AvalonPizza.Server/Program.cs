using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Middleware;
using AvalonPizza.Server.Repositories;
using AvalonPizza.Server.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AvalonPizza.Server;

public class Program
{
    private const string PizzaPolicy = "PizzaFrontendPolicy";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddSingleton(connectionString);

        // Add services to the container.
        builder.Services.AddControllers();

        // Dependency Injection list
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IToppingRepository, ToppingRepository>();
        builder.Services.AddScoped<IPizzaSizeRepository, PizzaSizeRepository>();
        builder.Services.AddScoped<ICacheService, CacheService>();

        // SWAGGER
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Avalon Pizza API", Version = "v1" });

            // 1. Define the Security Scheme (The "Lock")
            c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "Enter your API Key in the box below. Format: X-Api-Key: YOUR_KEY",
                Name = "X-Api-Key", // The header name your middleware looks for
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            // 2. Apply the Security Requirement (The "Keycard")
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "ApiKey" // Must match the name defined above
                        },
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        // Health Check
        builder.Services.AddHealthChecks()
            .AddSqlServer(
                connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
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

        // API Key Authentication
        app.UseRouting(); // Identifies the endpoint (Public or [Authorize])
        app.UseMiddleware<ApiKeyMiddleware>();

        // SQL Scripting => This runs every time you hit 'Start' in Visual Studio
        DbInitializer.Initialize(connectionString);

        // Useing the CORS Policy: UseCors must be placed AFTER UseRouting (if used) and BEFORE MapControllers
        app.UseCors(PizzaPolicy);

        // Optional: Sets the default home page (localhost:xxxx/)
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
                    <h1>🍕 Avalon Pizza API</h1>
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