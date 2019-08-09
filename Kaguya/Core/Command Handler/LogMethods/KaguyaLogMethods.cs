using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api;
using DiscordBotsList.Api.Objects;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Modules.osu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace Kaguya.Core.CommandHandler
{
    public class KaguyaLogMethods
    {
        readonly DiscordShardedClient _client = Global.client;
        readonly LavaShardClient _lavaShardClient = Global.lavaShardClient;
        readonly Color Yellow = new Color(255, 255, 102);
        readonly Color SkyBlue = new Color(63, 242, 255);
        readonly Color Red = new Color(255, 0, 0);
        readonly Color Violet = new Color(148, 0, 211);
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();

        public async Task OnReady(DiscordSocketClient client)
        {
            Config.bot.RecentVoteClaimAttempts = 0; //Resets rate limit for DBL API.

            _ = ulong.TryParse(Config.bot.BotUserID, out ulong ID);
            var mutualGuilds = client.GetUser(ID).MutualGuilds;

            if (Global.ShardsLoggedIn == Global.ShardsToLogIn && Global.TotalGuildCount > 1875) 
                //1875 is around how many guilds the bot should be in.
            {
                try
                {
                    logger.ConsoleStatusAdvisory("Retrieving bot from DBL API...");
                    AuthDiscordBotListApi dblAPI = new AuthDiscordBotListApi(ID, Config.bot.DblApiKey);
                    IDblSelfBot me = await dblAPI.GetMeAsync();
                    logger.ConsoleStatusAdvisory("Pushing stats to DBL API...");
                    await me.UpdateStatsAsync(Global.TotalGuildCount);
                    logger.ConsoleStatusAdvisory("Successfully pushed total guild count to DBL.");
                }
                catch (Exception e)
                {
                    logger.ConsoleCriticalAdvisory($"Failed to update Kaguya's DBL Stats (Is this bot on DBL?): {e.Message}");
                }
            }

            else if(Global.ShardsLoggedIn == Global.ShardsToLogIn && Global.TotalGuildCount < 1875 && Config.bot.BotUserID == "538910393918160916")
            {
                //Restarts the bot if the total guild count is lower than expected.

                var filePath = Assembly.GetExecutingAssembly().Location;
                Process.Start(filePath);
                Environment.Exit(0);
            }

            _ = new Dictionary<string, string>
            {
                { "server_count", $"{Global.TotalGuildCount}" }
            };

            int i = 0;
            foreach (var guild in mutualGuilds)
            {
                for (int j = 0; j <= guild.MemberCount; j++)
                {
                    i++;
                }
            }
            
            LoadKaguyaData(); //Loads all user accounts and servers into memory.
            await _lavaShardClient.StartAsync(Global.client); //Initializes the music service.
            logger.ConsoleMusicLogNoUser($"Kaguya Music Service Started. [Shard {client.ShardId}]");
            if (Global.ShardsLoggedIn == Global.ShardsToLogIn)
            {
                logger.ConsoleShardAdvisory($"ALL KAGUYA SHARDS LOGGED IN SUCCESSFULLY!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nBEGIN LOGGING" +
                    "\n--------------------------------------------");
            }
        }

        #pragma warning disable IDE1006 //Disable warnings for naming styles

        public static void LoadKaguyaData() //When called, loads/reloads all accounts and servers into memory.
        {
            if (Global.ShardsLoggedIn == Global.ShardsToLogIn)
            {
                Logger logger = new Logger();
                logger.ConsoleStatusAdvisory("Attempting to load accounts...");
                Global.UserAccounts = DataStorage2.LoadUserAccounts("Resources/accounts.json").ToList();
                logger.ConsoleStatusAdvisory("Accounts loaded. Loading servers...");
                Global.Servers = DataStorage2.LoadServers("Resources/servers.json").ToList();
                logger.ConsoleStatusAdvisory("Servers loaded.");
            }
        }

        public async Task osuLinkParser(SocketMessage s)
        {
            if (s != null)
            {
                BeatmapLinkParser parser = new BeatmapLinkParser();
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
                if (s is SocketUserMessage msg)
                {
                    var context = new ShardedCommandContext(_client, msg);
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

        public async Task JoinedNewGuild(SocketGuild guild)
        {
            logger.ConsoleGuildConnectionAdvisory(guild, "Joined new guild");

            Global.TotalGuildCount++;
            Global.TotalMemberCount += guild.MemberCount;
            Global.TotalTextChannels += guild.TextChannels.Count;
            Global.TotalVoiceChannels += guild.VoiceChannels.Count;

            var cmdPrefix = Servers.GetServer(guild).commandPrefix;
            var owner = guild.Owner;
            var channels = guild.Channels;
            var kID = ulong.TryParse(Config.bot.BotUserID, out ulong ID);
            IUser kaguya = _client.GetUser(ID);
            var dmChannel = await owner.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"Hey there, {owner.Username}, I am Kaguya! I will serve as your server's all-in-one Discord Bot solution complete with powerful administrative commands, " +
                $"in-depth customizable logging, leveling/currency systems, osu! related commands, and more! Before we continue please read the following statement from my creator as it contains very " +
                $"helpful information on how to use me!" +
                $"\n" +
                $"\nHello, thank you for choosing Kaguya! Below are some steps on what you need to know in order to get started." +
                $"\n" +
                $"\n1. Ensure Kaguya has the `Administrator` permission at all times." +
                $"\n2. Move Kaguya's role to the highest spot that you feel comfortable in the hierarchy. This is to ensure all administrative operations " +
                $"run as smoothly as possible. Kaguya cannot action users if their role is above Kaguya's." +
                $"\n3. Kaguya's command usage currently has a rate limit of 3 commands every 4.25 seconds. Exceeding this ratelimit will result in a " +
                $"temporary blacklist that gets longer for each breach. Please don't spam her commands!" +
                $"\n4. Kaguya's prefix is totally customizable up to 3 characters. **The default prefix is `$`.** " +
                $"To change the prefix for your server, simply type `$prefix <new prefix>`, or mention Kaguya if you have another bot that uses `$`. " +
                $"\n5. All commands have very detailed help commands. If you ever wonder how something works, do `$h <command>`. Example: `$h shadowban`" +
                $"\n6. Kaguya features very, very powerful administrative tools. Features include `antiraid`, `masskicking`, `massbanning`, " +
                $"`filtered phrases (dynamic/wildcard)`, `dynamic warning/punishment systems` and many more." +
                $"\n7. Use the `$help` command to see all commands based on category." +
                $"\n8. If you find that you need help with anything, don't understand something, find a bug, have a suggestion, or just want to express your love for Kaguya, please join " +
                $"the dedicated Kaguya Support Discord. I am extremely active here and can usually reply within a few minutes if I'm online. (Look for Stage#0001)" +
                $"\n" +
                $"\nThank you, and enjoy!" +
                $"\n" +
                $"\nKaguya Support: https://discord.gg/aumCJhr");

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

                        await guild.Owner.SendMessageAsync($"{guild.Owner.Mention} **I am missing the permissions required to operate in this guild. " +
                            $"I have exited the server.**");
                        await guild.LeaveAsync();
                        break;
                    }
                }
            }

            server.ID = guild.Id;
            server.ServerName = guild.Name;
            
        }

        public Task LeftGuild(SocketGuild guild)
        {
            Global.TotalGuildCount--;
            Global.TotalMemberCount -= guild.MemberCount;
            Global.TotalTextChannels -= guild.TextChannels.Count;
            Global.TotalVoiceChannels -= guild.TextChannels.Count;
            ServerMessageLogs.RemoveLog(guild.Id);
            logger.ConsoleGuildConnectionAdvisory(guild, "Disconnected from guild.");
            return Task.CompletedTask;
        }

        public Task MessageCache(SocketMessage s) //Called whenever a message is sent in a guild. Adds the message to a list.
        {
            if (s != null)
            {
                Config.bot.LastSeenMessage = DateTime.Now;

                if (s is SocketUserMessage msg && !msg.Author.IsBot)
                {
                    if (msg.Channel is SocketTextChannel)
                    {
                        var guild = (msg.Author as SocketGuildUser).Guild;
                        if (guild != null)
                        {
                            ServerMessageLog currentLog = ServerMessageLogs.GetLog(guild);
                            currentLog.AddMessage(msg);
                            return Task.CompletedTask;
                        }
                        else
                        {
                            logger.ConsoleCriticalAdvisory($"Failed to cache message for {guild.Name} with ID: {guild.Id}! [REMOVING!!!] Thrown from KaguyaLogMethods.cs line 195!");
                            ServerMessageLogs.DeleteLog(guild);
                            return Task.CompletedTask;
                        }
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

            foreach (string loggedMessage in currentLog.LastFiveHundredMessages)
            {
                if (loggedMessage.Contains(msg.Id.ToString()) && !currentServer.IsPurgingMessages)
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
            ulong loggingChannelID = currentServer.LogUpdatedMessages;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetTextChannel(loggingChannelID);
            await cache.GetOrDownloadAsync();
            var msg = cache;

            foreach (string loggedMessage in currentLog.LastFiveHundredMessages)
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
            ulong loggingChannelID = currentServer.LogBans;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Banned");
            embed.WithDescription($"User: `{user.Username}#{user.Discriminator}`\nUser ID: `{user.Id}` \nReason: `{currentServer.MostRecentBanReason}`");
            embed.WithThumbnailUrl("https://i.imgur.com/TKAMjoi.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Violet);
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        public async Task LoggingUserShadowbanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            ulong loggingChannelID = currentServer.LogShadowbans;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Shadowbanned");
            embed.WithDescription($"User: `{user}`\nUser ID: `{user.Id}` \nReason: `{currentServer.MostRecentShadowbanReason}`");
            embed.WithThumbnailUrl("https://i.imgur.com/1TvqYfQ.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Violet);
            await logChannel.SendMessageAsync("", false, embed.Build());

            
        }

        public async Task LoggingUserUnShadowbanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            ulong loggingChannelID = currentServer.LogUnShadowbans;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("User Un-Shadowbanned");
            embed.WithDescription($"User: `{user}`\nUser ID: `{user.Id}`");
            embed.WithThumbnailUrl("https://i.imgur.com/1gRBQHT.png");
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Violet);
            await logChannel.SendMessageAsync("", false, embed.Build());
        }

        public async Task LoggingUserUnbanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            ulong loggingChannelID = currentServer.LogUnbans;
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
            var content = message.Content.Split(' ');
            Server currentServer = Servers.GetServer(server);
            if (author.Administrator && content.Contains("$setlogchannel") || author.Administrator && content.Contains("$log"))
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

        public Task ClientDisconnected(Exception e, DiscordSocketClient client)
        {
            logger.ConsoleCriticalAdvisory(e, $"SHARD {client.ShardId} IS IN DISCONNECTED STATE!!");
            return Task.CompletedTask;
        }
    }
}
