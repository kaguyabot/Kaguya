using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.Commands;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Kaguya.Modules.Administration
{
    public class LoggingCommands : InteractiveBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public Color Violet = new Color(127, 0, 255);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.Token;
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();
        readonly DiscordShardedClient _client = Global.Client;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("setlogchannel")] //administration
        [Alias("log")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task SetLogChannel(string logType, IGuildChannel channel)
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);
            ulong logChannelID = channel.Id;

            switch (logType.ToLower())
            {
                case "deletedmessages":
                    server.LogDeletedMessages = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Deleted Messages` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "updatedmessages":
                    server.LogUpdatedMessages = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Edited Messages` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userjoins":
                    server.LogWhenUserJoins = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `User Joins` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userleaves":
                    server.LogWhenUserLeaves = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `User Leaves` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "bans":
                    server.LogBans = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Bans` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "unbans":
                    server.LogUnbans = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Unbans` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "changestologsettings":
                    server.LogChangesToLogSettings = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `changes to log settings` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filteredphrases":
                    server.LogWhenUserSaysFilteredPhrase = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Filtered Phrases` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userconnectstovoice":
                    server.LogWhenUserConnectsToVoiceChannel = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `user connected to voice` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userdisconnectsfromvoice":
                    server.LogWhenUserDisconnectsFromVoiceChannel = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `user disconnected from voice` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "levelannouncements":
                    server.LogLevelUpAnnouncements = logChannelID;
                    Servers.SaveServers();
                    await GlobalCommandResponses.CreateCommandResponse(Context,
                        stopWatch.ElapsedMilliseconds,
                        "Log Channel Set",
                        $"{Context.User.Mention} All log messages for `level announcements` will be sent in channel {channel.Name}");
                    break;
                case "shadowbans":
                    server.LogShadowbans = logChannelID;
                    Servers.SaveServers();
                    await GlobalCommandResponses.CreateCommandResponse(Context,
                        stopWatch.ElapsedMilliseconds,
                        "Log Channel Set",
                        $"{Context.User.Mention} All log messages for `shadowbans` will be sent in channel {channel.Name}");
                    break;
                case "unshadowbans":
                    server.LogUnShadowbans = logChannelID;
                    Servers.SaveServers();
                    await GlobalCommandResponses.CreateCommandResponse(Context,
                        stopWatch.ElapsedMilliseconds,
                        "Log Channel Set",
                        $"{Context.User.Mention} All log messages for `un-shadowbans` will be sent in channel {channel.Name}");
                    break;
                case "all":
                    server.LogDeletedMessages = logChannelID;
                    server.LogUpdatedMessages = logChannelID;
                    server.LogWhenUserJoins = logChannelID;
                    server.LogWhenUserLeaves = logChannelID;
                    server.LogBans = logChannelID;
                    server.LogUnbans = logChannelID;
                    server.LogChangesToLogSettings = logChannelID;
                    server.LogWhenUserSaysFilteredPhrase = logChannelID;
                    server.LogWhenUserConnectsToVoiceChannel = logChannelID;
                    server.LogWhenUserDisconnectsFromVoiceChannel = logChannelID;
                    server.LogLevelUpAnnouncements = logChannelID;
                    server.LogShadowbans = logChannelID;
                    server.LogUnShadowbans = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                default:
                    embed.WithTitle("Invalid Log Specification");
                    embed.WithDescription($"{Context.User.Mention} Invalid logging type!");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
            }
        }

        [Command("resetlogchannel")] //administration
        [Alias("rlog")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task ResetLogChannel(string logType)
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);

            switch (logType.ToLower())
            {
                case "deletedmessages":
                    server.LogDeletedMessages = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Deleted Messages` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "updatedmessages":
                    server.LogUpdatedMessages = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Updated Messages` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userjoins":
                    server.LogWhenUserJoins = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `User Joins` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userleaves":
                    server.LogWhenUserLeaves = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `User Leaves` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "bans":
                    server.LogBans = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Bans` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "unbans":
                    server.LogUnbans = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Unbans` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "changestologsettings":
                    server.LogChangesToLogSettings = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `changes to log settings` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filteredphrases":
                    server.LogWhenUserSaysFilteredPhrase = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Filtered Phrases` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userconnectstovoice":
                    server.LogWhenUserConnectsToVoiceChannel = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `user connects to voice` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userdisconnectsfromvoice":
                    server.LogWhenUserDisconnectsFromVoiceChannel = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `user disconnects from voice` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "levelannouncements":
                    server.LogLevelUpAnnouncements = 0;
                    Servers.SaveServers();
                    await GlobalCommandResponses.CreateCommandResponse(Context,
                        stopWatch.ElapsedMilliseconds,
                        "Log Channel Set",
                        $"{Context.User.Mention} Log messages for `level announcements` have been disabled.");
                    break;
                case "shadowbans":
                    server.LogShadowbans = 0;
                    Servers.SaveServers();
                    await GlobalCommandResponses.CreateCommandResponse(Context,
                        stopWatch.ElapsedMilliseconds,
                        "Log Channel Set",
                        $"{Context.User.Mention} Log messages for `shadowbans` have been disabled.");
                    break;
                case "unshadowbans":
                    server.LogUnShadowbans = 0;
                    Servers.SaveServers();
                    await GlobalCommandResponses.CreateCommandResponse(Context,
                        stopWatch.ElapsedMilliseconds,
                        "Log Channel Set",
                        $"{Context.User.Mention} Log messages for `un-shadowbans` have been disabled.");
                    break;
                case "all":
                    server.LogDeletedMessages = 0;
                    server.LogUpdatedMessages = 0;
                    server.LogWhenUserJoins = 0;
                    server.LogWhenUserLeaves = 0;
                    server.LogBans = 0;
                    server.LogUnbans = 0;
                    server.LogChangesToLogSettings = 0;
                    server.LogWhenUserSaysFilteredPhrase = 0;
                    server.LogWhenUserConnectsToVoiceChannel = 0;
                    server.LogWhenUserDisconnectsFromVoiceChannel = 0;
                    server.LogLevelUpAnnouncements = 0;
                    server.LogShadowbans = 0;
                    server.LogUnShadowbans = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Reset");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `everything` have been disabled.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                default:
                    embed.WithTitle("Invalid Log Specification");
                    embed.WithDescription($"{Context.User.Mention} Please specify a valid logging type!");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
            }
        }

        [Command("logtypes")] //administration
        [Alias("loglist")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task LogList()
        {
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            var server = Servers.GetServer(Context.Guild);
            ulong[] logChannels = { server.LogDeletedMessages, server.LogUpdatedMessages, server.LogWhenUserJoins, server.LogWhenUserLeaves,
            server.LogBans, server.LogUnbans, server.LogChangesToLogSettings, server.LogWhenUserSaysFilteredPhrase,
            server.LogWhenUserConnectsToVoiceChannel, server.LogWhenUserDisconnectsFromVoiceChannel, server.LogLevelUpAnnouncements, server.LogShadowbans,
            server.LogUnShadowbans};
            embed.WithTitle("List of Log Events");
            embed.WithDescription($"List of all types of logging you can subscribe to. Use these with {cmdPrefix}log to enable logging!" +
                "\n" +
                $"\n**DeletedMessages** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[0])}`**" +
                $"\n**UpdatedMessages** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[1])}`**" +
                $"\n**UserJoins** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[2])}`**" +
                $"\n**UserLeaves** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[3])}`**" +
                $"\n**Bans** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[4])}`**" +
                $"\n**Unbans** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[5])}`**" +
                $"\n**ChangesToLogSettings** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[6])}`**" +
                $"\n**FilteredPhrases** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[7])}`**" +
                $"\n**UserConnectsToVoice** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[8])}`**" +
                $"\n**UserDisconnectsFromVoice** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[9])}`**" +
                $"\n**LevelAnnouncements** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[10])}`**" +
                $"\n**Shadowbans** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[11])}`**" +
                $"\n**Unshadowbans** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[12])}`**" +
                "\n**All**");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }
    }
}
