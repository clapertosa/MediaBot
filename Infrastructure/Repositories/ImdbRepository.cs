using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Dapper;
using Discord;
using Domain.Configurations;
using Domain.Entities;
using Domain.Entities.Imdb;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories
{
    public class ImdbRepository : IImdbRepository
    {
        private readonly HttpClient _client;
        private readonly IDbConnection _connection;
        private readonly IHtmlUtils _htmlUtils;
        private readonly ImdbConfiguration _imdbConfiguration;

        private readonly string titleUri = "title/";
        // private readonly string _searchUri = "find?q=";

        public ImdbRepository(IHtmlUtils htmlUtils, IDbConnection connection, IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _htmlUtils = htmlUtils;
            _connection = connection;
            _client = httpClientFactory.CreateClient("imdb-client");
            _imdbConfiguration = configuration.GetSection("IMDB").Get<ImdbConfiguration>();
        }

        public async Task<IEnumerable<Media>> Search(string title)
        {
            var res = await _client.GetAsync($"{title[0]}/{title}.json");
            var results =
                JsonSerializer.Deserialize<SearchResultsJson>(await res.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            var media = new List<Media>();
            if (results.ResultList != null)
                foreach (var result in results.ResultList)
                    media.Add(new Media
                    {
                        ImdbId = result.Id,
                        Title = result.Title,
                        Url = $"{_imdbConfiguration.URI}{titleUri}{result.Id}",
                        Year = result.Year,
                        PosterPath = result.Poster?.ImageUrl ?? ""
                    });

            return media;
        }

        public async Task<Media> GetMedia(string url)
        {
            var htmlDoc = await _htmlUtils.GetHtmlDocAsync(url);
            return _htmlUtils.GetMediaInfo(htmlDoc, url);
        }

        public async Task<IEnumerable<Media>> GetUserMedia(IUser user)
        {
            var sqlQuery =
                "SELECT * FROM media AS M INNER JOIN \"user\" AS U ON U.discord_id = @UserDiscordId INNER JOIN user_media as UM ON U.id = UM.user_id";
            var media =
                await _connection.QueryAsync<Media>(sqlQuery, new {UserDiscordId = (long) user.Id});
            return media;
        }
    }
}