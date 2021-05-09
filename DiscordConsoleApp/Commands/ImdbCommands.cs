using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Discord;
using Discord.Commands;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace DiscordConsoleApp.Commands
{
    public class ImdbCommands : ModuleBase
    {
        private readonly IImdbRepository _imdbRepository;
        private readonly IConfiguration _configuration;

        public ImdbCommands(IImdbRepository imdbRepository, IConfiguration configuration)
        {
            _imdbRepository = imdbRepository;
            _configuration = configuration;
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
            var media = await _imdbRepository.Search(title);
            if (media.Count <= 0) await ReplyAsync("No results found.");

            if (media.Count > 0)
            {
                EmbedBuilder embeded = new EmbedBuilder();
                await ReplyAsync("Please select one:");
                int limit = media.Count > 3 ? 3 : media.Count;
                for (short i = 0; i < limit; i++)
                {
                    embeded.Title = media[i].Title;
                    embeded.ThumbnailUrl = media[i].PosterPath;
                    embeded.Url = media[i].Url;
                    var msg = await ReplyAsync("", false, embeded.Build());
                    await msg.AddReactionsAsync(new IEmote[]
                        {new Emoji(EmojiUnicode.Confirm), new Emoji(EmojiUnicode.Heart)});
                }
            }
        }

        [Command(CommandType.GetUserMedia)]
        public async Task GetUserMedia()
        {
            List<Media> media =
                await _imdbRepository.GetUserMedia(Context.User.Id,
                    _configuration["Databases:DiscordConnectionString"]);

            await ReplyAsync($"{Context.User.Mention} Favorites");
            foreach (Media m in media)
            {
                EmbedBuilder embedBuilder = new EmbedBuilder();
                embedBuilder.Title = m.Title;
                embedBuilder.Url = m.Url;
                embedBuilder.ThumbnailUrl = m.PosterPath;
                await ReplyAsync(null, false, embedBuilder.Build());
            }
        }
    }
}