namespace AvalonPizza.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
        builder.Services.AddControllers();

        var app = builder.Build();

        app.MapGet("/", () => new { message = "AVALON PIZZA API" });

        // Configure the HTTP request pipeline.
        app.MapControllers(); // This maps the attribute routes like [Route("api/[controller]")]

        app.Run();
    }
}