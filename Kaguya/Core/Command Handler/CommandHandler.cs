using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler;
using Kaguya.Core.Command_Handler.LogMethods;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.LevelingSystem;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Victoria;

#pragma warning disable

namespace Kaguya
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _commands;
        LavaSocketClient _lavaSocketClient;
        private IServiceProvider _services;
        readonly Color Yellow = new Color(255, 255, 102);
        readonly Color SkyBlue = new Color(63, 242, 255);
        readonly Color Red = new Color(255, 0, 0);
        readonly Color Violet = new Color(238, 130, 238);
        readonly Color Pink = new Color(252, 132, 255);
        readonly KaguyaLogMethods logger = new KaguyaLogMethods();
        readonly MusicLogMethods musicLogger = new MusicLogMethods();
        readonly Timers timers = new Timers();
        readonly Logger consoleLogger = new Logger();
        public string osuApiKey = Config.bot.OsuApiKey;
        public string tillerinoApiKey = Config.bot.TillerinoApiKey;

        public CommandHandler(IServiceProvider provider)
        {
            this._services = provider;
            _client = this._services.GetService<DiscordSocketClient>();
            _commands = new CommandService();
        }

        public async Task InitializeAsync()
        {
            try
            {
                _client = Global.Client;
                _lavaSocketClient = Global.lavaSocketClient;

                _commands = new CommandService();
                _commands.AddTypeReader(typeof(List<SocketGuildUser>), new ListSocketGuildUserTR());
                await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

                _client.Connected += logger.ClientConnected;
                _client.Ready += logger.OnReady;
                _client.Ready += timers.CheckChannelPermissions;
                _client.Ready += timers.ServerInformationUpdate;
                _client.Ready += timers.GameTimer;
                _client.Ready += timers.VerifyMessageReceived;
                _client.Ready += timers.ServerMessageLogCheck;
                _client.Ready += timers.VerifyUsers;
                _client.Ready += timers.ResourcesBackup;
                _client.Ready += timers.LogFileTimer;

                _lavaSocketClient.Log += musicLogger.MusicLogger;
                _lavaSocketClient.OnTrackFinished += musicLogger.OnTrackFinished;
                _lavaSocketClient.OnTrackException += musicLogger.OnTrackException;

                _client.MessageReceived += HandleCommandAsync;
                _client.MessageReceived += logger.osuLinkParser;
                _client.JoinedGuild += logger.JoinedNewGuild;
                _client.LeftGuild += logger.LeftGuild;
                _client.MessageReceived += logger.MessageCache;
                _client.MessageDeleted += logger.LoggingDeletedMessages;
                _client.MessageUpdated += logger.LoggingEditedMessages;
                _client.UserJoined += logger.LoggingUserJoins;
                _client.UserLeft += logger.LoggingUserLeaves;
                _client.UserBanned += logger.LoggingUserBanned;
                _client.UserUnbanned += logger.LoggingUserUnbanned;
                _client.MessageReceived += logger.LogChangesToLogSettings;
                _client.MessageReceived += logger.UserSaysFilteredPhrase;
                _client.UserVoiceStateUpdated += logger.UserConnectsToVoice;
                _client.Disconnected += logger.ClientDisconnected;
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
                return;
            }
            catch (Exception e)
            {
                consoleLogger.ConsoleCriticalAdvisory(e, "InitializeAsync method threw an exception. CommandHandler.cs line 105.");
                return;
            }
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var user = msg.Author as SocketGuildUser;
            if (msg is null || user is null || user.IsBot) return;

            var guild = Servers.GetServer(user.Guild);
            var userAccount = UserAccounts.GetAccount(user);

            if (userAccount.Blacklisted == 1) { return; }
            if (guild.IsBlacklisted) { return; }


            var context = new SocketCommandContext(_client, msg);

            foreach (string phrase in guild.FilteredWords)
            {
                if (msg.Content.Contains(phrase))
                {
                    await logger.UserSaysFilteredPhrase(msg);
                    consoleLogger.ConsoleCommandLog(context);
                }
            }

            Leveling.UserSentMessage(user, msg.Channel as SocketTextChannel);

            string oldUsername = userAccount.Username;
            string newUsername = context.User.Username;

            if ($"{oldUsername}" != $"{newUsername}#{user.Discriminator}")
                userAccount.Username = $"{newUsername}#{user.Discriminator}";

            int argPos = 0;

            if (!msg.HasStringPrefix(guild.commandPrefix, ref argPos) && !msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            var embed = new EmbedBuilder();
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.UnknownCommand:
                        embed.WithDescription($"**Error: The command `{context.Message.Content}` does not exist!**");
                        embed.WithFooter($"Use {guild.commandPrefix}h for the full commands list! Tag me with \"prefix <symbol>\" to edit my prefix!");
                        embed.WithColor(Red);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.UnknownCommand, $"The command {context.Message.Content} does not exist!");
                        break;
                    case CommandError.BadArgCount:
                        var cmdPrefix = Servers.GetServer(context.Guild).commandPrefix;
                        embed.WithDescription("**Error: I need a different set of information than what you've given me!**");
                        embed.WithFooter($"Use {cmdPrefix}h <command> to see the proper usage.");
                        embed.WithColor(Red);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, "User attempted to use invalid parameters for a command.");
                        break;
                    case CommandError.ParseFailed:
                        embed.WithDescription("**Error: I failed to parse a specified value!**");
                        embed.WithFooter($"You may be using text instead of a number. Review {guild.commandPrefix}h <command> for the proper usage!");
                        embed.WithColor(Red);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, "Failed to parse given value specified in command.");
                        break;
                    case CommandError.UnmetPrecondition:
                        embed.WithDescription($"**Error: {result.ErrorReason}**");
                        embed.WithFooter("Review $h <command> for the proper usage!");
                        embed.WithColor(Red);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, $"{result.ErrorReason}");
                        break;
                    case CommandError.MultipleMatches:
                        embed.WithDescription("**Error: I found multiple matches for the task you were trying to execute!**");
                        embed.WithFooter($"Review {guild.commandPrefix}h <command> for the proper usage! I can only do one thing at a time!");
                        embed.WithColor(Red);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, "Multiple matches found.");
                        break;
                    default:
                        embed.WithDescription("**Error: I failed to execute this command for an unknown reason.**");
                        embed.WithFooter($"Error reason: {result.ErrorReason}");
                        embed.WithColor(Red);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.Unsuccessful, $"{result.ErrorReason}");
                        break;
                }
            }
        }

        private Task MusicLogger(LogMessage msg)
        {
            consoleLogger.ConsoleMusicLog(msg);
            return Task.CompletedTask;
        }
    }
}
