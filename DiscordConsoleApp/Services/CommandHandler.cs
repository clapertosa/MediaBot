using System;
using System.Data;
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
using Microsoft.Extensions.Logging;

namespace DiscordConsoleApp.Services
{
    public partial class CommandHandler : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly IDbConnection _connection;
        private readonly IImdbRepository _imdbRepository;
        private readonly IServiceProvider _provider;
        private readonly CommandService _service;

        public CommandHandler(
            IServiceProvider provider,
            ILogger<CommandHandler> logger,
            IConfiguration config,
            IDbConnection connection,
            DiscordSocketClient client,
            CommandService service,
            IImdbRepository imdbRepository) : base(client, logger)
        {
            _provider = provider;
            _connection = connection;
            _client = client;
            _service = service;
            _config = config;
            _imdbRepository = imdbRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _service.CommandExecuted += OnCommandExecuted;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage {Source: MessageSource.User} message) return;

            var argPos = 0;
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            var pos = 0;
            if (message.HasStringPrefix(_config["prefix"], ref pos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                var result = await _service.ExecuteAsync(context, pos, _provider);

                if (!result.IsSuccess)
                {
                    var reason = result.Error;
                    if (arg.Content.Contains("imdb")) await context.Channel.SendMessageAsync($"Error: \n{reason}");
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
                    var embedBuilder = await SendMedia(msg);
                    var mediaMsg = await channelSocket.SendMessageAsync(embed: embedBuilder.Build());
                    await mediaMsg.AddReactionAsync(new Emoji(EmojiUnicode.Heart));
                    break;
                case EmojiUnicode.Heart:
                    var user = _client.GetUser(emoteSocket.UserId);
                    var answer = await SaveMedia(msg, user);
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
                    var user = _client.GetUser(emoteSocket.UserId);
                    var answer = await RemoveMedia(msg, user);
                    await channelSocket.SendMessageAsync(answer);
                    break;
                default: return;
            }
        }

        private static async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context,
            IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}