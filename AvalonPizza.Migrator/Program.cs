using Amazon;
using Amazon.Extensions.NETCore.Setup;
using DbUp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AvalonPizza.Migrator;

internal class Program
{
    static int Main(string[] args)
    {
        try
        {
            var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);

            // Configuration Builder follows the builder pattern, allowing us to add multiple configuration sources in a flexible way.
            // The build() at the end compiles all the sources into a single configuration object finalizing the setup.
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables() // it can pick up variables from the environment, which is useful for local development and CI/CD pipelines
                .AddSystemsManager("/pizzaapi", new AWSOptions { Region = RegionEndpoint.CACentral1 })
                .Build();

            // Get from AWS
            string baseConnectionString = GetRequiredEnvironmentVariable("DB_HOST");

            var user = configuration["DbUser"]; // user needs DDL (Data Definition Language: CREATE, ALTER, DROP) permissions to create the database if it doesn't exist, and to run the migration scripts which typically include CREATE TABLE, ALTER TABLE, etc.
            var pass = configuration["DbPassword"];
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                throw new Exception("Failed to retrieve credentials from AWS Parameter Store.");
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = baseConnectionString,
                InitialCatalog = "master",
                UserID = isDevelopment ? string.Empty : user,
                Password = isDevelopment ? string.Empty : pass,
                Encrypt = true,
                TrustServerCertificate = isDevelopment
            };

            // Ensure the database exists before trying to run migrations against it. If it doesn't exist, create it.
            using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PizzaStoreDb') CREATE DATABASE [PizzaStoreDb]";
                cmd.ExecuteNonQuery();
            }

            // Now that we know the database exists, we can point our connection string to it for the migration process.
            connectionStringBuilder.InitialCatalog = "PizzaStoreDb";

            var upgrader = DeployChanges.To // fluent api design pattern allows for chaining method calls in a readable way
                .SqlDatabase(connectionStringBuilder.ConnectionString) // which engine to use and where the database is located
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly()) // where to find the migration scripts, in this case, they are embedded as resources in the assembly. 
                                                                                // in .scproj file, we specify that all .sql files should be "EmbeddedResource".
                                                                                // DbUp automattically sorts the scripts alphabetically, this is why we prefix them with numbers (001_, 002_, etc.) to ensure they run in the correct order.
                .LogToConsole() // if using jenkins, this will allow us to see the migration logs in the console output.
                .Build();

            if (upgrader.IsUpgradeRequired()) // checks if there are any pending migrations that need to be applied to the database.
                                              // it does this by comparing the scripts that have been executed (tracked in a special table in the database) with the scripts available in the assembly.
            {
                Console.WriteLine("Migrations are pending...");

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.Error);
                    Console.ResetColor();
                    return 1; // non-zero exit code stops the Jenkins pipeline
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success! Database upgraded.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Database is already up to date.");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.ResetColor();

            return 1; // return 1 so Jenkins knows the build failed
        }
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
