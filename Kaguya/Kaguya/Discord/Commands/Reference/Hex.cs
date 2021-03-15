using Discord;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("hex")]
	public class Hex : KaguyaBase<Hex>
	{
		public Hex(ILogger<Hex> logger) : base(logger) {}

		[Command]
		[Summary("Takes a hexadecimal color value and outputs an embed with that color. Using " +
		         "a 3-digit value will translate it into a 6-digit hex value (i.e. `0F0` => `00FF00`).")]
		[Remarks("<hex value>")]
		// Various hex lenghts
		[Example("0000FF")]   // 6
		[Example("0F0")]      // 3
		[Example("0xFFFF00")] // 8
		[Example("0x00F")]    // 5
		[Example("#00FFFF")]  // 7
		[Example("#F00")]
		// 4
		public async Task HexTestCommand(string hex)
		{
			// Validate hex length
			if (!(hex.Length >= 3 && hex.Length <= 8))
			{
				if (hex.Length < 3)
				{
					await SendBasicErrorEmbedAsync("Your hex value is too short. It must have a minimum length of 3.");
				}
				else if (hex.Length > 8)
				{
					await SendBasicErrorEmbedAsync("Your hex value is too long. It must have a maximum length of 8.");
				}

				return;
			}

			// If hex has '0x' and '#' in the begining, remove them
			string hexString;

			// && (hex.Length == 5 || hex.Length == 8)
			if (hex.Substring(0, 2) == "0x")
			{
				hexString = hex.Substring(2);
			}
			// && (hex.Length == 4 || hex.Length == 7)
			else if (hex.Substring(0, 1) == "#")
			{
				hexString = hex.Substring(1);
			}
			else if (hex.Length == 3 || hex.Length == 6)
			{
				hexString = hex;
			}
			else
			{
				await SendBasicErrorEmbedAsync("Your hex value format is invalid.");

				return;
			}

			if (hexString.Length != 3 && hexString.Length != 6)
			{
				await SendBasicErrorEmbedAsync("Your hex value is invalid. The hexadecimal number must be 3 or 6 digits long.");

				return;
			}

			// Get the complete hex string
			string completeHex = "";

			if (hexString.Length == 3)
			{
				completeHex += hexString[0];
				completeHex += hexString[0];
				completeHex += hexString[1];
				completeHex += hexString[1];
				completeHex += hexString[2];
				completeHex += hexString[2];
			}
			else if (hexString.Length == 6)
			{
				completeHex = hexString;
			}

			// Get the color value
			uint colorValue;

			try
			{
				colorValue = Convert.ToUInt32(completeHex, 16);
			}
			catch (Exception)
			{
				await SendBasicErrorEmbedAsync(
					"Your hex value is invalid. The digits of a hexidecimal number must be from 0 to 9 and A to F.");

				return;
			}

			uint maxValue = Convert.ToUInt32("FFFFFF", 16);

			// If colorValue is FFFFFF, the color turns black, so we need to subtract one
			if (colorValue == maxValue)
			{
				colorValue--;
			}

			var color = new Color(colorValue);

			// Output embed
			string message = $"Your hex value is **#{completeHex.ToUpper()}**.\n" + "The color of this embed is what your hex looks like.";

			await SendBasicEmbedAsync(message, color);
		}
	}
}