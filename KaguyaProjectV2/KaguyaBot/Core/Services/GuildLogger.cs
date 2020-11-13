using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
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
        private static KaguyaEmbedBuilder _embed;

        public static void InitializeGuildLogListener()
        {
            _client.MessageDeleted += _client_MessageDeleted;
            _client.MessageUpdated += _client_MessageUpdated;
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeft;
            AntiRaidEvent.OnRaid += OnAntiRaid;
            _client.UserBanned += _client_UserBanned;
            _client.UserUnbanned += _client_UserUnbanned;
            _client.UserVoiceStateUpdated += _client_UserVoiceStateUpdated;
            //LevelUps
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

            KaguyaEmbedBuilder embed;
            string content = string.IsNullOrEmpty(message.Content)
                ? "<Message contained no text>"
                : $"{message.Content}";

            var sb = new StringBuilder($"🗑️ `[{GetFormattedTimestamp()}]` `ID: {message.Author.Id}` ");
            sb.Append($"Message deleted. Author: **{message.Author}**. Channel: **{((SocketTextChannel)message.Channel).Mention}**.");
            
            // Premium servers get more content in the log.
            if(server.IsPremium)
            {
                sb.Append($"\nContent: \"**{content}**\"");

                if (message.Attachments.Count > 0)
                {
                    sb.Append($" Attachments: **{message.Attachments.Count}**.");
                    foreach (IAttachment a in message.Attachments)
                    {
                        sb.Append($" URL: **<{a.ProxyUrl}>**");
                    }
                }
            }

            string msg = sb.ToString();
            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogDeletedMessages).SendMessageAsync(msg);
        }

        private static async Task _client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (!(arg3 is SocketGuildChannel channel))
            {
                return;
            }
            
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
            sb.Append($"Message updated. Author: **{oldMsg.Author}**. Channel: **{((SocketTextChannel)oldMsg.Channel).Mention}**.");

            if (server.IsPremium)
            {
                string arg2Content = arg2.Content.IsNullOrEmpty() ? "<No content>" : arg2.Content;
                sb.AppendLine($"\nOld Content:\n\"**{content}**\"");
                sb.Append($"New Content:\n\"**{arg2Content}**\"");
            }

            string msg = sb.ToString();
            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUpdatedMessages).SendMessageAsync(msg);
        }

        private static async Task _client_UserJoined(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);

            if (server.LogUserJoins == 0)
                return;

            string msg = $"✅ `[{GetFormattedTimestamp()}]` `ID: {arg.Id}` **{arg}** joined the server. Member Count: **{arg.Guild.MemberCount:N0}**";
            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendMessageAsync(msg);
        }

        private static async Task _client_UserLeft(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);

            if (server.LogUserLeaves == 0)
                return;

            const string X_CROSS = "<:RedCross:776513248312295484>";
            string msg = $"{X_CROSS} `[{GetFormattedTimestamp()}]` `ID: {arg.Id}` **{arg}** left the server or was kicked. Member Count: **{arg.Guild.MemberCount:N0}**";

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendMessageAsync(msg);
        }

        private static async Task OnAntiRaid(AntiRaidEventArgs e)
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
                    catch (Exception exception)
                    {
                        await ConsoleLogger.LogAsync($"Failed to deliver anti-raid log message in guild {server.ServerId}!\nReason: {exception.Message}", LogLvl.WARN);
                    }
                }
            }
            else
            {
                try
                {
                    await _client.GetGuild(e.SocketGuild.Id).GetTextChannel(server.LogAntiraids).SendMessageAsync(actionedUsers.ToString());
                }
                catch (Exception exception)
                {
                    await ConsoleLogger.LogAsync($"Failed to deliver anti-raid log message in guild {server.ServerId}!\nReason: {exception.Message}", LogLvl.WARN);
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
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync($"Failed to deliver user ban log message in guild {server.ServerId}!\nReason: {e.Message}", LogLvl.WARN);
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
                await arg2.GetTextChannel(server.LogBans).SendMessageAsync(msg);
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync($"Failed to deliver user unban log message in guild {server.ServerId}!\nReason: {e.Message}", LogLvl.WARN);
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
            {
                return;
            }

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
            
            if (server.LogVoiceChannelConnections != 0)
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogVoiceChannelConnections).SendMessageAsync(msg);
            }
        }
        
        private static string GetFormattedTimestamp()
        {
            var d = DateTime.Now;
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