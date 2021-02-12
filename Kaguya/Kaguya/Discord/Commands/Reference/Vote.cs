using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("vote")]
    [Alias("upvote", "uv")]
    [Summary("Displays a link to vote for Kaguya on top.gg. Voting online automatically " +
             "earns you rewards!")]
    public class Vote : KaguyaBase<Vote>
    {
        private readonly ILogger<Vote> _logger;

        public Vote(ILogger<Vote> logger) : base(logger) { _logger = logger; }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        public async Task UpvoteLinkCommandAsync()
        {
            string url = Global.TopGgUpvoteUrl;

            await SendBasicSuccessEmbedAsync($"Upvote on [top.gg]({url}) for bonus rewards!");
        }
    }
}