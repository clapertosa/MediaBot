using System;
using DbUp;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Database
{
    public static class InitializeDatabase
    {
        public static string GetDatabaseUrlFormatted(string url)
        {
            var urlCopy = url.Replace("postgres://", "");
            var usernameAndPassword = urlCopy[..urlCopy.IndexOf('@')].Split(':');
            var hostAndPort = urlCopy[(urlCopy.IndexOf('@') + 1)..urlCopy.IndexOf('/')].Split(':');
            var databaseName = urlCopy[(urlCopy.LastIndexOf('/') + 1)..urlCopy.Length];

            return
                $"Username = {usernameAndPassword[0]};" +
                $"Password = {usernameAndPassword[1]};" +
                $"Server = {hostAndPort[0]};" +
                $"Port = {hostAndPort[1]};" +
                $"Database = {databaseName};" +
                "SSL Mode = Prefer;" +
                "Trust Server Certificate = true";
        }

        public static void Run(IConfiguration configuration)
        {
            var databaseUri = GetDatabaseUrlFormatted(configuration["DATABASE_URL"]);
#if DEBUG
            EnsureDatabase.For.PostgresqlDatabase(databaseUri);
#endif
            var upgrader = DeployChanges.To.PostgresqlDatabase(databaseUri)
                .WithScriptsFromFileSystem("Database/Scripts").LogToConsole().Build();
            var result = upgrader.PerformUpgrade();
            if (!result.Successful) throw new Exception("An error occurred during database initialization (DbUp).");

            // Seed all initial data method
        }
    }
}