using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
            if (message == null || message.Author.IsBot) return;

            Server server = ServerQueries.GetServer((message.Channel as SocketGuildChannel).Guild.Id);
            User user = Users.GetUser(message.Author.Id);

            int argPos = 0;

            var context = new ShardedCommandContext(_client, message);
            await IsFilteredPhrase(context, server, message);

            ExperienceHandler.AddEXP(user, context);

            if (!(message.HasStringPrefix(server.CommandPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            //command is unspecified when there was a search failure(command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            Server server = ServerQueries.GetServer(context.Guild.Id);

            if (result.IsSuccess)
            {
                await Logger.Log($"Command Executed [Command: {context.Message} | User: {context.User} | Channel: {context.Channel} | " +
                    $"Guild: {context.Guild}]", LogLevel.INFO);
                return;
            }

            HandleCommandResult(context, server, result);
        }

        public async Task<bool> IsFilteredPhrase(ICommandContext context, Server server, IMessage message)
        {
            var userPerms = (await context.Guild.GetUserAsync(context.User.Id)).GuildPermissions;

            if (userPerms.Administrator)
                return false;

            List<FilteredPhrase> fp = ServerQueries.GetAllFilteredPhrasesForServer(server.Id) ?? new List<FilteredPhrase>();

            if (fp.Count == 0) return false;

            List<string> phrases = new List<string>();

            foreach (var element in fp) { phrases.Add(element.Phrase); }
            foreach(var phrase in phrases)
            {
                if (message.Content.ToLower().Contains(phrase.ToLower()))
                {
                    await context.Channel.DeleteMessageAsync(message);
                    await Logger.Log($"Filtered phrase detected: [Guild: {server.Id} | Phrase: {phrase}]", LogLevel.INFO);
                    return true;
                }
            }

            return false;
        }

        private async void HandleCommandResult(ICommandContext context, Server server, IResult result)
        {
            string cmdPrefix = server.CommandPrefix;
            await Logger.Log($"Command Failed [Command: {context.Message} | User: {context.User} | Guild: {context.Guild.Id}]", LogLevel.TRACE);

            if (!result.IsSuccess)
            {
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
                {
                    Title = "Command Failed",
                    Description = $"Failed to execute command `{context.Message}` \nReason: {result.ErrorReason}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Use {cmdPrefix}h <command> for information on how to use a command!",
                    },
                };
                embed.SetColor(EmbedColor.RED);

                await context.Channel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}