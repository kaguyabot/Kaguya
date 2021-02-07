using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Kaguya.Internal.Events.ArgModels;
using Kaguya.Internal.Services.Models;
using Microsoft.Extensions.Logging;
using OsuSharp;

namespace Kaguya.Internal.Services
{
    public class GuildLoggerService
    {
        private readonly ILogger<GuildLoggerService> _logger;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private readonly LogConfigurationRepository _logConfigurationRepository;
        private readonly DiscordShardedClient _client;
        private readonly AntiraidConfigRepository _antiraidConfigRepository;
        private readonly CommonEmotes _commonEmotes;

        public GuildLoggerService(ILogger<GuildLoggerService> logger, KaguyaServerRepository kaguyaServerRepository, 
            LogConfigurationRepository logConfigurationRepository, DiscordShardedClient client, 
            AntiraidConfigRepository antiraidConfigRepository, CommonEmotes commonEmotes)
        {
            _logger = logger;
            _kaguyaServerRepository = kaguyaServerRepository;
            _logConfigurationRepository = logConfigurationRepository;
            _client = client;
            _antiraidConfigRepository = antiraidConfigRepository;
            _commonEmotes = commonEmotes;
        }
        
        public async Task LogMessageDeleted(Cacheable<IMessage, ulong> cache, ISocketMessageChannel textChannel)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(((SocketGuildChannel) textChannel).Guild.Id);
            var config = await _logConfigurationRepository.GetOrCreateAsync(((SocketGuildChannel) textChannel).Guild.Id);

            if (!config.MessageDeleted.HasValue)
                return;

            IMessage message = cache.Value;

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
                await _client.GetGuild(server.ServerId).GetTextChannel(config.MessageDeleted.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send message deleted log to channel {config.MessageDeleted.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.MessageDeleted = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogMessageUpdated(Cacheable<IMessage, ulong> cache, SocketMessage message, ISocketMessageChannel textChannel)
        {
            if (!(textChannel is SocketGuildChannel channel))
                return;

            if (!cache.HasValue)
            {
                return;
            }
            
            IMessage oldMsg = cache.Value;

            if (oldMsg.Author.IsBot)
            {
                return;
            }
            
            var server = await _kaguyaServerRepository.GetOrCreateAsync(channel.Guild.Id);
            var config = await _logConfigurationRepository.GetOrCreateAsync(channel.Guild.Id);

            if (!config.MessageUpdated.HasValue)
            {
                return;
            }
            
            string content = oldMsg.Content;

            if (content == message.Content)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                content = "<No previous text>";
            }

            var sb = new StringBuilder($"📝 `[{GetFormattedTimestamp()}]` `ID: {oldMsg.Author.Id}` ");
            sb.Append($"Message updated. Author: **{oldMsg.Author}**. Channel: **{((SocketTextChannel) oldMsg.Channel).Mention}**.");

            if (server.IsPremium)
            {
                string arg2Content = string.IsNullOrWhiteSpace(message.Content) ? "<No content>" : message.Content;
                sb.AppendLine($"\nOld Content:\n\"**{content}**\"");
                sb.Append($"New Content:\n\"**{arg2Content}**\"");
            }

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(config.MessageUpdated.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send message updated log to channel {config.MessageUpdated.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.MessageUpdated = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogUserJoined(SocketGuildUser user)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(user.Guild.Id);
            var config = await _logConfigurationRepository.GetOrCreateAsync(user.Guild.Id);

            if (!config.UserJoins.HasValue)
            {
                return;
            }

            string msg = $"✅ `[{GetFormattedTimestamp()}]` `ID: {user.Id}` **{user}** joined the server. Member Count: **{user.Guild.MemberCount:N0}**";
            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(config.UserJoins.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send user join log to channel {config.UserJoins.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.UserJoins = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogUserLeft(SocketGuildUser user)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(user.Guild.Id);
            var config = await _logConfigurationRepository.GetOrCreateAsync(user.Guild.Id);

            if (!config.UserLeaves.HasValue)
            {
                return;
            }

            string msg = $"{_commonEmotes.RedCrossEmote} `[{GetFormattedTimestamp()}]` `ID: {user.Id}` **{user}** left the server. " +
                         $"Member Count: **{user.Guild.MemberCount:N0}**";

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(config.UserLeaves.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send user leave log to channel {config.UserLeaves.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.UserLeaves = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogAntiRaid(AdminAction action, SocketUser socketUser)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(action.ServerId);
            var config = await _logConfigurationRepository.GetOrCreateAsync(action.ServerId);
            var arConfig = await _antiraidConfigRepository.GetAsync(action.ServerId);
                
            if (arConfig == null || !config.Antiraids.HasValue)
            {
                return;
            }
            
            string punishString = AntiraidData.FormattedAntiraidPunishment(action.Action);
            string logString = $"🛡️ `[Anti-Raid]` `[{GetFormattedTimestamp()}]` `ID: {action.ActionedUserId}` **{socketUser}** was automatically {punishString}.";

            try
            {
                await _client.GetGuild(action.ServerId).GetTextChannel(config.Antiraids.Value).SendMessageAsync(logString);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send antiraid log to channel {config.Antiraids.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.Antiraids = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogUserBanned(SocketUser bannedUser, SocketGuild guild)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(guild.Id);
            var config = await _logConfigurationRepository.GetOrCreateAsync(guild.Id);

            if (!config.Bans.HasValue)
            {
                return;
            }

            string msg = $"⛔ `[{GetFormattedTimestamp()}]` `ID: {bannedUser.Id}` **{bannedUser}** was banned from the server. " +
                         $"Member Count: **{guild.MemberCount - 1:N0}**"; // - 1 on count b/c guild is a cached object.

            try
            {
                await guild.GetTextChannel(config.Bans.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send ban log to channel {config.Bans.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.Bans = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogUserUnbanned(SocketUser unbannedUser, SocketGuild guild)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(guild.Id);
            var config = await _logConfigurationRepository.GetOrCreateAsync(guild.Id);

            if (!config.UnBans.HasValue)
            {
                return;
            }

            string msg = $"♻ `[{GetFormattedTimestamp()}]` `ID: {unbannedUser.Id}` **{unbannedUser}** has been unbanned.";

            try
            {
                await guild.GetTextChannel(config.UnBans.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send unban log to channel {config.UnBans.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.UnBans = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogUserVoiceStateUpdated(SocketUser user, SocketVoiceState curVoiceState, SocketVoiceState nextVoiceState)
        {
            KaguyaServer server;
            LogConfiguration config;

            if (curVoiceState.VoiceChannel is null)
            {
                server = await _kaguyaServerRepository.GetOrCreateAsync(nextVoiceState.VoiceChannel.Guild.Id);
                config = await _logConfigurationRepository.GetOrCreateAsync(nextVoiceState.VoiceChannel.Guild.Id);
            }
            else
            {
                server = await _kaguyaServerRepository.GetOrCreateAsync(curVoiceState.VoiceChannel.Guild.Id);
                config = await _logConfigurationRepository.GetOrCreateAsync(curVoiceState.VoiceChannel.Guild.Id);
            }

            if (!config.VoiceUpdates.HasValue)
            {
                return;
            }

            string changeString = ""; // User has...
            string emoji = string.Empty;

            if (curVoiceState.VoiceChannel is null)
            {
                emoji = "🎙️🟢"; // Green circle
                changeString = $"joined **{nextVoiceState.VoiceChannel.Name}**";
            }

            if (curVoiceState.VoiceChannel != null && nextVoiceState.VoiceChannel != null)
            {
                emoji = "🎙️🟡"; // Yellow circle.
                changeString = $"moved from **{curVoiceState.VoiceChannel.Name}** to **{nextVoiceState.VoiceChannel.Name}**";
            }

            if (nextVoiceState.VoiceChannel is null)
            {
                emoji = "🎙️🔴"; // Red circle.
                changeString = $"disconnected from **{curVoiceState.VoiceChannel.Name}**";
            }

            var sb = new StringBuilder($"{emoji} `[{GetFormattedTimestamp()}]` `ID: {user.Id}` **{user}** ");
            sb.Append($"has {changeString}.");

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(config.VoiceUpdates.Value).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                _logger.LogWarning($"Failed to send voice state log to channel {config.VoiceUpdates.Value} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!");

                config.VoiceUpdates = null;
                await _logConfigurationRepository.UpdateAsync(config);
            }
        }

        public async Task LogFilteredPhrase(FilteredWordEventData fwData)
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(fwData.ServerId);
            var config = await _logConfigurationRepository.GetOrCreateAsync(fwData.ServerId);

            if (!config.FilteredWord.HasValue)
            {
                return;
            }

            IUser author = _client.GetUser(fwData.UserId);

            var sb = new StringBuilder($"🛂 `[{GetFormattedTimestamp()}]` `ID: {author.Id}` Filtered word detected from **{author}**. ");
            sb.Append($"Phrase: **{fwData.Phrase}**");

            if (server.IsPremium)
            {
                sb.Append($"\nMessage Contents: **{fwData.Message.Content}**");
            }

            string msg = sb.ToString();

            try
            {
                await _client.GetGuild(fwData.Server.ServerId).GetTextChannel(server.LogFilteredPhrases).SendMessageAsync(msg);
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to send filtered phrase log to channel {server.LogFilteredPhrases} " +
                                             $"in guild {server.ServerId}. Resetting this log channel to null so it " +
                                             "doesn't happen again!", LogLvl.WARN);

                server.LogFilteredPhrases = 0;
                await DatabaseQueries.UpdateAsync(server);
            }
        }
        
        public async Task LogWarn(IModeratorEventArgs mArgs)
        {
            if (!mArgs.Server.IsPremium) return;

            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.WARN,
                "🚔", GetFormattedTimestamp(), mArgs.Server.LogWarns);

            await logger.LogModerationAction();
        }

        public async Task LogUnwarn(IModeratorEventArgs mArgs)
        {
            if (!mArgs.Server.IsPremium) return;

            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.UNWARN,
                "🛃 ☑", GetFormattedTimestamp(), mArgs.Server.LogUnwarns);

            await logger.LogModerationAction();
        }

        public async Task LogShadowban(IModeratorEventArgs mArgs)
        {
            if (!mArgs.Server.IsPremium) return;

            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.SHADOWBAN,
                "👻 ⛔", GetFormattedTimestamp(), mArgs.Server.LogShadowbans);

            await logger.LogModerationAction();
        }

        public async Task LogUnshadowban(IModeratorEventArgs mArgs)
        {
            if (!mArgs.Server.IsPremium) return;

            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.UNSHADOWBAN,
                "👻 ✅", GetFormattedTimestamp(), mArgs.Server.LogUnshadowbans);

            await logger.LogModerationAction();
        }

        public async Task LogMute(IModeratorEventArgs mArgs)
        {
            if (!mArgs.Server.IsPremium) return;

            var logger = new ModeratorEventLogger(mArgs, InternalModerationAction.MUTE,
                "🔕", GetFormattedTimestamp(), mArgs.Server.LogMutes);

            await logger.LogModerationAction();
        }

        public async Task LogUnmute(IModeratorEventArgs mArgs)
        {
            if (!mArgs.Server.IsPremium) return;

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