using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("ping")]
	public class Ping : KaguyaBase<Ping>
	{
		private readonly ILogger<Ping> _logger;
		private readonly DiscordShardedClient _client;

		protected Ping(ILogger<Ping> logger, DiscordShardedClient client) : base(logger)
		{
			_logger = logger;
			_client = client;
		}

		[Command]
		[Summary("Displays the bot's latency to the gateway.")]
		public async Task CommandPing()
		{
			string latency = (_client.Latency.ToString("N0") + "ms").AsBold();
			await SendBasicEmbedAsync($"🏓 Pong! Latency: {latency}", Color.Green);
		}
	}
}