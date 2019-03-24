using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord_Bot.Core.LevelingSystem;
using Discord_Bot.Core.UserAccounts;
using Discord_Bot.Core.Server_Files;

namespace Discord_Bot
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;
        private IServiceProvider _services;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            _service.AddTypeReader(typeof(List<SocketGuildUser>), new ListSocketGuildUserTR());
            await _service.AddModulesAsync(
              Assembly.GetExecutingAssembly(),
              _services);
            _client.MessageReceived += HandleCommandAsync;
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
            if (server.ID != null)
                return;
            else Servers.SaveServers();
        }
    }
}
