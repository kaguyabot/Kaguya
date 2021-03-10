using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("premium")]
	[Summary("Displays a link to the official Kaguya store and the Kaguya Premium wiki page.")]
	public class Premium : KaguyaBase<Premium>
	{
		public Premium(ILogger<Premium> logger) : base(logger) {}

		[Command]
		[InheritMetadata(CommandMetadata.Summary)]
		public async Task PremiumReferenceCommandAsync(params string[] args)
		{
			string store = Global.StoreUrl;
			string wiki = Global.WikiPremiumBenefitsUrl;

			await SendBasicSuccessEmbedAsync("Check out Kaguya Premium!\n" +
			                                 $"💰 [Kaguya Store]({store})".AsBold() +
			                                 " | " +
			                                 $"🗒️ [Premium Wiki]({wiki})".AsBold());
		}
	}
}