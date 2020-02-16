using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Promote : KaguyaBase
    {
        [MusicCommand]
        [Command("Promote")]
        [Summary("Promotes a track from the queue to the top of the queue. This would " +
                 "make the track play immediately after the currently playing song. Find out " +
                 "which song you want to promote by using the `queue` command first.")]
        [Remarks("<track num>\n6")]
        // [RequireUserPermission()]
        // [RequireBotPermission()]
        [RequireContext(ContextType.Guild)]
        public async Task Command(int num)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var node = ConfigProperties.LavaNode;
            var player = node.GetPlayer(Context.Guild);

            if (player == null)
            {
                await SendBasicErrorEmbedAsync($"There needs to be an active music player in the " +
                                               $"server for this command to work. Start one " +
                                               $"by using `{server.CommandPrefix}play <song>`!");
                return;
            }

            switch (player.Queue.Count)
            {
                case 0:
                    await SendBasicErrorEmbedAsync("There are no songs to promote because there's nothing in the queue.");
                    return;
                case 1:
                    await SendBasicErrorEmbedAsync("There's only one song in the queue, therefore it will be playing next.");
                    return;
            }

            if (num < 1)
            {
                await SendBasicErrorEmbedAsync("The available range of tracks to promote starts at `1`. " +
                                               $"Please find the number for your track using the `{server.CommandPrefix}queue` command.");
                return;
            }

            if (num > player.Queue.Items.Count())
            {
                await SendBasicErrorEmbedAsync($"The requested track doesn't exist in your queue. Please use " +
                                               $"the `{server.CommandPrefix}queue` command to see all " +
                                               $"available tracks to promote.");
                return;
            }

            var oldQueueTracks = player.Queue.Items.Where(x => x.GetType() == typeof(LavaTrack)).ToList();
            var track = (LavaTrack)oldQueueTracks[num - 1];

            player.Queue.Clear();
            player.Queue.Enqueue(track);

            foreach (var item in oldQueueTracks)
            {
                if (!item.Equals(track))
                    player.Queue.Enqueue(item);
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Kaguya Music: Promote {Centvrio.Emoji.AwardMedal.FirstPlace}",
                Description = $"{Context.User.Mention} Successfully promoted `{track.Title}` to the front of the queue."
            };

            await SendEmbedAsync(embed);
        }
    }
}
