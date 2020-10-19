using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class DeleteFavorite : KaguyaBase
    {
        [MusicCommand]
        [Command("DeleteFavorite")]
        [Alias("delfav")]
        [Summary("Deletes one or multiple favorited tracks. Tracks are added to this collection via the " +
                 "`favoritetrack` command. The collection can be seen via the `favls` command.\n\n" +
                 "The `tracknum` argument refers to the number assigned to the track in the `favls` list.")]
        [Remarks("<tracknum> {...}\n" +
                 "3 (Removes track #3 from your favorites)\n" +
                 "3 5 7 (Removes tracks #3, #5, and #7 from your favorites)")]
        public async Task Command(params int[] args)
        {
            if (args.Length == 0)
            {
                await SendBasicErrorEmbedAsync($"You need to specify at least one `tracknum` to remove. " +
                                               $"These can be found via the `favls` command.");

                return;
            }

            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            List<FavoriteTrack> userFavoriteTracks = await DatabaseQueries.GetAllForUserAsync<FavoriteTrack>(user.UserId);

            if (!userFavoriteTracks.Any())
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} You do not have any favorited tracks.");

                return;
            }

            const int LIMIT_ATTEMPTS = 3;
            int failedAttempts = 0;

            var successSb = new StringBuilder();
            var errorSb = new StringBuilder();
            var toRemove = new List<FavoriteTrack>(args.Length);
            int i = 0;
            foreach (int num in args)
            {
                if (failedAttempts >= LIMIT_ATTEMPTS)
                {
                    errorSb.AppendLine($"{Context.User.Mention} Too many failed attempts. Aborting further execution.");

                    break;
                }

                FavoriteTrack match = userFavoriteTracks.ElementAtOrDefault(num - 1);
                if (match == null)
                {
                    errorSb.AppendLine($"{Context.User.Mention} Track `#{num}` does not exist in your " +
                                       $"`{server.CommandPrefix}favls` list.");

                    failedAttempts++;

                    continue;
                }

                successSb.AppendLine($"Successfully removed track `#{num}` from your favorites list.");
                toRemove.Insert(i, userFavoriteTracks[num - 1]);
                i++;
            }

            await DatabaseQueries.DeleteAsync(toRemove);
            await ConsoleLogger.LogAsync($"Deleted {args.Length} tracks from user's favorites list [ID: {user.UserId}]",
                LogLvl.DEBUG);

            var finalSb = new StringBuilder();
            if (successSb.Length > 0)
                finalSb.AppendLine(successSb.ToString());

            if (errorSb.Length > 0)
                finalSb.AppendLine(errorSb.ToString());

            var embed = new KaguyaEmbedBuilder(errorSb.Length > 0 ? EmbedColor.VIOLET : EmbedColor.GREEN)
            {
                Title = "Remove Favorite Tracks",
                Description = finalSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}