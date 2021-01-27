using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Discord;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("hex")]
	public class Hex : KaguyaBase<Hex>
	{
		private readonly ILogger<Hex> _logger;

		public Hex(ILogger<Hex> logger) : base(logger)
		{
			_logger = logger;
		}

		[Command]
		[Summary("Takes a hexadecimal color value and outputs an embed with that color.")]
		[Remarks("<hex value>")]
		[Example("FFFFFF")]
		[Example("0000FF")]
		[Example("FF0000")]
		public async Task HexTestCommand(string hex)
		{
			if (hex.Length < 6 || hex.Length > 6)
			{
				await SendBasicErrorEmbedAsync($"The hex value must have a length of 6 and your value has a length of {hex.Length}.");

				return;
			}

			uint colorValue;

			try
			{
				colorValue = Convert.ToUInt32(hex, 16);
			}
			catch (Exception)
			{
				await SendBasicErrorEmbedAsync($"Your hex value is not valid. The digits of a hexidecimal number must be from 0 to 9 and A to F.");

				return;
			}

			uint maxValue = Convert.ToUInt32("FFFFFF", 16);

			// If colorValue is FFFFFF, the color turns black, so we need to subtract one
			if (colorValue == maxValue)
			{
				colorValue--;
			}

			var color = new Color(colorValue);

			await SendBasicEmbedAsync("The color of this embed is what your hex looks like.", color);
		}
	}
}
