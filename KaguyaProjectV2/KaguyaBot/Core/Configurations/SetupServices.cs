using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart
{
    public class SetupServices
    {
        public ServiceProvider ConfigureServices(DiscordSocketConfig config, DiscordShardedClient client)
        {
            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new InteractiveService(client))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }
    }
}