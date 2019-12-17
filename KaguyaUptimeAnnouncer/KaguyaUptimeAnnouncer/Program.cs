using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using KaguyaUptimeAnnouncer.Timers;
using Microsoft.Extensions.DependencyInjection;

namespace KaguyaUptimeAnnouncer
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig();

            client = new DiscordSocketClient(config);
            GlobalProperties.client = client;

            using (var services = new SetupServices().ConfigureServices(config, client))
            {
                try
                {
                    client = services.GetRequiredService<DiscordSocketClient>();

                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await client.LoginAsync(TokenType.Bot, "NjA2OTcwMjg2ODYwMTQwNTQ0.XfUfSw.zOX5eYnvnnsskY4t47DuTryEIA0");
                    await client.StartAsync();

                    await StartTimers();
                    await Task.Delay(-1);
                }
                catch (HttpException e)
                {
                    Console.WriteLine($"Error when logging into Discord:\n" +
                                      $"-Have you configured your config file?\n" +
                                      $"-Is your token correct? Exception: {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Something broke! Exception: {e.Message}");
                }
            }
        }

        private async Task StartTimers()
        {
            await UptimeTimer.Start();
        }
    }
}
