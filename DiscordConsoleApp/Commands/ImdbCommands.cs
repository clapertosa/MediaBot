using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Discord;
using Discord.Commands;

namespace DiscordConsoleApp.Commands
{
    public class ImdbCommands : ModuleBase
    {
        private readonly IImdbRepository _imdbRepository;

        public ImdbCommands(IImdbRepository imdbRepository)
        {
            _imdbRepository = imdbRepository;
        }

        [Command(CommandType.Commands)]
        [Alias(CommandType.Help)]
        public async Task CommandsList()
        {
            await ReplyAsync($"{Context.User.Mention} Here's commands list");
            await ReplyAsync($"!{CommandType.Search}");
            await ReplyAsync($"!{CommandType.GetUserMedia}");
        }

        [Command(CommandType.Search)]
        public async Task Search([Remainder] string title)
        {
            var media = (await _imdbRepository.Search(title)).ToList();
            if (!media.Any()) await ReplyAsync("No results found.");

            if (media.Any())
            {
                var embedded = new EmbedBuilder();
                await ReplyAsync("Please select one:");
                var limit = media.Count > 5 ? 5 : media.Count;
                for (short i = 0; i < limit; i++)
                {
                    embedded.Title = media[i].Title;
                    embedded.ThumbnailUrl = media[i].PosterPath;
                    embedded.Url = media[i].Url;
                    var msg = await ReplyAsync("", false, embedded.Build());
                    await msg.AddReactionsAsync(new IEmote[]
                        {new Emoji(EmojiUnicode.Confirm), new Emoji(EmojiUnicode.Heart)});
                }
            }
        }

        [Command(CommandType.GetUserMedia)]
        public async Task GetUserMedia()
        {
            var media =
                await _imdbRepository.GetUserMedia(Context.User);

            await ReplyAsync($"{Context.User.Mention} Favorites");
            foreach (var m in media)
            {
                var embedBuilder = new EmbedBuilder();
                embedBuilder.Title = m.Title;
                embedBuilder.Url = m.Url;
                embedBuilder.ThumbnailUrl = m.PosterPath;
                await ReplyAsync(null, false, embedBuilder.Build());
            }
        }
    }
}