using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord.Rest;
using Kaguya.Discord.Attributes.Enums;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("fetch")]
    public class Fetch : KaguyaBase<Fetch>
    {
        private readonly ILogger<Fetch> _logger;

        public Fetch(ILogger<Fetch> logger) : base(logger) { _logger = logger; }

        [Command]
        [Summary("Gets some basic information about a user.")]
        [Remarks("<user id>")]
        public async Task FetchCommand(ulong userId = 1)
        {
            RestUser info = await Context.Client.Rest.GetUserAsync(userId);
            var embed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow)
                .WithDescription("Test");

            await SendEmbedAsync(embed);
        }
    }
}