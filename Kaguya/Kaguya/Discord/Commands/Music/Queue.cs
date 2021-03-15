using Discord;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Music;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
	[Module(CommandModule.Music)]
	[Group("queue")]
	[Alias("q")]
	[RequireUserPermission(GuildPermission.Connect)]
	[RequireBotPermission(GuildPermission.Connect)]
	[RequireBotPermission(GuildPermission.Speak)]
	public class Queue : KaguyaBase<Queue>
	{
		private readonly LavaNode _lavaNode;
		public Queue(ILogger<Queue> logger, LavaNode lavaNode) : base(logger) { _lavaNode = lavaNode; }

		[Command]
		[Summary("Displays the current music queue, up to 25 songs.")]
		public async Task QueueCommand()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendBasicErrorEmbedAsync("The player couldn't be found for this server. Ensure " +
				                               "there is an active music player before using this command.");

				return;
			}

			await SendEmbedAsync(MusicEmbeds.GetQueueEmbed(player.Queue, player.Track));
		}
	}
}