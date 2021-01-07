using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using NekosSharp;

namespace Kaguya.Discord.Commands.Fun
{
    [Module(CommandModule.Fun)]
    [Group("avatar")]
    [Alias("avi", "av")]
    public class Avatar : KaguyaBase<Avatar>
    {
        private readonly ILogger<Avatar> _logger;
        private readonly NekoClient _nekoClient;
        public Avatar(ILogger<Avatar> logger, NekoClient nekoClient) : base(logger)
        {
            _logger = logger;
            _nekoClient = nekoClient;
        }

        [Command]
        [Summary("Generates an avatar perfect for you!")]
        public async Task AvatarCommand()
        {
            var avatar = await _nekoClient.Image_v3.Avatar();
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
                        .WithDescription($"{Context.User.Mention} here is a new avatar!")
                        .WithImageUrl(avatar.ImageUrl)
                        .Build();
            

            await SendEmbedAsync(embed);
        }
    }
}