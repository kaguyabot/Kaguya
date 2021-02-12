using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("changelog")]
    [Summary("Displays a link to the changelog.")]
    public class Changelog : KaguyaBase<Changelog>
    {
        public Changelog(ILogger<Changelog> logger) : base(logger) { }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        // We declare params string[] args because it doesn't matter if people pass anything in or not,
        // we want to avoid errors where possible.
        public async Task ChangelogCommandAsync(params string[] args)
        {
            await SendBasicSuccessEmbedAsync($"[Here you go! (Changelog)]({Global.WikiChangelog})");
        }
    }
}