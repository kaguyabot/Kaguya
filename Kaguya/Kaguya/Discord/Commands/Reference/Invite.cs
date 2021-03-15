using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("invite")]
	public class Invite : KaguyaBase<Invite>
	{
		private readonly IOptions<AdminConfigurations> _adminConfig;
		public Invite(ILogger<Invite> logger, IOptions<AdminConfigurations> adminConfig) : base(logger) { _adminConfig = adminConfig; }

		[Command]
		[Summary("Displays an invitation URL for Kaguya. You can use this to add Kaguya to your " + "Discord servers.")]
		public async Task InviteCommand()
		{
			ulong ownerId = _adminConfig.Value.OwnerId;
			ulong curUserId = Context.User.Id;

			if (ownerId == curUserId)
			{
				await SendBasicSuccessEmbedAsync(
					"Invite Kaguya".AsBold() + $"\n\n[Here you go!]({Global.InviteUrl})\n" + $"[Development]({Global.LocalDebugInviteUrl})",
					false);
			}
			else
			{
				await SendBasicSuccessEmbedAsync("Invite Kaguya".AsBold() + $"\n\n[Here you go!]({Global.InviteUrl})");
			}
		}
	}
}