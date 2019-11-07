using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += CommandExecutedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        }

        public async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('$', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client.GetShardFor((message.Channel as SocketGuildChannel).Guild), message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<Discord.Commands.CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
            {
                await Logger.Logger.Log($"Command Executed [Command: {context.Message} | User: {context.User} | Channel: {context.Channel} | " +
                    $"Guild: {context.Guild}]", DataStorage.JsonStorage.LogLevel.INFO);
                return;
            }
        }
    }
}