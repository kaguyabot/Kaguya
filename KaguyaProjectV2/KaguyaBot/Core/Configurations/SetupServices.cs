using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace KaguyaProjectV2.KaguyaBot.Core.Configurations
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