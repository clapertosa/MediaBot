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
            // do operations

            return $"{user.Mention} saved or not";
        }
    }
}