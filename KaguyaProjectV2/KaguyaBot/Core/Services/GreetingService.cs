using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class GreetingService
    {
        public static async Task Trigger(SocketGuildUser u)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(u.Guild.Id);

            if (!server.CustomGreetingIsEnabled)
                return;
            if (string.IsNullOrWhiteSpace(server.CustomGreeting))
                return;

            var channel = u.Guild.GetTextChannel(server.LogGreetings);

            var greetingMsg = server.CustomGreeting;
            greetingMsg = greetingMsg.Replace("{USERNAME}", u.Username);
            greetingMsg = greetingMsg.Replace("{USERMENTION}", u.Mention);
            greetingMsg = greetingMsg.Replace("{SERVER}", u.Guild.Name);
            greetingMsg = greetingMsg.Replace("{MEMBERCOUNT}", $"{u.Guild.MemberCount}");
            greetingMsg = greetingMsg.Replace("{MEMBERCOUNT2}", $"{u.Guild.MemberCount.Ordinalize()}");

            await channel.SendMessageAsync(greetingMsg);
        }
    }
}
