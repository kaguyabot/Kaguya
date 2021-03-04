using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.Configuration
{
    [Module(CommandModule.Configuration)]
    [Group("togglelevels")]
    [Alias("tl")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Summary("Toggles all level announcement notifications on or off for the server. Level announcements " +
             "are on by default.")]
    public class ToggleLevels : KaguyaBase<ToggleLevels>
    {
        private readonly ILogger<ToggleLevels> _logger;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        public ToggleLevels(ILogger<ToggleLevels> logger, KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaServerRepository = kaguyaServerRepository;
        }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        public async Task ToggleLevelsCommandAsync()
        {
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
            server.LevelAnnouncementsEnabled = !server.LevelAnnouncementsEnabled;

            if (server.LevelAnnouncementsEnabled)
            {
                await SendBasicSuccessEmbedAsync("Successfully enabled all level announcements.");
            }
            else
            {
                await SendBasicSuccessEmbedAsync("Successfully disabled all level announcements.");
            }

            await _kaguyaServerRepository.UpdateAsync(server);
        }
    }
}