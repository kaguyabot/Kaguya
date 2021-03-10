using Discord;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
	[Module(CommandModule.Music)]
	[Group("volume")]
	[Alias("v", "vol")]
	[RequireUserPermission(GuildPermission.Connect)]
	[RequireBotPermission(GuildPermission.Connect)]
	[RequireBotPermission(GuildPermission.Speak)]
	public class Volume : KaguyaBase<Volume>
	{
		private readonly LavaNode _lavaNode;
		public Volume(ILogger<Volume> logger, LavaNode lavaNode) : base(logger) { _lavaNode = lavaNode; }

		[Command]
		[Summary("Changes the volume of the music player to the desired value. Using a `+` or `-` " +
		         "modifier allows for adjustment of the volume to your desired offset.\n\n" +
		         "Valid inputs range from -200 to 200. Please be careful using " +
		         "volume levels above 100, it can get extremely loud and damage your hearing.\n\n" +
		         "Use without a parameter to view the current player's volume.")]
		[Remarks("[# volume]")]
		[Example("50")]
		[Example("+25")]
		[Example("-70")]
		[Example("200 (LOUD)")]
		[Example("0 (Muted)")]
		[Example("-200 (Muted)")]
		public async Task SetVolumeCommand(string desiredVolume = null)
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await SendBasicErrorEmbedAsync("I couldn't find a player for this server. Please ensure you have " +
				                               "an active player before using this command.");

				return;
			}

			int curVolume = player.Volume;
			if (string.IsNullOrWhiteSpace(desiredVolume))
			{
				if (curVolume == 0)
				{
					await SendBasicEmbedAsync("The player is muted.", Color.Orange);
				}
				else
				{
					await SendBasicEmbedAsync($"The current volume is: {curVolume.ToString().AsBold()}.", Color.Teal);
				}

				return;
			}

			if (!short.TryParse(desiredVolume, out short result) || !(result is >= -200 and <= 200))
			{
				await SendBasicErrorEmbedAsync("Invalid input received. Acceptable values are whole " +
				                               "numbers between -200 and 200.\n\n" +
				                               "Please review the `help volume` command for more details.");

				return;
			}

			char beginning = desiredVolume[0];
			bool negate = beginning == '-';
			bool add = beginning == '+';

			int newVolume;
			if (!add && !negate)
			{
				newVolume = result;
			}
			else
			{
				newVolume = Math.Clamp(curVolume + result, 0, 200);
			}

			await player.UpdateVolumeAsync((ushort) newVolume);
			await SendBasicSuccessEmbedAsync($"Updated the volume to {newVolume.ToString().AsBold()}.");
		}
	}
}