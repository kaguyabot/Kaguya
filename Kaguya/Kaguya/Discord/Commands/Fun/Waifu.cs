using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using NekosSharp;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Fun
{
	[Module(CommandModule.Fun)]
	[Group("waifu")]
	public class Waifu : KaguyaBase<Waifu>
	{
		private readonly NekoClient _nekoClient;
		public Waifu(ILogger<Waifu> logger, NekoClient nekoClient) : base(logger) { _nekoClient = nekoClient; }

		[Command]
		[Summary("Displays an image of your new waifu!")]
		public async Task WaifuCommand()
		{
			var image = await _nekoClient.Image_v3.Waifu();
			var embed = new KaguyaEmbedBuilder(KaguyaColors.Green).WithDescription($"{Context.User.Mention} here is your new waifu!")
			                                                      .WithImageUrl(image.ImageUrl)
			                                                      .Build();

			await SendEmbedAsync(embed);
		}
	}
}