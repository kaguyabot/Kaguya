using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord.WebSocket;
using Discord_Bot.Core.LevelingSystem;
using Discord_Bot.Core.UserAccounts;
using Discord_Bot.Core.Server_Files;
using Discord;

#pragma warning disable

namespace Discord_Bot
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;
        private IServiceProvider _services;
        Color Yellow = new Color(255, 255, 102);
        Color SkyBlue = new Color(63, 242, 255);
        Color Red = new Color(255, 0, 0);
        Color Violet = new Color(238,130,238);

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            _service.AddTypeReader(typeof(List<SocketGuildUser>), new ListSocketGuildUserTR());
            await _service.AddModulesAsync(
              Assembly.GetExecutingAssembly(),
              _services);
            _client.JoinedGuild += JoinedNewGuild;
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += MessageCache;
            _client.MessageDeleted += LoggingDeletedMessages;
            _client.MessageUpdated += LoggingEditedMessages;
            _client.UserJoined += LoggingUserJoins;
            _client.UserLeft += LoggingUserLeaves;
            _client.UserBanned += LoggingUserBanned;
            _client.UserUnbanned += LoggingUserUnbanned;
            _client.MessageReceived += LogChangesToLogSettings;
            _client.MessageReceived += UserSaysFilteredPhrase;
            _client.UserVoiceStateUpdated += UserConnectsToVoice;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;
            var userAccount = UserAccounts.GetAccount(context.User);
            if (userAccount.Blacklisted == 1)
            {
                Console.WriteLine($"Blacklisted user {userAccount.Username} detected.");
                return;
            }
            var server = Servers.GetServer(context.Guild);
            foreach(string phrase in server.FilteredWords)
            {
                if(phrase.Contains(msg.ToString()))
                {
                    UserSaysFilteredPhrase(msg);
                }
            }
            ServerLogMethod(context);
            ServerMethod(context);
            Leveling.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel);
            string oldUsername = userAccount.Username;
            string newUsername = context.User.Username;
            if (oldUsername + "#" + context.User.Discriminator != newUsername + "#" + context.User.Discriminator)
                userAccount.Username = newUsername + "#" + context.User.Discriminator;
            List<ulong> oldIDs = userAccount.IsInServerIDs;
            List<string> oldSNames = userAccount.IsInServers;
            if (oldIDs.Contains(context.Guild.Id))
            {
                userAccount.IsInServerIDs = oldIDs;
                UserAccounts.SaveAccounts();
            }
            else if (oldSNames.Contains(context.Guild.Name))
            {
                userAccount.IsInServers = oldSNames;
                UserAccounts.SaveAccounts();
            }
            else
            {
                userAccount.IsInServerIDs.Add(context.Guild.Id);
                userAccount.IsInServers.Add(context.Guild.Name);
                UserAccounts.SaveAccounts();
            }
            int argPos = 0;
            if (msg.HasStringPrefix(Servers.GetServer(context.Guild).commandPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        private static void ServerMethod(SocketCommandContext context)
        {
            var server = Servers.GetServer(context.Guild);
            server.ID = context.Guild.Id;
            server.ServerName = context.Guild.Name;
            Servers.SaveServers();
        }

        private static void ServerLogMethod(SocketCommandContext context)
        {
            var serverLog = ServerMessageLogs.GetLog(context.Guild);
            serverLog.ID = context.Guild.Id;
            serverLog.ServerName = context.Guild.Name;
            ServerMessageLogs.SaveServerLogging();
        }

        private async Task JoinedNewGuild(SocketGuild guild)
        {
            var cmdPrefix = Servers.GetServer(guild).commandPrefix;
            var owner = guild.Owner;
            await owner.GetOrCreateDMChannelAsync();
            await owner.SendMessageAsync($"Hey there, {owner.Username}, I am Kaguya! I will serve as your server's all-in-one Discord Bot solution complete with powerful administrative commands, " +
                $"in-depth customizable logging, leveling/currency systems, osu! related commands, and more! Before we continue please read the following statement from my creator as it contains very " +
                $"helpful information on how to use me!" +
                $"\n" +
                $"\nGreetings, I recommend you check out the `{cmdPrefix}help` and `{cmdPrefix}helpdm` commands before continuing. If you have any troubles using Kaguya, resort to these commands!" +
                $"\nIn addition, Kaguya's default prefix is `$`. If you have another bot that uses `$`, don't worry as her prefix is fully customizable (up to two characters). In chat, tag Kaguya (`@Kaguya#2708`) " +
                $"and type `prefix <new prefix>` to edit her prefix. This way, you won't accidentally change the prefix of another bot that also uses the `$` symbol. If you ever wish to reset your prefix " +
                $"back to the default, tag Kaguya and type `prefix` with nothing else." +
                $"\nExamples of how to edit and reset the prefix: " +
                $"\n`@Kaguya#2708 prefix k!` (<-- Sets prefix to `k!`)" +
                $"\n`@Kaguya#2708 prefix` (<-- Prefix has been reset from `<old prefix>` to `$`)" +
                $"\n" +
                $"\nFinally, if you wish to report a bug, please go to the Kaguya github page (found through `{cmdPrefix}helpdm`) and create an issue. Same thing for feature requests :)" +
                $"\n" +
                $"\nThank you, and enjoy!");
        }

        private async Task MessageCache(SocketMessage s) //Called whenever a message is sent in a guild. Adds the message to a list.
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            var currentLog = ServerMessageLogs.GetLog(context.Guild);
            currentLog.AddMessage(msg);
            ServerMessageLogs.SaveServerLogging();
        }

        private async Task LoggingDeletedMessages(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) //Called whenever a message is deleted
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
            
            foreach(string loggedMessage in currentLog.LastThousandMessages)
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

        private async Task LoggingEditedMessages(Cacheable<IMessage, ulong> cache, SocketMessage message, ISocketMessageChannel channel)
        {
            IGuild server = (channel as ITextChannel).Guild;
            string channelName = channel.Name;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            var currentLog = ServerMessageLogs.GetLog((SocketGuild)server);
            ulong loggingChannelID = currentServer.LogMessageEdits;
            if (loggingChannelID == 0) return;
            ISocketMessageChannel logChannel = (ISocketMessageChannel)_client.GetGuild(currentServer.ID).GetChannel(loggingChannelID);
            await cache.GetOrDownloadAsync();
            var msg = cache;

            foreach (string loggedMessage in currentLog.LastThousandMessages)
            {
                if (loggedMessage.Contains(message.Id.ToString()) & !message.Author.IsBot)
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

        private async Task LoggingUserJoins(SocketGuildUser user)
        {
            IGuild server = (user as IGuildUser).Guild;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            var currentLog = ServerMessageLogs.GetLog((SocketGuild)server);
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

        }

        private async Task LoggingUserLeaves(SocketGuildUser user)
        {
            IGuild server = (user as IGuildUser).Guild;
            Server currentServer = Servers.GetServer((SocketGuild)server);
            var currentLog = ServerMessageLogs.GetLog((SocketGuild)server);
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

        private async Task LoggingUserBanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            var currentLog = ServerMessageLogs.GetLog(server);
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

        private async Task LoggingUserUnbanned(SocketUser user, SocketGuild server)
        {
            Server currentServer = Servers.GetServer(server);
            var currentLog = ServerMessageLogs.GetLog(server);
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

        private async Task LogChangesToLogSettings(SocketMessage message)
        {
            var server = (message.Channel as SocketGuildChannel).Guild;
            SocketGuildChannel channel = message.MentionedChannels.First();
            var user = (message as SocketUserMessage).Author;
            var author = (user as SocketGuildUser).GuildPermissions;
            Server currentServer = Servers.GetServer(server);
            var currentLog = ServerMessageLogs.GetLog(server);
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

        private async Task UserSaysFilteredPhrase(SocketMessage message) //note for stage: make sure this is the only place where a filtered phrase gets removed
        {
            var guild = (message.Channel as SocketGuildChannel).Guild;
            var server = Servers.GetServer(guild);
            var filteredPhrases = server.FilteredWords;
            foreach(string phrase in filteredPhrases)
            {
                if (message.Content.Contains($"{phrase}"))
                {
                    message.DeleteAsync();
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

        private async Task UserConnectsToVoice(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            var guild = (user as SocketGuildUser).Guild;
            var server = Servers.GetServer(guild);
            var voiceChannelBefore = beforeState.VoiceChannel;
            var voiceChannelAfter = afterState.VoiceChannel;
            if(voiceChannelBefore != null && voiceChannelAfter == null) //When user disconnects from VC
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
            else if(voiceChannelBefore == null && voiceChannelAfter != null) //When user connects to VC
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
    }
}
