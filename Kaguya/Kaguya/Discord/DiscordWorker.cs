using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kaguya.Discord.options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaguya.Discord
{
    public class DiscordWorker : IHostedService
    {
        private readonly IOptions<DiscordConfigurations> _configs;
        private readonly ILogger<DiscordWorker> _logger;
        private DiscordShardedClient _client;

        public DiscordWorker(IOptions<DiscordConfigurations> configs, ILogger<DiscordWorker> logger)
        {
            _configs = configs;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var restClient = new DiscordRestClient();
            await restClient.LoginAsync(TokenType.Bot, _configs.Value.BotToken);
            var shards = await restClient.GetRecommendedShardCountAsync();

            _client = new DiscordShardedClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = _configs.Value.AlwaysDownloadUsers ?? true,
                MessageCacheSize = _configs.Value.MessageCacheSize ?? 50,
                TotalShards = shards,
                LogLevel = LogSeverity.Debug
            });

            _client.Log += async (logMessage) =>
            {
                await Task.Run(() =>
                    _logger.Log(logMessage.Severity.ToLogLevel(), logMessage.Exception, logMessage.Message));
            };

            await _client.LoginAsync(TokenType.Bot, _configs.Value.BotToken);

            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
        }
    }
}