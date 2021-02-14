using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("leave")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Leave : KaguyaBase<Leave>
    {
        private readonly ILogger<Leave> _logger;
        private readonly LavaNode _lavaNode;
        
        public Leave(ILogger<Leave> logger, LavaNode lavaNode) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
        }

        [Command]
        [Summary("Disconnects me from the current voice channel.")]
        public async Task LeaveCommand()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendBasicErrorEmbedAsync("I couldn't find the music player for this server. Please ensure I am connected to a voice channel " +
                                               "before using this command.");

                return;
            }

            string vcName = player.VoiceChannel.Name.AsBold();

            try
            {
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to disconnect from {vcName}. If the issue " +
                                               $"persists, please contact the developers for support.");
                _logger.LogError(e, $"Failed to disconnect from voice channel '{vcName}' in {Context.Guild.Id} via $leave.");

                return;
            }
            await SendBasicSuccessEmbedAsync($"Disconnected from {vcName}.");
        }
    }
}