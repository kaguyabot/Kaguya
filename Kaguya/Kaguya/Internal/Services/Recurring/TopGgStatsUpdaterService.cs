using Discord.WebSocket;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Web.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services.Recurring
{
	/// <summary>
	///  Automatically posts stats to Top.GG
	/// </summary>
	public class TopGgStatsUpdaterService : BackgroundService, ITimerReceiver
	{
		private readonly DiscordShardedClient _client;
		private readonly ILogger<TopGgStatsUpdaterService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly ITimerService _timerService;

		public TopGgStatsUpdaterService(ILogger<TopGgStatsUpdaterService> logger, IServiceProvider serviceProvider,
			ITimerService timerService, DiscordShardedClient client)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
			_timerService = timerService;
			_client = client;
		}

		public async Task HandleTimer(object payload)
		{
			if (!_client.AllShardsReady())
			{
				_logger.LogInformation("All shards not ready. Aborting. Retrying in 1 minute");
				await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddMinutes(1), this);

				return;
			}

			await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddMinutes(15), this);

			using (var scope = _serviceProvider.CreateScope())
			{
				var statsRepository = scope.ServiceProvider.GetRequiredService<KaguyaStatisticsRepository>();
				var curStats = await statsRepository.GetMostRecentAsync();

				try
				{
					int guildCount = curStats.ConnectedServers;
					int shardCount = curStats.Shards;

					// DOES NOT WORK. USING MANUAL POST REQUEST AS TEMPORARY SOLUTION.
					// await dblBot.UpdateStatsAsync(guildCount, shardCount, _client.Shards.Select(x => x.ShardId).ToArray());

					await POSTData(guildCount, shardCount);
				}
				catch (Exception e)
				{
					_logger.LogCritical(e, "Failed to post stats to top.gg");
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			await _timerService.TriggerAtAsync(DateTimeOffset.Now, this);
		}

		// ReSharper disable once InconsistentNaming
		private async Task POSTData(int serverCount, int shardCount)
		{
			var config = _serviceProvider.GetRequiredService<IOptions<TopGgConfigurations>>().Value;

			if (config == null)
			{
				_logger.LogCritical("Top.GG stats POST failed. IOptions<TopGgConfigurations> did not have a value! " +
				                    "Please ensure the config file is setup correctly!");

				return;
			}

			string url = $"https://top.gg/api/bots/{_client.CurrentUser.Id}/stats";

			var body = new TopGgStatsPostBody(serverCount, shardCount);
			string json = JsonConvert.SerializeObject(body, Formatting.Indented);

			var data = new StringContent(json, Encoding.UTF8, "application/json");

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
				var response = await client.PostAsync(url, data);

				_logger.LogInformation("Stats posted to top.gg -- response code: " +
				                       $"{(int) response.StatusCode} | Message: {response.ReasonPhrase}");
			}
		}

		private class TopGgStatsPostBody
		{
			public TopGgStatsPostBody(int serverCount, int shardsCount)
			{
				this.ServerCount = serverCount;
				this.ShardsCount = shardsCount;
			}

			[JsonProperty("server_count")]
			public int ServerCount { get; }
			[JsonProperty("shard_count")]
			public int ShardsCount { get; }
		}
	}
}