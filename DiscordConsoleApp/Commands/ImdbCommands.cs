using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Discord;
using Discord.Commands;
using Domain.Entities;

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
            await ReplyAsync("ecco la lista dei comanid");
        }

        [Command(CommandType.Search)]
        public async Task Search([Remainder] string title)
        {
            var media = await _imdbRepository.Search(title);
            if (media.Count <= 0) await ReplyAsync("No results found.");

            if (media.Count > 1)
            {
                EmbedBuilder embeded = new EmbedBuilder();
                await ReplyAsync("Please select one:");
                foreach (Media m in media)
                {
                    embeded.Title = m.Title;
                    embeded.ThumbnailUrl = m.PosterPath;
                    embeded.Url = m.Url;
                    var msg = await ReplyAsync("", false, embeded.Build());
                    await msg.AddReactionAsync(new Emoji(EmojiUnicode.Confirm));
                    await msg.AddReactionAsync(new Emoji(EmojiUnicode.Heart));
                }
            } else if (media.Count == 1)
            {
                // return all the media info
            }
        }
        
    }
}