using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using Discord;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public static class MusicService
    {
        public static async Task OnTrackEnd(TrackEndedEventArgs e)
        {
            var player = e.Player;
            var queue = player.Queue;

            if(player != null && queue?.Count > 0 && e.Reason.ShouldPlayNext())
            {
                var success = queue.TryDequeue(out var val);
                if(success)
                {
                    var track = (LavaTrack)val;
                    await player.PlayAsync(track);

                    if(player.TextChannel != null)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"Songs left in queue: `{queue.Count:N0}`");
                        sb.AppendLine($"Title: `{track.Title}`");
                        sb.AppendLine($"Duration: `{track.Duration.Humanize(2)}`");
                        sb.AppendLine($"Track Name: `{track.Title}`");

                        var embed = new KaguyaEmbedBuilder
                        {
                            Title = $"New Track",
                            Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder
                                {
                                    Name = "Track Information",
                                    Value = sb.ToString()
                                }
                            }
                        };

                        await player.TextChannel.SendEmbedAsync(embed);
                    }

                    await ConsoleLogger.LogAsync("Successfully continued the music " + 
                    $"queue in guild {e.Player.VoiceChannel.Guild}", LogLvl.DEBUG);
                }
            }
        }
    }
}