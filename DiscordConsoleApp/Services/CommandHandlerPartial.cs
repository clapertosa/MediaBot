using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Discord;
using Discord.WebSocket;
using Domain.Entities;

namespace DiscordConsoleApp.Services
{
    public partial class CommandHandler
    {
        private async Task<EmbedBuilder> SendMedia(IMessage msg)
        {
            var embedBuilder = new EmbedBuilder();
            foreach (var msgEmbed in msg.Embeds)
            {
                var media = await _imdbRepository.GetMedia(msgEmbed.Url);
                embedBuilder = new EmbedBuilder
                {
                    Title = media.Title,
                    Description = media.Plot,
                    ThumbnailUrl = media.PosterPath,
                    Url = media.Url
                };
                embedBuilder.AddField("Meta", string.Join(", ", media.MetaData));
                embedBuilder.AddField("Genres", string.Join(", ", media.Genres));
                embedBuilder.AddField("Director", $"[{media.Director?.FullName ?? "-"}]({media?.Director?.Url ?? ""})");
                embedBuilder.AddField("Actors",
                    string.Join(", ", media.Actors.Select(x => $"[{x.FullName}]({x.Url})")));
                embedBuilder.AddField("Vote", $"{media.Vote}/10 ({media.VotesNumber})");
                embedBuilder.AddField("Release date", media.ReleaseDate);
            }

            return embedBuilder;
        }

        private async Task<User> GetUser(ulong userDiscordId)
        {
            var sqlGetUserQuery = "SELECT * FROM \"user\" AS U WHERE U.discord_id = @UserDiscordId";
            return await _connection.QueryFirstOrDefaultAsync<User>(sqlGetUserQuery,
                new {UserDiscordId = (long) userDiscordId});
        }

        private async Task<Media> GetMedia(IEmbed embed)
        {
            var mediaId = embed?.Url.Substring(
                embed.Url.LastIndexOf("/", StringComparison.Ordinal) + 1);
            var sqlGetMediaQuery = "SELECT * FROM media AS M WHERE M.imdb_id = @ImdbId";
            return await _connection.QueryFirstOrDefaultAsync<Media>(sqlGetMediaQuery,
                new
                {
                    ImdbId = mediaId
                });
        }

        private async Task<string> SaveMedia(IMessage msg, SocketUser user)
        {
            var embed = msg.Embeds.FirstOrDefault();

            var userRecord = await GetUser(user.Id);

            // Check if user exists
            if (userRecord == null)
            {
                var sqlInsertUserQuery =
                    "INSERT INTO \"user\"(discord_id, username, discriminator, is_bot, public_flags) VALUES(@DiscordId, @Username, @Discriminator, @IsBot, @PublicFlags) RETURNING *";
                userRecord = await _connection.QueryFirstAsync<User>(sqlInsertUserQuery, new
                    {
                        DiscordId = (long) user.Id,
                        user.Username,
                        user.Discriminator,
                        user.IsBot,
                        user.PublicFlags
                    }
                );
            }
            else
            {
                await _imdbRepository.UpdateUser(userRecord, user);
            }

            // Check if media exists
            var mediaRecord = await GetMedia(embed);

            if (mediaRecord == null)
            {
                var sqlInsertMediaQuery =
                    "INSERT INTO media(imdb_id, title, poster_path, url) VALUES(@MediaId, @Title, @PosterPath, @Url) RETURNING *";
                mediaRecord = await _connection.QueryFirstAsync<Media>(sqlInsertMediaQuery, new
                {
                    MediaId = embed?.Url.Substring(embed.Url.LastIndexOf("/", StringComparison.Ordinal) + 1),
                    embed?.Title,
                    PosterPath = embed?.Thumbnail.Value.Url ?? embed?.Image.Value.Url ?? "",
                    embed?.Url
                });
            }
            else
            {
                await _imdbRepository.UpdateMedia(mediaRecord, embed);
            }

            // Check if user already has media
            var sqlCountUserMediaQuery =
                "SELECT COUNT(*) FROM user_media WHERE user_id = @UserId AND media_id = @MediaId";
            var mediaAlreadySaved = await _connection.QueryFirstAsync<int>(sqlCountUserMediaQuery, new
            {
                UserId = userRecord.Id,
                MediaId = mediaRecord.Id
            }) > 0;

            if (!mediaAlreadySaved)
            {
                var sqlInsertMediaUserQuery = "INSERT INTO user_media(user_id, media_id) VALUES(@UserId, @MediaId)";
                await _connection.ExecuteAsync(sqlInsertMediaUserQuery,
                    new {UserId = userRecord.Id, MediaId = mediaRecord.Id});
            }

            return $"{user.Mention} {embed?.Title} Added";
        }

        private async Task<string> RemoveMedia(IMessage msg, SocketUser user)
        {
            var embed = msg.Embeds.FirstOrDefault();

            var userRecord = await GetUser(user.Id);
            var mediaRecord = await GetMedia(embed);

            var sqlRemoveUserMediaQuery = "DELETE FROM user_media WHERE user_id = @UserId AND media_id = @MediaId";
            if (userRecord != null && mediaRecord != null)
                await _connection.ExecuteAsync(sqlRemoveUserMediaQuery,
                    new {UserId = (long) userRecord.Id, MediaId = mediaRecord.Id});

            return $"{user.Mention} {embed?.Title} removed";
        }
    }
}