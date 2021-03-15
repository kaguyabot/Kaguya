using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using NekosSharp;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Fun
{
	[Module(CommandModule.Fun)]
	[Group("avatar")]
	[Alias("avi", "av")]
	public class Avatar : KaguyaBase<Avatar>
	{
		private readonly NekoClient _nekoClient;
		public Avatar(ILogger<Avatar> logger, NekoClient nekoClient) : base(logger) { _nekoClient = nekoClient; }

		[Command]
		[Summary("Generates an avatar perfect for you!")]
		public async Task AvatarCommand()
		{
			var avatar = await _nekoClient.Image_v3.Avatar();
			var embed = new KaguyaEmbedBuilder(KaguyaColors.Green).WithDescription($"{Context.User.Mention} here is a new avatar!")
			                                                      .WithImageUrl(avatar.ImageUrl)
			                                                      .Build();

			await SendEmbedAsync(embed);
		}
	}
}