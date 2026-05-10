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





        var app = builder.Build();

        // This runs every time you hit 'Start' in Visual Studio
        DbInitializer.Initialize(connectionString);

        app.MapGet("/", () => new { message = "AVALON PIZZA API" });

        // Configure the HTTP request pipeline.
        app.MapControllers();

        app.Run();
    }
}