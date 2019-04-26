using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Core.Server_Files;
using Discord;
using Kaguya.Modules.osu;
using System.Diagnostics;
using System.Timers;
using Kaguya.Core.Command_Handler;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using System.Collections.Generic;

namespace Kaguya.Core.CommandHandler
{
    public class KaguyaLogMethods
    {
        readonly DiscordSocketClient _client = Global.Client;
        readonly public IServiceProvider _services;
        readonly Color Yellow = new Color(255, 255, 102);
        readonly Color SkyBlue = new Color(63, 242, 255);
        readonly Color Red = new Color(255, 0, 0);
        readonly Color Violet = new Color(238, 130, 238);
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();
        readonly Timers timer = new Timers();

        private static readonly HttpClient client = new HttpClient();

        public async Task OnReady()
        {
            var botID = ulong.TryParse(Config.bot.botUserID, out ulong ID);
            var mutualGuilds = _client.GetUser(ID).MutualGuilds;

            var serverCountValue = new Dictionary<string, string>
            {
                {"server_count", $"{mutualGuilds.Count()}"}
            };

            Console.WriteLine("Pushing guild count to DBL API...");

            var content = new FormUrlEncodedContent(serverCountValue);
            var response = await client.PostAsync($"https://discordbots.org/api/bots/{Config.bot.botUserID}/stats", content);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Discord Bot List API Response:" + responseString);

            int i = 0;
            foreach(var guild in mutualGuilds)
            {
                for (int j = 0; j <= guild.MemberCount; j++)
                {
                    i++;
                }

            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\nAce Pilot Kaguya cleared for takeoff. Servicing {mutualGuilds.Count()} guilds" +
                $" and {i.ToString("N0")} members." +
                "\nBegin Logging\n");
            Console.WriteLine("--------------------------------------------");
        }

        #pragma warning disable IDE1006 //Disable warnings for naming styles

        public async Task osuLinkParser(SocketMessage s)
        {
            if (s != null)
            {
                BeatmapLinkParser parser = new BeatmapLinkParser();
                EmbedBuilder embed = new EmbedBuilder();
                if (s is SocketUserMessage msg)
                {
                    var context = new SocketCommandContext(_client, msg);
                    if (s.Content.Contains("https://osu.ppy.sh/beatmapsets/"))
                    {
                        await parser.LinkParserMethod(s, embed, context);
                    }
                    else if (s.Content.Contains("https://osu.ppy.sh/b/"))
                    {
                        await parser.LinkParserMethod(s, embed, context);
                    }
                    else { return; }
                }
                else { return; }
            }
        }

        public void ServerLogMethod(SocketCommandContext context)
        {
            var server = Servers.GetServer(context.Guild);
            server.ID = context.Guild.Id;
            server.ServerName = context.Guild.Name;
            ServerMessageLogs.SaveServerLogging();
        }

        public async Task JoinedNewGuild(SocketGuild guild)
        {
            var cmdPrefix = Servers.GetServer(guild).commandPrefix;
            var owner = guild.Owner;
            var channels = guild.Channels;
            var kID = ulong.TryParse(Config.bot.botUserID, out ulong ID);
            IUser kaguya = _client.GetUser(ID);
            await owner.GetOrCreateDMChannelAsync();
            await owner.SendMessageAsync($"Hey there, {owner.Username}, I am Kaguya! I will serve as your server's all-in-one Discord Bot solution complete with powerful administrative commands, " +
                $"in-depth customizable logging, leveling/currency systems, osu! related commands, and more! Before we continue please read the following statement from my creator as it contains very " +
                $"helpful information on how to use me!" +
                $"\n" +
                $"\nGreetings, **The very first thing you should do as the server owner is move Kaguya's role to the highest position in your role list. Else, the bot may not work for your server!!** " +
                $"Second, the first command `({cmdPrefix}exp)` for example, may be very slow, as Kaguya has to update all channel permissions to allow for her use in your server. Do not make any changes " +
                $"to these permissions. " +
                $"Next, I recommend you check out the `{cmdPrefix}help` and `{cmdPrefix}helpdm` commands before continuing. If you have any troubles using Kaguya, resort to these commands!" +
                $"\nIn addition, Kaguya's default prefix is `$`. If you have another bot that uses `$`, don't worry as her prefix is fully customizable (up to two characters). In chat, tag Kaguya (`@Kaguya#2708`) " +
                $"and type `prefix <new prefix>` to edit her prefix. This way, you won't accidentally change the prefix of another bot that also uses the `$` symbol. If you ever wish to reset your prefix " +
                $"back to the default, tag Kaguya and type `prefix` with nothing else." +
                $"\nExamples of how to edit and reset the prefix: " +
                $"\n`@Kaguya#2708 prefix k!` (<-- Sets prefix to `k!`)" +
                $"\n`@Kaguya#2708 prefix` (<-- Prefix has been reset from `<old prefix>` to `$`)" +
                $"\n" +
                $"\nFinally, if you wish to report a bug, please go to the Kaguya github page (found through `{cmdPrefix}helpdm`) and create an issue." +
                $"\nYou may also let me know in Kaguya's dedicated support server: https://discord.gg/yhcNC97" +
                $"\n" +
                $"\nThank you, and enjoy!");

            var server = Servers.GetServer(guild);

            foreach (SocketTextChannel channel in guild.TextChannels)
            {
                if (!channel.GetPermissionOverwrite(kaguya as SocketGuildUser).HasValue && server.IsBlacklisted == false)
                {
                    try
                    {
                        await channel.AddPermissionOverwriteAsync(kaguya, OverwritePermissions.AllowAll(channel));
                        logger.ConsoleGuildAdvisory(guild, channel, $"Kaguya has been granted permissions for channel #{channel.Name}");
                    }
                    catch (Exception exception)
                    {
                        logger.ConsoleStatusAdvisory($"Could not overwrite permissions for #{channel.Name} in guild \"{channel.Guild.Name}\"");
                        logger.ConsoleCriticalAdvisory(exception, $"Guild {guild.Name} has been blacklisted.");

                        await guild.Owner.SendMessageAsync($"**This server has been blacklisted because I was unable to alter text channel permissions." +
                            $"\nPlease contact Stage#0001 in my support server (https://discord.gg/yhcNC97) to be unblacklisted!**");
                        server.IsBlacklisted = true;
                    }
                }
            }

            server.ID = guild.Id;
            server.ServerName = guild.Name;
            Servers.SaveServers();
            logger.ConsoleGuildConnectionAdvisory(guild, "Joined new guild");
        }

        public Task LeftGuild(SocketGuild guild)
        {
            logger.ConsoleGuildConnectionAdvisory(guild, "Disconnected from guild.");
            return Task.CompletedTask;
        }

        public Task MessageCache(SocketMessage s) //Called whenever a message is sent in a guild. Adds the message to a list.
        {
            if (s != null)
            {
                var msg = s as SocketUserMessage;

                Config.bot.LastSeenMessage = DateTime.Now;

                if (msg != null && msg.Channel.GetType().ToString() == "Discord.WebSocket.SocketTextChannel") //Checks to make sure that the message is actually from a guild channel (NOT a DM).
                {
                    SocketCommandContext context = new SocketCommandContext(_client, msg);
                    var guild = context.Guild;
                    if (guild != null)
                    {
                        ServerMessageLog currentLog = ServerMessageLogs.GetLog(context.Guild);
                        currentLog.AddMessage(msg);
                        ServerMessageLogs.SaveServerLogging();
                        return Task.CompletedTask;
                    }
                    else
                    {
                        logger.ConsoleCriticalAdvisory($"Failed to cache message for {context.Guild.Name} with ID: {context.Guild.Id}! [REMOVING!!!] Thrown from KaguyaLogMethods.cs line 139!");
                        ServerMessageLogs.RemoveLog(context.Guild.Id);
                        return Task.CompletedTask;
                    }
                }
            }
            return Task.CompletedTask;
        }

        public async Task LoggingDeletedMessages(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) //Called whenever a message is deleted
        {
            IGuild server = (channel as ITextChannel).Guild;
            string channelName = channel.Name;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            var currentLog = ServerMessageLogs.GetLog((SocketGuild)server);
            ulong loggingChannelID = currentServer.LogDeletedMessages;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            await cache.GetOrDownloadAsync();
            var msg = cache;

            foreach (string loggedMessage in currentLog.LastThousandMessages)
            {
                if (loggedMessage.Contains(msg.Id.ToString()) && !loggedMessage.Contains("Kaguya#2708"))
                {
                    var text = loggedMessage.Split('℀');
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithTitle("Message Deleted");
                    embed.WithDescription($"`{text.FirstOrDefault(x => x.Contains("Author:"))}` " +
                        $"\n`{text.FirstOrDefault(x => x.Contains("Channel:"))}` \n`{text.FirstOrDefault(x => x.Contains("Message:"))}` \n`{text.FirstOrDefault(x => x.Contains("MsgID"))}`");
                    embed.WithThumbnailUrl("https://i.imgur.com/ODy5o2s.png");
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithColor(Yellow);
                    await logChannel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task LoggingEditedMessages(Cacheable<IMessage, ulong> cache, SocketMessage message, ISocketMessageChannel channel)
        {
            IGuild server = (channel as ITextChannel).Guild;
            string channelName = channel.Name;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            var currentLog = ServerMessageLogs.GetLog((SocketGuild)server);
            ulong loggingChannelID = currentServer.LogMessageEdits;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetTextChannel(loggingChannelID);
            await cache.GetOrDownloadAsync();
            var msg = cache;

            foreach (string loggedMessage in currentLog.LastThousandMessages)
            {
                if (loggedMessage.Contains(message.Id.ToString()) & !message.Author.IsBot && (!loggedMessage.Contains(message.Content)))
                {
                    var text = loggedMessage.Split('℀');
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithTitle("Message Updated");
                    embed.WithDescription($"**Author**: `{message.Author}`\n**Channel**: `#{channel.Name}`\n**Old** {text.FirstOrDefault(x => x.Contains("Message:"))}\n**New** Message: {message.Content}");
                    embed.WithThumbnailUrl("https://i.imgur.com/FdZ5nNT.png");
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithColor(Yellow);
                    await logChannel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task LoggingUserJoins(SocketGuildUser user)
        {
            IGuild server = (user as IGuildUser).Guild;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            ulong loggingChannelID = currentServer.LogWhenUserJoins;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Joined");
            embed.WithDescription($"User: `{user.Username}#{user.Discriminator}`\nUser ID: `{user.Id}`\nAccount Created: `{user.CreatedAt}`");
            embed.WithThumbnailUrl("https://i.imgur.com/LXiUKgF.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(SkyBlue);
            await logChannel.SendMessageAsync("", false, embed.Build());
            logger.ConsoleGuildConnectionAdvisory(user.Guild, "User joined guild");
        }

        public async Task LoggingUserLeaves(SocketGuildUser user)
        {
            IGuild server = (user as IGuildUser).Guild;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            ulong loggingChannelID = currentServer.LogWhenUserLeaves;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Left");
            embed.WithDescription($"User: `{user.Username}#{user.Discriminator}`\n`User ID: {user.Id}`");
            embed.WithThumbnailUrl("https://i.imgur.com/624oxi8.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Red);
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        public async Task LoggingUserBanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            ulong loggingChannelID = currentServer.LogWhenUserIsBanned;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Banned");
            embed.WithDescription($"User: `{user.Username}#{user.Discriminator}`\nUser ID: `{user.Id}`");
            embed.WithThumbnailUrl("https://i.imgur.com/TKAMjoi.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Violet);
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        public async Task LoggingUserUnbanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            ulong loggingChannelID = currentServer.LogWhenUserIsUnbanned;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Unbanned");
            embed.WithDescription($"User: `{user.Username}#{user.Discriminator}`\nUser ID: `{user.Id}`");
            embed.WithThumbnailUrl("https://i.imgur.com/RH0HHkJ.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Violet);
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        public async Task LogChangesToLogSettings(SocketMessage message)
        {
            if (message.Author.IsBot) { return; }
            var server = (message.Channel as SocketGuildChannel).Guild;
            var user = (message as SocketUserMessage).Author;
            var author = (user as SocketGuildUser).GuildPermissions;
            Server currentServer = Servers.GetServer(server);
            if (author.Administrator && message.Content.Contains("$setlogchannel") || author.Administrator && message.Content.Contains("$log"))
            {
                ulong loggingChannelID = currentServer.LogChangesToLogSettings;
                if (loggingChannelID == 0) return;
                ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("Administrator Changed Log Settings");
                embed.WithDescription($"Administrator `{user}` has changed logging settings for `{server.Name}`.");
                embed.WithThumbnailUrl("https://i.imgur.com/4lBFG4H.png");
                embed.WithColor(Violet);
                embed.WithTimestamp(DateTime.Now);
                await logChannel.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task UserSaysFilteredPhrase(SocketMessage message)
        {
            if (message != null && message.Channel.GetType().ToString() == "Discord.WebSocket.SocketTextChannel")
            {
                var guild = (message.Channel as SocketGuildChannel).Guild;
                var server = Servers.GetServer(guild);
                var filteredPhrases = server.FilteredWords;
                foreach (string phrase in filteredPhrases)
                {
                    if (message.Content.ToLower().Contains($"{phrase}"))
                    {
                        await message.DeleteAsync();
                        ulong loggingChannelID = server.LogWhenUserSaysFilteredPhrase;
                        if (loggingChannelID == 0) return;
                        ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(server.ID).GetChannel(loggingChannelID);
                        EmbedBuilder embed = new EmbedBuilder();
                        embed.WithTitle("Filtered Phrase Detected");
                        embed.WithDescription($"**Author:** `{message.Author}`" +
                            $"\n**UserID:** `{message.Author.Id}`" +
                            $"\n**Message:** `{message.Content}`" +
                            $"\n**MessageID:** `{message.Id}`");
                        embed.WithThumbnailUrl("https://i.imgur.com/npzKmyY.png");
                        embed.WithColor(Red);
                        embed.WithTimestamp(DateTime.Now);
                        await logChannel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task UserConnectsToVoice(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            var guild = (user as SocketGuildUser).Guild;
            var server = Servers.GetServer(guild);
            var voiceChannelBefore = beforeState.VoiceChannel;
            var voiceChannelAfter = afterState.VoiceChannel;
            if (voiceChannelBefore != null && voiceChannelAfter == null) //When user disconnects from VC
            {
                ulong loggingVoiceDisconnectID = server.LogWhenUserDisconnectsFromVoiceChannel;
                if (loggingVoiceDisconnectID == 0) return;
                ISocketMessageChannel logVoiceDisconnect = (ISocketMessageChannel)_client.GetGuild(server.ID).GetChannel(loggingVoiceDisconnectID);
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("User Disconnected From Voice Chat");
                embed.WithDescription($"**User:** `{user.Username}#{user.Discriminator}`" +
                    $"\n**Exited From Channel:** `{voiceChannelBefore}`");
                embed.WithThumbnailUrl("https://i.imgur.com/B5BNtp3.png");
                embed.WithColor(Yellow);
                embed.WithTimestamp(DateTime.Now);
                await logVoiceDisconnect.SendMessageAsync("", false, embed.Build());
            }
            else if (voiceChannelBefore == null && voiceChannelAfter != null) //When user connects to VC
            {
                ulong loggingVoiceConnectID = server.LogWhenUserConnectsToVoiceChannel;
                if (loggingVoiceConnectID == 0) return;
                ISocketMessageChannel logVoiceConnect = (ISocketMessageChannel)_client.GetGuild(server.ID).GetChannel(loggingVoiceConnectID);
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle("User Connected To Voice Chat");
                embed.WithDescription($"**User:** `{user.Username}#{user.Discriminator}`" +
                    $"\n**Entered Channel:** `{voiceChannelAfter}`");
                embed.WithThumbnailUrl("https://i.imgur.com/iAIa8La.png");
                embed.WithColor(Yellow);
                embed.WithTimestamp(DateTime.Now);
                await logVoiceConnect.SendMessageAsync("", false, embed.Build());
            }
        }

        public Task ClientDisconnected(Exception e)
        {
            logger.ConsoleCriticalAdvisory(e, "KAGUYA IS IN DISCONNECTED STATE!!");
            return Task.CompletedTask;
        }

        public Task ClientConnected()
        {
            logger.ConsoleStatusAdvisory("Client connected.");
            return Task.CompletedTask;
        }
    }
}
