using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using Victoria;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class FavoriteTrackCommand : KaguyaBase
    {
        [MusicCommand]
        [Command("FavoriteTrack")]
        [Alias("fav", "favorite")]
        [Summary("Allows any user to `favorite` the currently playing track. This will save the track " +
                 "in a playlist that can be accessed by the `favls` command.\n\n" +
                 "Limit: `50 tracks`.\n" +
                 "[Kaguya Premium](" + ConfigProperties.KaguyaStore + ") limit: `500 tracks`.")]
        [Remarks("")]
        public async Task Command()
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            
            LavaNode node = ConfigProperties.LavaNode;
            if (!node.TryGetPlayer(Context.Guild, out LavaPlayer player) || player == null)
            {
                await SendBasicErrorEmbedAsync($"There needs to be an active music player in the " +
                                               $"server for this command to work. Start one " +
                                               $"by using `{server.CommandPrefix}play <song>`!");
                return;
            }
            
            if (!player.IsPlaying() && !player.IsPaused())
            {
                await SendBasicErrorEmbedAsync($"The music player must either be paused or actively playing " +
                                               $"in order to favorite a track.");
                return;
            }

            var curTrack = player.Track;
            var userFavorites = await DatabaseQueries.GetAllForUserAsync<FavoriteTrack>(user.UserId);
            
            var favTrack = new FavoriteTrack
            {
                UserId = user.UserId,
                TrackId = curTrack.Id,
                TrackTitle = curTrack.Title,
                TrackAuthor = curTrack.Author,
                TrackDuration = curTrack.Duration.TotalSeconds
            };

            if (userFavorites.Any(x => x.UserId == user.UserId && x.TrackId == curTrack.Id))
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} You have already favorited this track!");
                return;
            }

            if (userFavorites.Count >= 50 && !user.IsPremium || userFavorites.Count >= 500 && user.IsPremium)
            {
                await SendBasicErrorEmbedAsync("You cannot favorite anymore tracks. Use the `delfav` command to make " +
                                               "room!");
            }
            
            await DatabaseQueries.InsertAsync(favTrack);

            var embed = new KaguyaEmbedBuilder(EmbedColor.GREEN)
            {
                Title = "Favorite Track",
                Description = $"{Context.User.Mention} successfully favorited `{curTrack.Title}` by " +
                              $"`{curTrack.Author}`."
            };

            await SendEmbedAsync(embed);
        }
    }
}