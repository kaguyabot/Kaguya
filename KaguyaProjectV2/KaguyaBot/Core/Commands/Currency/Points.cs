using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

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
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var points = user.Points;

            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} currently has " +
                                                             $"`{points:N0}` points.");
        }
    }
}
