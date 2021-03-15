using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Music;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Kaguya.Discord.Commands.Music
{
	[Module(CommandModule.Music)]
	[Group("loop")]
	public class Loop : KaguyaBase<Loop>
	{
		private readonly LavaNode _lavaNode;
		public Loop(ILogger<Loop> logger, LavaNode lavaNode) : base(logger) { _lavaNode = lavaNode; }

		[Command]
		[Summary("Allows looping of the currently playing track. Default amount is 1, " +
		         "but if you want to repeat a song more than once, you can " +
		         "specify how many times you want to repeat the song using the " +
		         "`amount` parameter. Looped tracks are enqueued immediately after " +
		         "the currently playing song.\n\n" +
		         "The `amount` parameter must be between 1 and 10.")]
		[Remarks("[amount]")]
		// Delete if no remarks needed.
		public async Task LoopMusicCommandAsync(int amount = 1)
		{
			if (amount < 1)
			{
				await SendBasicErrorEmbedAsync("Invalid input. Cannot loop a track less than once.");

				return;
			}

			if (amount > 10)
			{
				await SendBasicErrorEmbedAsync("You may only loop a song 10 times at once.");

				return;
			}

			if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
			{
				await SendBasicErrorEmbedAsync("Failed to join voice channel. Are you in a voice channel?");

				return;
			}

			var player = _lavaNode.GetPlayer(Context.Guild);
			if (player.PlayerState != PlayerState.Playing)
			{
				await SendBasicErrorEmbedAsync("The player must be actively playing a track in " + "order to loop it.");

				return;
			}

			var queueCopy = new DefaultQueue<LavaTrack>();
			var curTrack = player.Track;
			for (int i = 0; i < amount; i++)
			{
				queueCopy.Enqueue(curTrack);
			}

			using (var enumerator = player.Queue.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					queueCopy.Enqueue(enumerator.Current);
				}
			}

			// Clear queue and re-enqueue all items.
			player.Queue.Clear();
			foreach (var item in queueCopy)
			{
				player.Queue.Enqueue(item);
			}

			string s = amount == 1 ? "" : "s";
			var embed = new KaguyaEmbedBuilder(KaguyaColors.Purple)
			{
				Title = "🔂 Loop Tracks",
				Description = $"Successfully looped {curTrack.Title.AsBold()} {amount.ToString().AsBold()} " + $"time{s}"
			};

			await SendEmbedAsync(embed);
		}
	}
}