using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using NekosSharp;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Fun
{
	[Module(CommandModule.Fun)]
	[Group("wallpaper")]
	public class Wallpaper : KaguyaBase<Wallpaper>
	{
		private readonly NekoClient _nekoClient;
		public Wallpaper(ILogger<Wallpaper> logger, NekoClient nekoClient) : base(logger) { _nekoClient = nekoClient; }

		[Command]
		[Summary("Displays a random desktop wallpaper.")]
		public async Task WallpaperCommand()
		{
			var randomWallpaper = await _nekoClient.Image_v3.Wallpaper();
			var embed = new KaguyaEmbedBuilder(KaguyaColors.Blue).WithTitle("Wallpaper")
			                                                     .WithDescription($"{Context.User.Mention}")
			                                                     .WithImageUrl(randomWallpaper.ImageUrl);

			await SendEmbedAsync(embed);
		}
	}
}