using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using Victoria.Enums;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class DeleteTrack : KaguyaBase
    {
        [MusicCommand]
        [Command("DeleteTrack")]
        [Alias("deltrack", "dt")]
        [Summary("Deletes a track from the current music player's queue.")]
        [Remarks("<track num>\n3 (=> Removes track #3 from the queue)")]
        public async Task Command(int num)
        {
            var player = ConfigProperties.LavaNode.GetPlayer(Context.Guild);
            var playerState = player.PlayerState;
            var queue = player.Queue;

            if (!(playerState == PlayerState.Playing || playerState == PlayerState.Paused) || !queue.Any())
            {
                await SendBasicErrorEmbedAsync("The player must be either playing " +
                                               "or paused and must have at least 1 song " +
                                               "in the queue.");
                return;
            }

            if (player.Queue.ElementAtOrDefault(num - 1) != null)
            {
                
            }
        }
    }
}