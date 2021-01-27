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
		[Example("FF")]
		[Example("00FF")]
		public async Task HexTestCommand(string hex)
		{
			if (hex.Length > 6)
			{
				await SendBasicErrorEmbedAsync($"Your hex value is too big. Please enter up to a maximum of 6 digits.");

				return;
			}

			string completeHex = hex;

			// if the user adds less than 6 digits, append zeros to the remaining length of the input
			for (int i = 0; i < 6 - hex.Length; i++)
			{
				completeHex += "0";
			}

			uint colorValue;

			try
			{
				colorValue = Convert.ToUInt32(completeHex, 16);
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

			string message = 
				$"Your hex value is **#{completeHex.ToUpper()}**.\n" +
				$"The color of this embed is what your hex looks like.";

			await SendBasicEmbedAsync(message, color);
		}
	}
}
