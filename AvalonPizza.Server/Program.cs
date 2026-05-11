using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Repositories;
using Microsoft.Data.SqlClient;
using Serilog;

namespace AvalonPizza.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
         Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(path: @"C:\temp\log\pizza_api_.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
        .CreateLogger();
        // Tell ASP.NET Core to use Serilog
        builder.Host.UseSerilog();

        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=PizzaStoreDb;Trusted_Connection=True;TrustServerCertificate=True;";
        
        builder.Services.AddSingleton(connectionString);

        // Add services to the container.
        builder.Services.AddControllers();

        // Dependency Injection list
        builder.Services.AddScoped<IPizzaRepository, PizzaRepository>();

        var app = builder.Build();

        // Placed near the top. It means it wraps around everything that follows (the Database initializer, the Controllers, etc.).
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
            // First, connect to 'master' to ensure the database exists
            var masterConnection = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "InitializeDb.sql");
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Could not find the SQL script at: {scriptPath}");
            }
            var script = File.ReadAllText(scriptPath);

            using (var conn = new SqlConnection(masterConnection))
            {
                conn.Open();

                // SQL scripts with 'GO' commands need to be split because
                // ADO.NET doesn't understand the 'GO' keyword.
                var batches = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;

                    using (var cmd = new SqlCommand(batch, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}