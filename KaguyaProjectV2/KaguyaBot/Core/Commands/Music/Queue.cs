using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using Victoria;
using Victoria.Enums;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Queue : KaguyaBase
    {
        [DisabledCommand]
        [MusicCommand]
        [Command("Queue")]
        [Alias("q")]
        [Summary("Displays the current music player's queue, up to the next 50 songs.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            LavaNode node = ConfigProperties.LavaNode;
            LavaPlayer player = node.GetPlayer(Context.Guild);

            if (player == null)
            {
                await SendBasicErrorEmbedAsync($"There needs to be an active music player in the " +
                                               $"server for this command to work. Start one " +
                                               $"by using `{server.CommandPrefix}play <song>`!");

                return;
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Kaguya Music: Queue {Centvrio.Emoji.Office.SpiralNotepad}",
                Description = ""
            };

            if (player.Queue.Count == 0 && player.PlayerState != PlayerState.Playing)
            {
                await SendBasicErrorEmbedAsync($"The current player is not playing a song and has nothing in the queue.");

                return;
            }

            if (player.Queue.Count == 0 && player.PlayerState == PlayerState.Playing)
            {
                embed.Description = $"**Now Playing:** [{player.Track.Title}]({player.Track.Url})\n" +
                                    $"No items in queue.";
            }

            if (player.Queue.Count > 0 && player.PlayerState == PlayerState.Playing)
            {
                embed.Description = $"**Now Playing:** [{player.Track.Title}]({player.Track.Url})\n\n" +
                                    $"**Up Next: {Centvrio.Emoji.AudioVideo.FastDown}\n**";

                for (int i = 0; i < (player.Queue.Count < 50 ? player.Queue.Count : 50); i++)
                {
                    embed.Description += $"`#{i + 1}.` [{((LavaTrack) player.Queue.ToList()[i]).Title}]" +
                                         $"({((LavaTrack) player.Queue.ToList()[i]).Url})\n";
                }
            }

            await SendEmbedAsync(embed);
        }
    }
}