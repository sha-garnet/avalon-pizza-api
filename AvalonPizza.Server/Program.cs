using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Repositories;
using AvalonPizza.Server.Services;
using Microsoft.Data.SqlClient;
using Serilog;
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

        var app = builder.Build();

        // Global Error Handling
        // Placed near the top (before app.MapControllers().). It means it wraps around everything that follows (the Database initializer, the Controllers, etc.).
        // If anything below it fails, your "net" will catch it. This "net" catches any unhandled errors in your API. Relyin on our global middleware
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred during request {Path}", context.Request.Path);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorResponse = new
                {
                    error = "A server error occurred.",
                    details = app.Environment.IsDevelopment() ? ex.Message : "Contact support for assistance."
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        });

        // This runs every time you hit 'Start' in Visual Studio
        DbInitializer.Initialize(connectionString);

        // Useing the CORS Policy: UseCors must be placed AFTER UseRouting (if used) and BEFORE MapControllers
        app.UseCors(PizzaPolicy);

        app.MapGet("/", () => new { message = "AVALON PIZZA API!" });

        // Configure the HTTP request pipeline.
        app.MapControllers();

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

            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "InitializeDb.sql");
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