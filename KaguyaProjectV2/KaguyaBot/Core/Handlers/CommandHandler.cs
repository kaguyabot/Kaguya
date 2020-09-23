using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services;
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
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
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

            // Never blacklist the bot owner.
            if (user.IsBlacklisted && user.UserId != ConfigProperties.BotConfig.BotOwnerId) return;
            if (server.IsBlacklisted) return;

            var context = new ShardedCommandContext(_client, message);
            
            if(await IsFilteredPhrase(context, server, message))
                return; // If filtered phrase (and user isn't admin), return.

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

        private static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            //command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            Server server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);
            User user = await DatabaseQueries.GetOrCreateUserAsync(context.User.Id);

            await HandleCommandResult(command, context, user, server, result);
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

        private static async Task HandleCommandResult(Optional<CommandInfo> command, ICommandContext context, User user, 
            Server server, IResult result)
        {
            string cmdPrefix = server.CommandPrefix;

            if (!result.IsSuccess)
            {
                await ConsoleLogger.LogAsync($"Command Failed [Command: {context.Message} | User: {context.User} | " +
                                             $"Guild: {context.Guild.Id}]", LogLvl.DEBUG);
            }

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
            }
            else
            {
                if (result is ExecuteResult executeResult && executeResult.Exception != null 
                && executeResult.Exception.GetType() == typeof(KaguyaSupportException))
                {
                    await DisplayKaguyaSupportException(context, result, cmdPrefix);
                }
                else
                {
                    // The command error isn't a KaguyaSupportException, so throw a generic error message.
                    await DisplayGenericErrorEmbed(command, context, result, cmdPrefix);
                }
            }
        }

        /// <summary>
        /// Creates and sends an error message to the user upon failed command.
        /// </summary>
        private static async Task DisplayGenericErrorEmbed(Optional<CommandInfo> command, ICommandContext context, IResult result, string cmdPrefix)
        {
            string cmdName = command.IsSpecified ? command.Value.Name.ToLower() : "<command>";
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder(EmbedColor.RED)
            {
                Title = "Error",
                Description = $"Command: `{context.Message}`\nReason: {result.ErrorReason}\n\n" +
                              $"Help: Use `{cmdPrefix}h {cmdName}` to learn how to use this command.",
            };

            try
            {
                await context.Channel.SendEmbedAsync(embed);
            }
            catch (Discord.Net.HttpException e)
            {
                bool logLeave = true;
                if (e.DiscordCode.HasValue && e.DiscordCode == 50013) // missing permissions
                {
                    if (context.Guild.Id == 264445053596991498)
                        return;
                    
                    IGuildUser owner = await context.Guild.GetOwnerAsync();
                    var missPermEmbed = new KaguyaEmbedBuilder(EmbedColor.ORANGE)
                    {
                        Description = "__**Error: Missing Permissions**__\n\n" +
                                      $"I have left `{context.Guild.Name}` because I did not have " +
                                      $"permission to send messages in your server!\n\n" +
                                      $"You may add me back to your server [here.](https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=469101694)"
                    };

                    try
                    {
                        await owner.SendMessageAsync(embed: missPermEmbed.Build());
                    }
                    catch (Exception ex)
                    {
                        await ConsoleLogger.LogAsync(ex, $"Failed to DM owner of guild {context.Guild.Id} " +
                                                         $"as I was leaving the server [ID: {context.Guild.Id} | " +
                                                         $"Name: {context.Guild.Name}].", LogLvl.WARN);
                    }
                    finally
                    {
                        await context.Guild.LeaveAsync();
                        await ConsoleLogger.LogAsync($"Self-ejected from guild {context.Guild.Id} due to missing " +
                                                     $"permissions (Discord Error: 50013)", LogLvl.INFO);
                        logLeave = false;
                    }
                }

                if (logLeave)
                {
                    await ConsoleLogger.LogAsync(
                        $"An exception was thrown when trying to send a command error notification into a text channel.\n" +
                        $"Channel: {context.Channel.Id} in guild {context.Guild.Id}\n" +
                        $"User Message: {context.Message}\n" +
                        $"Exception Message: {e.Message}\n" +
                        $"Stack Trace: {e.StackTrace}",
                        LogLvl.WARN);
                }
            }
        }

        private static async Task DisplayKaguyaSupportException(ICommandContext context, IResult result, string cmdPrefix)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder(EmbedColor.RED)
            {
                Title = "Command Failed",
                Description = $"Failed to execute command `{context.Message}`\n\nReason: `{result.ErrorReason}`\n\n" +
                              $"[Kaguya Support](https://discord.gg/aumCJhr)\n" +
                              $"[Report a bug](https://github.com/stageosu/Kaguya/issues/new?assignees=&labels=Bug&template=bug-report.md&title=)",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Use {cmdPrefix}h <command> for information on how to use a command!",
                }
            };

            try
            {
                await context.Channel.SendEmbedAsync(embed);
            }
            catch (Discord.Net.HttpException e)
            {
                await ConsoleLogger.LogAsync(
                    $"An exception was thrown when trying to send a command error notification into a text channel.\n" +
                    $"Channel: {context.Channel.Id} in guild {context.Guild.Id}\n" +
                    $"User Message: {context.Message}\n" +
                    $"Exception Message: {e.Message}\n" +
                    $"Stack Trace: {e.StackTrace}",
                    LogLvl.WARN);
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