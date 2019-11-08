using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart
{
    public class SetupServices
    {
        public ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton(new DiscordSocketClient()) //Added for InteractiveService to work
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }
    }
}
