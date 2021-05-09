using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using DiscordConsoleApp.Services;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordConsoleApp
{
    class Program
    {
        private const string DatabaseName = "discord_imdbot";
        private static void InitializeDatabase(IConfiguration configuration)
        {
            var connString = configuration["Databases:DiscordConnectionString"] ;
            var cmdText = "select count(*) from master.dbo.sysdatabases where name=@database";

            using var sqlConnection = new SqlConnection(connString);
            sqlConnection.Open();
            
            using var sqlCheckDatabaseCmd = new SqlCommand(cmdText, sqlConnection);
            sqlCheckDatabaseCmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = DatabaseName;
            
            // If database does not exist
            if (Convert.ToInt32(sqlCheckDatabaseCmd.ExecuteScalar()) == 0)
            {
                string sqlCreateDbText = "CREATE DATABASE [discord_imdbot]";
                using SqlCommand sqlCreateDb = new SqlCommand(sqlCreateDbText, sqlConnection);
                sqlCreateDb.ExecuteNonQuery();
                sqlConnection.ChangeDatabase(DatabaseName);
                // Create Tables
                string sqlCmdText = File.ReadAllText("./SQL/Queries/InitializeDB.sql");
                using SqlCommand sqlCmds = new SqlCommand(sqlCmdText, sqlConnection);
                sqlCmds.ExecuteNonQuery();
                // AddMedia Store Procedure
                string sqlAddMedia = File.ReadAllText("./SQL/StoredProcedures/AddMedia.sql");
                using SqlCommand sqlCmdAddMedia = new SqlCommand(sqlAddMedia, sqlConnection);
                sqlCmdAddMedia.ExecuteNonQuery();
                // RemoveMedia Store Procedure
                string sqlRemoveMedia = File.ReadAllText("./SQL/StoredProcedures/RemoveMedia.sql");
                using SqlCommand sqlCmdRemoveMedia = new SqlCommand(sqlRemoveMedia, sqlConnection);
                sqlCmdRemoveMedia.ExecuteNonQuery();
                // Get User Media
                string sqlGetUserMediaSp = File.ReadAllText("./SQL/StoredProcedures/GetUserMedia.sql");
                using SqlCommand sqlCmdGetUserMediaSp = new SqlCommand(sqlGetUserMediaSp, sqlConnection);
                sqlCmdGetUserMediaSp.ExecuteNonQuery();
            }
        }
        
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("config.json", true, true)
                        .AddEnvironmentVariables()
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel
                        .Debug); // Defines what kind of information should be logged (e.g. Debug, Information, Warning, Critical) adjust this to your liking
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    InitializeDatabase(context.Configuration);
                    
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity
                            .Debug, // Defines what kind of information should be logged from the API (e.g. Verbose, Info, Warning, Critical) adjust this to your liking
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["Tokens:Discord"];
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Sync;
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<CommandHandler>();
                    services.AddInfrastructure();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}