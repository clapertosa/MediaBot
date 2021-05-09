using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Domain.Entities;

namespace DiscordConsoleApp.Services
{
    public partial class CommandHandler
    {
        private async Task<EmbedBuilder> SendMedia(IMessage msg)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            foreach (var msgEmbed in msg.Embeds)
            {
                Media media = await _imdbRepository.GetMedia(msgEmbed.Url);
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

        private async Task<string> SaveMedia(IMessage msg, SocketUser user)
        {
            var firstOrDefault = msg.Embeds.FirstOrDefault();

            string storedProcedure = "SP_AddMedia";
            await using var sqlConnection =
                new SqlConnection(_config["Databases:DiscordConnectionString"] + "discord_imdbot");
            await using var sqlCommand = new SqlCommand(storedProcedure, sqlConnection)
                {CommandType = CommandType.StoredProcedure};

            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@UserId", SqlDbType.BigInt) {Value = user.Id},
                new SqlParameter("@MediaId", SqlDbType.VarChar)
                {
                    Value = firstOrDefault?.Url.Substring(
                        firstOrDefault.Url.LastIndexOf("/", StringComparison.Ordinal) + 1)
                },
                new SqlParameter("@MediaTitle", SqlDbType.NVarChar) {Value = firstOrDefault?.Title},
                new SqlParameter("@MediaPosterPath", SqlDbType.VarChar)
                    {Value = firstOrDefault?.Thumbnail.Value.Url ?? firstOrDefault?.Image.Value.Url ?? ""}
            };
            sqlCommand.Parameters.AddRange(parameters.ToArray());
            await sqlConnection.OpenAsync();
            await sqlCommand.ExecuteNonQueryAsync();

            return $"{user.Mention} {firstOrDefault?.Title} Added";
        }

        private async Task<string> RemoveMedia(IMessage msg, SocketUser user)
        {
            var firstOrDefault = msg.Embeds.FirstOrDefault();

            string storedProcedure = "SP_RemoveMedia";
            await using var sqlConnection =
                new SqlConnection(_config["Databases:DiscordConnectionString"] + "discord_imdbot");
            await using var sqlCommand = new SqlCommand(storedProcedure, sqlConnection)
                {CommandType = CommandType.StoredProcedure};

            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@UserId", SqlDbType.BigInt) {Value = user.Id},
                new SqlParameter("@MediaId", SqlDbType.VarChar)
                {
                    Value = firstOrDefault?.Url.Substring(
                        firstOrDefault.Url.LastIndexOf("/", StringComparison.Ordinal) + 1)
                }
            };
            sqlCommand.Parameters.AddRange(parameters.ToArray());
            await sqlConnection.OpenAsync();
            await sqlCommand.ExecuteNonQueryAsync();

            return $"{user.Mention} {firstOrDefault?.Title} removed";
        }
    }
}