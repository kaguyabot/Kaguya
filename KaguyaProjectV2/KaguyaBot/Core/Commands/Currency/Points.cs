using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class ViewPoints : KaguyaBase
    {
        [CurrencyCommand]
        [Command("Points")]
        [Summary("Displays the user's total points.")]
        [Remarks("")]
        public async Task Command()
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            int points = user.Points;

            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} you have " +
                                             $"`{points:N0}` points.");
        }
    }
}