using Microsoft.Data.SqlClient;

namespace AvalonPizza.Server;

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
