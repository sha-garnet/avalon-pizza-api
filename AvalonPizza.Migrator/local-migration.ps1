$env:ENVIRONMENT = "Development"
$env:DB_HOST = "(localdb)\MSSQLLocalDB"

# Testing the migration script on the local database
dotnet run --project C:\Source\AvalonPizza\AvalonPizza.Migrator\AvalonPizza.Migrator.csproj