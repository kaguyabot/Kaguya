using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class DailyLoot : KaguyaBase
    {
        [CurrencyCommand]
        [Command("DailyLoot")]
        [Alias("daily", "d", "timely")]
        [Summary("Claim your daily loot or give it to somebody else! Rewards increased " +
                 "if given to another user.")]
        [Remarks("<user>")]
        public async Task Command(SocketGuildUser guildUser = null)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            if (!user.CanGetDailyPoints)
            {
                var dt = DateTime.FromOADate(user.LatestDailyBonus).AddHours(24);
                await SendBasicErrorEmbedAsync($"You must wait `{dt.Humanize(false)}` " +
                                               $"before you may claim this bonus."); 
                return;
            }

            var r = new Random();
            var points = r.Next(35, 700);
            var exp = r.Next(8, 112);

            user.Points += points;
            user.Experience += exp;
            user.LatestDailyBonus = DateTime.Now.ToOADate();

            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Successfully received " +
                                             $"`+{points} points` and `+{exp} exp`!");
            await DatabaseQueries.UpdateAsync(user);
        }
    }
}
