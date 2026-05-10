using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Repositories;

namespace AvalonPizza.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=PizzaStoreDb;Trusted_Connection=True;TrustServerCertificate=True;";
        builder.Services.AddSingleton(connectionString);

        // Add services to the container.
        builder.Services.AddControllers();


        // Dependency Injection
        builder.Services.AddScoped<IPizzaRepository, PizzaRepository>();


        var app = builder.Build();

        // Placed near the top. It means it wraps around everything that follows (the Database initializer, the Controllers, etc.).
        // If anything below it fails, your "net" will catch it. This "net" catches any unhandled errors in your API
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = "A server error occurred.",
                    // Only show the techy details if we are in Development mode
                    details = app.Environment.IsDevelopment() ? ex.Message : "Contact support for assistance."
                };
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        });

        // This runs every time you hit 'Start' in Visual Studio
        DbInitializer.Initialize(connectionString);

        app.MapGet("/", () => new { message = "AVALON PIZZA API" });

        // Configure the HTTP request pipeline.
        app.MapControllers();

        app.Run();
    }
}