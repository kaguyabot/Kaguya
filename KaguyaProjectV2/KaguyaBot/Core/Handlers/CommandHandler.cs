using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.TypeReaders;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public class CommandHandler
    {
        public static CommandService _commands;
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += HandleCommandLog;
        }

        public async Task InitializeAsync()
        {
            _commands.AddTypeReader(typeof(List<SocketGuildUser>), new ListSocketGuildUserTr());
            _commands.AddTypeReader(typeof(Emote), new EmoteTr());
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        }

        public async Task HandleCommandAsync(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message) || message.Author.IsBot) return;
            if (msg.Channel.GetType() != typeof(SocketTextChannel))
                return;

            Server server = await DatabaseQueries.GetOrCreateServerAsync(((SocketGuildChannel)message.Channel).Guild.Id);
            User user = await DatabaseQueries.GetOrCreateUserAsync(message.Author.Id);

            if (user.IsBlacklisted && user.UserId != ConfigProperties.BotConfig.BotOwnerId) return;
            if (server.IsBlacklisted) return;

            var context = new ShardedCommandContext(_client, message);
            await IsFilteredPhrase(context, server, message); // If filtered phrase (and user isn't admin), return.

            await ExperienceHandler.TryAddExp(user, server, context);
            await ServerSpecificExpHandler.TryAddExp(user, server, context);

            // If the channel is blacklisted and the user isn't an Admin, return.
            if (server.BlackListedChannels.Any(x => x.ChannelId == context.Channel.Id) &&
                !context.Guild.GetUser(context.User.Id).GuildPermissions.Administrator)
                return;

            if (Regex.IsMatch(msg.Content, @"http[Ss|\s]://osu.ppy.sh/beatmapsets/[0-9]*#\b(?:osu|taiko|mania|fruits)\b/[0-9]*") ||
               Regex.IsMatch(msg.Content, @"http[Ss|\s]://osu.ppy.sh/b/[0-9]*"))
                await AutomaticBeatmapLinkParserService.LinkParserMethod(msg, context);

            int argPos = 0;
            if (!(message.HasStringPrefix(server.CommandPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            //command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            Server server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);
            User user = await DatabaseQueries.GetOrCreateUserAsync(context.User.Id);

            if (result.IsSuccess)
            {
                server.TotalCommandCount++;
                user.ActiveRateLimit++;

                // Providing context as a parameter will automatically log all information about an executed command.
                await ConsoleLogger.LogAsync(context);

                await DatabaseQueries.InsertAsync(new CommandHistory
                {
                    Command = command.Value.Aliases[0],
                    Timestamp = DateTime.Now,
                    UserId = context.User.Id,
                    ServerId = context.Guild.Id
                });
                await DatabaseQueries.UpdateAsync(server);
                await DatabaseQueries.UpdateAsync(user);
                return;
            }

            await HandleCommandResult(context, server, result);
        }

        public async Task<bool> IsFilteredPhrase(ICommandContext context, Server server, IMessage message)
        {
            var userPerms = (await context.Guild.GetUserAsync(context.User.Id)).GuildPermissions;

            if (userPerms.Administrator)
                return false;

            var fp = server.FilteredPhrases.ToList();
            if (fp.Count == 0) return false;

            var phrases = fp.Select(element => element.Phrase).ToList();
            foreach (var phrase in phrases)
            {
                if (message.Content.ToLower().Contains(phrase.ToLower()))
                {
                    await context.Channel.DeleteMessageAsync(message);
                    await ConsoleLogger.LogAsync($"Filtered phrase detected: [Guild: {server.ServerId} | Phrase: {phrase}]", LogLvl.INFO);
                    return true;
                }
            }

            return false;
        }

        private static async Task HandleCommandResult(ICommandContext context, Server server, IResult result)
        {
            string cmdPrefix = server.CommandPrefix;
            await ConsoleLogger.LogAsync($"Command Failed [Command: {context.Message} | User: {context.User} | Guild: {context.Guild.Id}]", LogLvl.DEBUG);

            if (!result.IsSuccess)
            {
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
                {
                    Title = "Command Failed",
                    Description = $"Failed to execute command `{context.Message}` \nReason: {result.ErrorReason}\n",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Use {cmdPrefix}h <command> for information on how to use a command!",
                    },
                };
                embed.SetColor(EmbedColor.RED);

                try
                {
                    await context.Channel.SendEmbedAsync(embed);
                }
                catch (Discord.Net.HttpException e)
                {
                    await ConsoleLogger.LogAsync(
                        $"An exception was thrown when trying to send a command result into a text channel.\n" +
                        $"Channel: {context.Channel.Id} in guild {context.Guild.Id}\n" +
                        $"Message: {e.Message}\n" +
                        $"Stack Trace: {e.StackTrace}",
                        LogLvl.WARN);
                }
            }
        }

        private static async Task HandleCommandLog(LogMessage logMsg)
        {
            if (logMsg.Exception is CommandException cmdException)
            {
                await ConsoleLogger.LogAsync(logMsg, cmdException);
            }
        }
    }
}