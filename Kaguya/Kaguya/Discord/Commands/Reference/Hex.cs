using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("hex")]
	[Alias("hx")]
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
		public async Task HexTestCommand(string hex)
		{
			uint colorValue = Convert.ToUInt32(hex, 16);
			uint maxValue = Convert.ToUInt32("FFFFFF", 16);

			// If colorValue is FFFFFF, the color turns black, so we need to subtract one
			if (colorValue == maxValue) --colorValue;

			var embed = new KaguyaEmbedBuilder(colorValue)
			{
				Description = "Success!"
			};

			await SendEmbedAsync(embed);
		}
	}
}
