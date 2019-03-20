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
            Leveling.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel);
            string oldUsername = userAccount.Username;
            string newUsername = context.User.Username;
            if(oldUsername + "#" + context.User.Discriminator != newUsername + "#" + context.User.Discriminator)
                userAccount.Username = newUsername + "#" + context.User.Discriminator;
            int argPos = 0;
            if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, null);
                if(!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}
