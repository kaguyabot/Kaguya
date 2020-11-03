using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Skip : KaguyaBase
    {
        [MusicCommand]
        [Command("Skip")]
        [Summary("Skips the currently playing song, if there is one.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            LavaNode node = ConfigProperties.LavaNode;
            LavaPlayer player = node.GetPlayer(Context.Guild);

            if (player.Queue.Count > 0)
            {
                var skipEmbed = new KaguyaEmbedBuilder
                {
                    Title = $"Kaguya Music: Skip {Centvrio.Emoji.AudioVideo.FastForward}",
                    Description = $"Successfully skipped `{player.Track.Title}`.\n" +
                                  $"Now playing: `{((LavaTrack) player.Queue.Peek()).Title}`"
                };

                await SendEmbedAsync(skipEmbed);
            }

            try
            {
                await player.SkipAsync();
            }
            catch (Exception)
            {
                var skipEmbed = new KaguyaEmbedBuilder
                {
                    Title = $"Kaguya Music: Skip {Centvrio.Emoji.AudioVideo.FastForward}",
                    Description = $"Successfully skipped `{player.Track.Title}`.\n" +
                                  $"There are no more tracks in the queue."
                };

                await SendEmbedAsync(skipEmbed);

                await player.StopAsync();
            }
        }
    }
}