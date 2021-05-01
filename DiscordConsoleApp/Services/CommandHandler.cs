using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace DiscordConsoleApp.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly IImdbRepository _imdbRepository;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config, IImdbRepository imdbRepository)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _imdbRepository = imdbRepository;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
            _service.CommandExecuted += OnCommandExecuted;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            int pos = 0;
            if (message.HasStringPrefix(_config["prefix"], ref pos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                var result = await _service.ExecuteAsync(context, pos, _provider);

                if (!result.IsSuccess)
                {
                    var reason = result.Error;
                    if (arg.Content.Contains("imdb"))
                    {
                        await context.Channel.SendMessageAsync($"Error: \n{reason}");
                    }
                }
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channelSocket,
            SocketReaction emoteSocket)
        {
            var msg = await channelSocket.GetMessageAsync(emoteSocket.MessageId);
            // Emote must be a check
            if (emoteSocket.Emote.Name != "✅") return;
            // If reaction added by bot, return
            if (emoteSocket.UserId == _client.CurrentUser.Id) return;
            foreach (var msgEmbed in msg.Embeds)
            {
                Media media = await _imdbRepository.GetMedia(msgEmbed.Url);
                EmbedBuilder embedBuilder = new EmbedBuilder
                {
                    Title = media.Title,
                    Description = media.Plot,
                    ThumbnailUrl = media.PosterPath,
                    Url = media.Url
                };
                embedBuilder.AddField("Meta", string.Join(", ", media.MetaData));
                embedBuilder.AddField("Genres", string.Join(", ", media.Genres));
                embedBuilder.AddField("Director", $"[{media.Director?.FullName?? "-"}]({media?.Director?.Url??""})");
                embedBuilder.AddField("Actors",
                    string.Join(", ", media.Actors.Select(x => $"[{x.FullName}]({x.Url})")));
                embedBuilder.AddField("Vote", $"{media.Vote}/10 ({media.VotesNumber})");
                embedBuilder.AddField("Release date", media.ReleaseDate);

                await channelSocket.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}