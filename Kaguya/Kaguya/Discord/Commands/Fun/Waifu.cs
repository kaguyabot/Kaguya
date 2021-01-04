using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Discord.Attributes.Enums;
using NekosSharp;

namespace Kaguya.Discord.Commands.Fun
{
    [Module(CommandModule.Fun)]
    [Group("waifu")]
    public class Waifu : KaguyaBase<Waifu>
    {
        private readonly ILogger<Waifu> _logger;
        private readonly NekoClient _nekoClient;
        
        public Waifu(ILogger<Waifu> logger, NekoClient nekoClient) : base(logger)
        {
            _logger = logger;
            _nekoClient = nekoClient;
        }

        [Command]
        [Summary("Displays an image of your new waifu!")]
        public async Task WaifuCommand()
        {
            var image = await _nekoClient.Image_v3.Waifu();
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
                        .WithDescription($"{Context.User.Mention} here is your new waifu!")
                        .WithImageUrl(image.ImageUrl)
                        .Build();

            await SendEmbedAsync(embed);
        }
    }
}