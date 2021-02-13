using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("premium")]
    [Summary("Displays a link to the official Kaguya store and the Kaguya Premium wiki page.")]
    public class Premium : KaguyaBase<Premium>
    {
        private readonly ILogger<Premium> _logger;

        public Premium(ILogger<Premium> logger) : base(logger) { _logger = logger; }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        public async Task PremiumReferenceCommandAsync(params string[] args)
        {
            string store = Global.StoreUrl;
            string wiki = Global.WikiPremiumBenefitsUrl;

            await SendBasicSuccessEmbedAsync("Check out Kaguya Premium!\n" +
                                             $"💰 [Kaguya Store]({store})".AsBold() + " | " + $"🗒️ [Premium Wiki]({wiki})".AsBold());
        }
    }
}