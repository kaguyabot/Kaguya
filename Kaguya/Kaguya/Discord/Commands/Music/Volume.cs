using System;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("volume")]
    [Alias("v", "vol")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Volume : KaguyaBase<Volume>
    {
        private readonly ILogger<Volume> _logger;
        private readonly LavaNode _lavaNode;
        
        public Volume(ILogger<Volume> logger, LavaNode lavaNode) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
        }

        [Command]
        [Summary("Changes the volume of the music player to the desired value. Range: 0-200. Please be careful using " +
                 "volume levels above 100, it can get extremely loud and damage your hearing.\n\n" +
                 "Use without a parameter to view the current player's volume.")]
        [Remarks("[# volume]")]
        public async Task SetVolumeCommand(int? desiredVolume = null)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendBasicErrorEmbedAsync("I couldn't find a player for this server. Please ensure you have " +
                                               "an active player before using this command.");

                return;
            }
            
            if (!desiredVolume.HasValue)
            {
                var curVolume = player.Volume;

                if (curVolume == 0)
                {
                    await SendBasicEmbedAsync($"The player is muted.", Color.Orange);
                }
                else
                {
                    await SendBasicEmbedAsync($"The current volume is: {curVolume.ToString().AsBold()}.", Color.Teal);
                }

                return;
            }

            int volVal = desiredVolume.Value;
            switch (volVal)
            {
                case < 0:
                    await SendBasicErrorEmbedAsync("The volume may not be set to a negative value.");

                    break;
                case > 200:
                    await SendBasicErrorEmbedAsync("The maximum volume value is 200.");

                    break;
                default:
                    await player.UpdateVolumeAsync(Convert.ToUInt16(volVal));
                    await SendBasicSuccessEmbedAsync($"Updated the volume to {volVal.ToString().AsBold()}.");
                    break;
            }
        }
    }
}