using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class PlayFavorite : KaguyaBase
    {
        [DisabledCommand]
        [RequireVoteCommand]
        [MusicCommand]
        [Command("PlayFavorite")]
        [Alias("playfav", "pf")]
        [Summary("Allows a user to play a song from their `favorites` playlist. To get this " +
            "list, execute the `favls` command. You also need to have favorited at least one " +
            "track before.")]
        [Remarks("<trackNum>")]
        public async Task Command(int trackNum)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            List<FavoriteTrack> userFavorites = await DatabaseQueries.GetAllForUserAsync<FavoriteTrack>(user.UserId);

            if (!userFavorites.Any())
            {
                await SendBasicErrorEmbedAsync("You have not favorited any tracks. Use the `h favorite` and " +
                                               "`h favls` commands for more information on how to use this system.");

                return;
            }

            int trackIndex = trackNum - 1;
            FavoriteTrack trackMatch;
            try
            {
                trackMatch = userFavorites[trackIndex];
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"The track number you provided doesn't match a track in " +
                                               $"your playlist.");

                return;
            }

            var s = new Search();
            await s.SearchAndPlayAsync(Context, trackMatch.TrackId, true);
        }
    }
}