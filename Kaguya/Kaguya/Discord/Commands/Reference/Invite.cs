using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("invite")]
    public class Invite : KaguyaBase<Invite>
    {
        private readonly ILogger<Invite> _logger;

        public Invite(ILogger<Invite> logger) : base(logger) { _logger = logger; }

        [Command]
        [Summary("Displays an invitation URL for Kaguya. You can use this to add Kaguya to your " +
                 "Discord servers.")]
        public async Task InviteCommand()
        {
            // todo: change to live URL before real deployment.
            await SendBasicSuccessEmbedAsync($"[(BETA) Here you go!]({Global.BetaInviteUrl})");
        }
    }
}