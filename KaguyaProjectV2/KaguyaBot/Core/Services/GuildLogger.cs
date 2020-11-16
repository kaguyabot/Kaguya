using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using LinqToDB.Common;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class GuildLogger
    {
        private static readonly DiscordShardedClient _client = ConfigProperties.Client;

        public static void InitializeGuildLogListener()
        {
            _client.MessageDeleted += _client_MessageDeleted;
            _client.MessageUpdated += _client_MessageUpdated;
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeft;
            _client.UserBanned += _client_UserBanned;
            _client.UserUnbanned += _client_UserUnbanned;
            _client.UserVoiceStateUpdated += _client_UserVoiceStateUpdated;
            KaguyaEvents.OnRaid += LogAntiRaid;
            KaguyaEvents.OnFilteredPhrase += LogFilteredPhrase;
            KaguyaEvents.OnWarn += LogWarns;
            KaguyaEvents.OnUnwarn += LogUnwarn;
            KaguyaEvents.OnMute += LogMute;
            KaguyaEvents.OnUnmute += LogUnmute;
            KaguyaEvents.OnShadowban += LogShadowban;
            KaguyaEvents.OnUnmute += LogUnshadowban;
        }

        private static async Task _client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(((SocketGuildChannel) arg2).Guild.Id);

            if (server.LogDeletedMessages == 0)
                return;

            if (server.IsCurrentlyPurgingMessages)
                return;

            IMessage message = arg1.Value;

            if (message is null || message.Author.IsBot)
                return;

            string content = string.IsNullOrEmpty(message.Content)
                ? "<Message contained no text>"
                : $"{message.Content}";

            var sb = new StringBuilder($"🗑️ `[{GetFormattedTimestamp()}]` `ID: {message.Author.Id}` ");
            sb.Append($"Message deleted. Author: **{message.Author}**. Channel: **{((SocketTextChannel) message.Channel).Mention}**.");

            // Premium servers get more content in the log.
            if (server.IsPremium)
            {
                sb.Append($"\nContent: \"**{content}**\"");

                if (message.Attachments.Count > 0)
                {
                    sb.Append($" Attachments: **{message.Attachments.Count}**.");
                    foreach (IAttachment a in message.Attachments)
                        sb.Append($" URL: **<{a.ProxyUrl}>**");
                }
            }

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogDeletedMessages).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send message deleted log to channel {server.LogDeletedMessages} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogDeletedMessages = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task _client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (!(arg3 is SocketGuildChannel channel))
                return;

            Server server = await DatabaseQueries.GetOrCreateServerAsync(channel.Guild.Id);

            if (server.LogUpdatedMessages == 0)
                return;

            IMessage oldMsg = arg1.Value;

            if (oldMsg.Author.IsBot) return;

            string content = oldMsg.Content;

            if (content == arg2.Content)
                return;

            if (string.IsNullOrEmpty(content))
                content = "<No previous text>";

            var sb = new StringBuilder($"📝 `[{GetFormattedTimestamp()}]` `ID: {oldMsg.Author.Id}` ");
            sb.Append($"Message updated. Author: **{oldMsg.Author}**. Channel: **{((SocketTextChannel) oldMsg.Channel).Mention}**.");

            if (server.IsPremium)
            {
                string arg2Content = arg2.Content.IsNullOrEmpty() ? "<No content>" : arg2.Content;
                sb.AppendLine($"\nOld Content:\n\"**{content}**\"");
                sb.Append($"New Content:\n\"**{arg2Content}**\"");
            }

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUpdatedMessages).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send message updated log to channel {server.LogUpdatedMessages} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogUpdatedMessages = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task _client_UserJoined(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);

            if (server.LogUserJoins == 0)
                return;

            string msg = $"✅ `[{GetFormattedTimestamp()}]` `ID: {arg.Id}` **{arg}** joined the server. Member Count: **{arg.Guild.MemberCount:N0}**";
            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send user join log to channel {server.LogUserJoins} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogUserJoins = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task _client_UserLeft(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);

            if (server.LogUserLeaves == 0)
                return;

            const string X_CROSS = "<:RedCross:776513248312295484>";
            string msg = $"{X_CROSS} `[{GetFormattedTimestamp()}]` `ID: {arg.Id}` **{arg}** left the server or was kicked. " +
                         $"Member Count: **{arg.Guild.MemberCount:N0}**";

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserLeaves).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send user leave log to channel {server.LogUserLeaves} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogUserLeaves = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task LogAntiRaid(AntiRaidEventArgs e)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(e.SocketGuild.Id);

            if (server.LogAntiraids == 0)
                return;

            string punishString = AntiRaidService.FormattedAntiraidPunishment(e.Punishment);

            int lines = 0;
            var actionedUsers = new StringBuilder();
            foreach (SocketGuildUser user in e.GuildUsers)
            {
                actionedUsers.AppendLine($"🛡️ `[Anti-Raid]` `[{GetFormattedTimestamp()}]` `ID: {user.Id}` **{user}** was automatically {punishString}.");
                lines++;
            }

            // If there are more than 10 users being actioned, send messages in bulk with 10 users per message.
            if (lines > 10)
            {
                string[] textLines = actionedUsers.ToString().Split('\n').Where(x => !x.IsNullOrEmpty()).ToArray();
                int msgCount = (lines + 9) / 10;
                for (int i = 0; i < msgCount; i++)
                {
                    var curMsg = new StringBuilder();
                    for (int j = 0; j < 10; j++)
                    {
                        int index = j + (i * 10);

                        if (index == textLines.Length)
                            break;

                        curMsg.Append(textLines[index]);
                    }

                    try
                    {
                        await _client.GetGuild(e.SocketGuild.Id).GetTextChannel(server.LogAntiraids).SendMessageAsync(curMsg.ToString());
                    }
                    catch (Exception)
                    {
                        await ConsoleLogger.LogAsync($"Failed to send antiraid log to channel {server.LogAntiraids} " +
                                                     $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                                     "doesn't happen again!", LogLvl.WARN);

                        server.LogAntiraids = 0;
                        await DatabaseQueries.UpdateAsync(server);
                    }
                }
            }
            else
            {
                try
                {
                    await _client.GetGuild(e.SocketGuild.Id).GetTextChannel(server.LogAntiraids).SendMessageAsync(actionedUsers.ToString());
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync($"Failed to send antiraid log to channel {server.LogAntiraids} " +
                                                 $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                                 "doesn't happen again!", LogLvl.WARN);

                    server.LogAntiraids = 0;
                    await DatabaseQueries.UpdateAsync(server);
                }
            }
        }

        private static async Task _client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg2.Id);

            if (server.LogBans == 0)
                return;

            string msg = $"⛔ `[{GetFormattedTimestamp()}]` `ID: {arg1.Id}` **{arg1}** was banned from the server. " +
                         $"Member Count: **{arg2.MemberCount - 1:N0}**";

            try
            {
                await arg2.GetTextChannel(server.LogBans).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send ban log to channel {server.LogBans} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogBans = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task _client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg2.Id);

            if (server.LogUnbans == 0)
                return;

            string msg = $"♻ `[{GetFormattedTimestamp()}]` `ID: {arg1.Id}` **{arg1}** has been unbanned.";

            try
            {
                await arg2.GetTextChannel(server.LogUnbans).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send unban log to channel {server.LogUnbans} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogUnbans = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task _client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            Server server;

            if (arg2.VoiceChannel is null)
                server = await DatabaseQueries.GetOrCreateServerAsync(arg3.VoiceChannel.Guild.Id);
            else
                server = await DatabaseQueries.GetOrCreateServerAsync(arg2.VoiceChannel.Guild.Id);

            if (server.LogVoiceChannelConnections == 0)
                return;

            string changeString = ""; // User has...
            string emoji = string.Empty;

            if (arg2.VoiceChannel is null)
            {
                emoji = "🎙️🟢"; // Green circle
                changeString = $"joined **{arg3.VoiceChannel.Name}**";
            }

            if (arg2.VoiceChannel != null && arg3.VoiceChannel != null)
            {
                emoji = "🎙️🟡"; // Yellow circle.
                changeString = $"moved from **{arg2.VoiceChannel.Name}** to **{arg3.VoiceChannel.Name}**";
            }

            if (arg3.VoiceChannel is null)
            {
                emoji = "🎙️🔴"; // Red circle.
                changeString = $"disconnected from **{arg2.VoiceChannel.Name}**";
            }

            var sb = new StringBuilder($"{emoji} `[{GetFormattedTimestamp()}]` `ID: {arg1.Id}` **{arg1}** ");
            sb.Append($"has {changeString}.");

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogVoiceChannelConnections).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send voice state log to channel {server.LogVoiceChannelConnections} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogVoiceChannelConnections = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task LogFilteredPhrase(FilteredPhraseEventArgs fpArgs)
        {
            Server server = fpArgs.Server;

            if (server.LogFilteredPhrases == 0)
                return;

            IUser author = fpArgs.Author;

            var sb = new StringBuilder($"🛂 `[{GetFormattedTimestamp()}]` `ID: {author.Id}` Filtered phrase detected by **{author}**. ");
            sb.Append($"Phrase: **{fpArgs.Phrase}**");

            if (server.IsPremium)
                sb.Append($"\nMessage Contents: **{fpArgs.Message.Content}**");

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(fpArgs.Server.ServerId).GetTextChannel(server.LogFilteredPhrases).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send filtered phrase log to channel {server.LogFilteredPhrases} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to 0 so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogFilteredPhrases = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }

        private static async Task LogWarns(IModeratorEventArgs mArgs)
        {
            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.WARN,
                "🚔", GetFormattedTimestamp(), mArgs.Server.LogWarns);

            await logger.LogModerationAction();
        }

        //todo: shadowban, unshadowban, mute, unmute logtypes...

        private static async Task LogUnwarn(IModeratorEventArgs mArgs)
        {
            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.UNWARN,
                "🛃☑", GetFormattedTimestamp(), mArgs.Server.LogUnwarns);

            await logger.LogModerationAction();
        }

        private static async Task LogShadowban(IModeratorEventArgs mArgs)
        {
            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.SHADOWBAN,
                "👻⛔", GetFormattedTimestamp(), mArgs.Server.LogShadowbans);

            await logger.LogModerationAction();
        }

        private static async Task LogUnshadowban(IModeratorEventArgs mArgs)
        {
            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.UNSHADOWBAN,
                "👻✅", GetFormattedTimestamp(), mArgs.Server.LogUnshadowbans);

            await logger.LogModerationAction();
        }

        private static async Task LogMute(IModeratorEventArgs mArgs)
        {
            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.MUTE,
                "🔕", GetFormattedTimestamp(), mArgs.Server.LogMutes);

            await logger.LogModerationAction();
        }

        private static async Task LogUnmute(IModeratorEventArgs mArgs)
        {
            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.UNMUTE,
                "🔔", GetFormattedTimestamp(), mArgs.Server.LogUnmutes);

            await logger.LogModerationAction();
        }

        private static string GetFormattedTimestamp()
        {
            DateTime d = DateTime.Now;
            var sb = new StringBuilder();

            sb.Append(d.Month.ToString("00") + "-");
            sb.Append(d.Day.ToString("00") + "-");
            sb.Append(d.Year.ToString("0000") + " ");
            sb.Append(d.Hour.ToString("00") + ":");
            sb.Append(d.Minute.ToString("00") + ":");
            sb.Append(d.Second.ToString("00"));

            return sb.ToString();
        }
    }
}