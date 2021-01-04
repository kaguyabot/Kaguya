using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Discord.Attributes.Enums;
using NekosSharp;

namespace Kaguya.Discord.Commands.Fun
{
    [Module(CommandModule.Fun)]
    [Group("wallpaper")]
    public class Wallpaper : KaguyaBase<Wallpaper>
    {
        private readonly ILogger<Wallpaper> _logger;
        private readonly NekoClient _nekoClient;
        
        public Wallpaper(ILogger<Wallpaper> logger, NekoClient nekoClient) : base(logger)
        {
            _logger = logger;
            _nekoClient = nekoClient;
        }

        [Command]
        [Summary("Displays a random desktop wallpaper.")]
        public async Task WallpaperCommand()
        {
            Request randomWallpaper = await _nekoClient.Image_v3.Wallpaper();
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Blue)
                        .WithTitle("Wallpaper")
                        .WithDescription($"{Context.User.Mention}")
                        .WithImageUrl(randomWallpaper.ImageUrl);

            await SendEmbedAsync(embed);
        }
    }
}