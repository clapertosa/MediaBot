using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using DiscordConsoleApp.Commands;
using Microsoft.Extensions.Configuration;

namespace DiscordConsoleApp.Services
{
    public partial class CommandHandler : InitializedService
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
            _client.ReactionRemoved += OnReactionRemoved;
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

            // If message is not by bot or reaction added by bot
            if (!msg.Author.IsBot || emoteSocket.UserId == _client.CurrentUser.Id) return;

            // var users = await msg.GetReactionUsersAsync(new Emoji("✅"), Int32.MaxValue).FlattenAsync();
            switch (emoteSocket.Emote.Name)
            {
                case EmojiUnicode.Confirm:
                    EmbedBuilder embedBuilder = await SendMedia(msg);
                    var mediaMsg = await channelSocket.SendMessageAsync(embed: embedBuilder.Build());
                    await mediaMsg.AddReactionAsync(new Emoji(EmojiUnicode.Heart));
                    break;
                case EmojiUnicode.Heart:
                    SocketUser user = _client.GetUser(emoteSocket.UserId);
                    string answer = await SaveMedia(msg, user);
                    await channelSocket.SendMessageAsync(answer);
                    break;
                default: return;
            }
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channelSocket,
            SocketReaction emoteSocket)
        {
            var msg = await channelSocket.GetMessageAsync(emoteSocket.MessageId);

            switch (emoteSocket.Emote.Name)
            {
                case EmojiUnicode.Heart:
                    SocketUser user = _client.GetUser(emoteSocket.UserId);
                    string answer = await RemoveMedia(msg, user);
                    await channelSocket.SendMessageAsync(answer);
                    break;
                default: return;
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}