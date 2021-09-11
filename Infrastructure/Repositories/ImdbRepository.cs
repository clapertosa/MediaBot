using System;
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

        private readonly string _titleUri = "title/";
        // private readonly string _searchUri = "find?q=";

        public ImdbRepository(IHtmlUtils htmlUtils, IDbConnection connection, IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _htmlUtils = htmlUtils;
            _connection = connection;
            _client = httpClientFactory.CreateClient("imdb-client");
            _imdbConfiguration = configuration.GetSection("IMDB").Get<ImdbConfiguration>();
        }

        private bool HaveSameFieldsValues(object firstEntity, object secondEntity, string[] properties)
        {
            bool haveSameFieldsValues = true;

            foreach (string property in properties)
            {
                var firstValue = firstEntity.GetType().GetProperty(property)?.GetValue(firstEntity);
                var secondValue = secondEntity.GetType().GetProperty(property)?.GetValue(secondEntity);

                if (!Equals(firstValue, secondValue))
                {
                    haveSameFieldsValues = false;
                    break;
                }
            }

            return haveSameFieldsValues;
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
                    if (result.Type != null)
                    {
                        media.Add(new Media
                        {
                            ImdbId = result.Id,
                            Title = result.Title,
                            Url = $"{_imdbConfiguration.URI}{_titleUri}{result.Id}",
                            Year = result.Year,
                            PosterPath = result.Poster?.ImageUrl ?? ""
                        });
                    }

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
                "SELECT * FROM media AS M INNER JOIN user_media AS UM ON UM.media_id = M.id INNER JOIN \"user\" AS U ON U.id = UM.user_id WHERE U.discord_id = @UserDiscordId";
            var media =
                await _connection.QueryAsync<Media>(sqlQuery, new {UserDiscordId = (long) user.Id});
            return media;
        }

        public async Task UpdateUser(User oldUser, IUser newUser)
        {
            User discordUser = new User
            {
                Username = newUser.Username,
                Discriminator = newUser.Discriminator,
                IsBot = newUser.IsBot,
                PublicFlags = (int) newUser.PublicFlags
            };

            if (HaveSameFieldsValues(oldUser, discordUser,
                new[] {"Username", "Discriminator", "IsBot", "PublicFlags"})) return;

            string sqlUpdateUserQuery =
                "UPDATE \"user\" SET username = @Username, discriminator = @Discriminator, is_bot = @IsBot, public_flags = @PublicFlags, updated_at = @UpdatedAt WHERE discord_id = @DiscordId";
            await _connection.ExecuteAsync(sqlUpdateUserQuery, new
            {
                DiscordId = (long) newUser.Id,
                Username = newUser.Username,
                Discriminator = newUser.Discriminator,
                IsBot = newUser.IsBot,
                PublicFlags = (int) newUser.PublicFlags,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public async Task UpdateMedia(Media oldMedia, IEmbed embed)
        {
            string mediaId = embed.Url.Substring(embed.Url.LastIndexOf('/') + 1);
            Media newMedia = new Media
            {
                ImdbId = mediaId,
                Title = embed.Title,
                PosterPath = embed.Thumbnail.Value.Url ?? "",
                Url = embed.Url
            };

            if (HaveSameFieldsValues(oldMedia, newMedia, new[] {"ImdbId", "Title", "PosterPath", "Url"})) return;

            string sqlUpdateMediaQuery =
                "UPDATE media SET imdb_id = @ImdbId, title = @Title, poster_path = @PosterPath, url = @Url, updated_at = @UpdatedAt  WHERE Id = @MediaRecordId";
            await _connection.ExecuteAsync(sqlUpdateMediaQuery, new
            {
                MediaRecordId = oldMedia.Id,
                ImdbId = mediaId,
                Title = newMedia.Title,
                PosterPath = newMedia.PosterPath,
                Url = newMedia.Url,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}