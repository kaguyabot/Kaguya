using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Kaguya.Core.LevelingSystem;
using Kaguya.Core.UserAccounts;
using Kaguya.Core.Server_Files;
using Discord;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;
using Kaguya.Modules;
using Kaguya.Core.CommandHandler;

#pragma warning disable

namespace Kaguya
{
    class CommandHandler
    {

        DiscordSocketClient _client;
        CommandService _service;
        private IServiceProvider _services; 
        readonly Color Yellow = new Color(255, 255, 102);
        readonly Color SkyBlue = new Color(63, 242, 255);
        readonly Color Red = new Color(255, 0, 0);
        readonly Color Violet = new Color(238, 130, 238);
        readonly Color Pink = new Color(252, 132, 255);
        readonly KaguyaLogMethods logger = new KaguyaLogMethods();
        public string osuapikey = Config.bot.osuapikey;
        public string tillerinoapikey = Config.bot.tillerinoapikey;
        public async Task InitializeAsync(DiscordSocketClient client)
        {
            try
            {
                _client = client;
                _service = new CommandService();
                _service.AddTypeReader(typeof(List<SocketGuildUser>), new ListSocketGuildUserTR());
                await _service.AddModulesAsync(
                  Assembly.GetExecutingAssembly(),
                  _services);
                _client.Ready += logger.OnReady;
                _client.Ready += logger.GameTimer;
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
            }
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;
            var kID = ulong.TryParse(Config.bot.botUserID, out ulong ID);
            SocketUser kaguya = _client.GetUser(ID);
            if (kaguya != null)
            {
                foreach (SocketGuildChannel channel in context.Guild.Channels)
                {
                    if (!channel.GetPermissionOverwrite(kaguya as IUser).HasValue)
                        await channel.AddPermissionOverwriteAsync(kaguya, OverwritePermissions.AllowAll(channel));
                }
            }
            var userAccount = UserAccounts.GetAccount(context.User);
            if (userAccount.Blacklisted == 1)
            {
                Console.WriteLine($"Blacklisted user {userAccount.Username} detected.");
                return;
            }
            await _client.SetGameAsync("Support Server: yhcNC97");
            var server = Servers.GetServer(context.Guild);
            foreach(string phrase in server.FilteredWords)
            {
                if(msg.Content.Contains(phrase))
                {
                    await logger.UserSaysFilteredPhrase(msg);
                }
            }
            logger.ServerLogMethod(context);
            logger.ServerMethod(context);
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

        
    }
}
