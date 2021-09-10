using System;
using System.Data;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Dapper;
using Domain.Configurations;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddTransient<IDbConnection>(_ =>
                {
                    DefaultTypeMap.MatchNamesWithUnderscores = true;
                    return new NpgsqlConnection(
                        InitializeDatabase.GetDatabaseUrlFormatted(configuration["DATABASE_URL"]));
                }
            );

            // Http Client
            services.AddHttpClient("imdb-client",
                client =>
                {
                    client.BaseAddress = new Uri(configuration.GetSection("IMDB").Get<ImdbConfiguration>().SearchAPI);
                });

            services.AddScoped<IImdbRepository, ImdbRepository>();
            services.AddSingleton<IHtmlUtils, HtmlUtils>();
        }
    }
}