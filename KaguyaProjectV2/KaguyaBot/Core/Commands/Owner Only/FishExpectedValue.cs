using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class FishExpectedValue : KaguyaBase
    {
        [OwnerCommand]
        [Command("FishCalc")]
        [Alias("fc")]
        [Summary("Displays how many points the user earned for their last `<amount>` fish as well as " +
                 "how much they paid to play.")]
        [Remarks("<user id> [amount]")]
        public async Task Command(ulong userId, int amount = 100)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(userId);
            IEnumerable<Fish> recentFish = user.Fish.OrderByDescending(x => x.TimeCaught).Take(amount);
            int value = recentFish.Select(x => x.Value).Sum();
            int baitCost = user.FishCost() * amount;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Fish Bait to Value Ratio",
                Description = $"User {Client.GetUser(userId)?.ToString() ?? userId.ToString()}'s last {amount} fish stats:\n\n" +
                              $"Total cost of bait spent at this level: `{baitCost:N0}`\n" +
                              $"Total value of the most recent {amount} fish: `{value:N0}`"
            };

            await SendEmbedAsync(embed);
        }
    }
}