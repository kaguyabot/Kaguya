using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

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
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            if (!user.CanGetDailyPoints)
            {
                var dt = DateTime.FromOADate(user.LastDailyBonus).AddHours(24);
                await SendBasicErrorEmbedAsync($"You must wait `{dt.Humanize(false)}` " +
                                               $"before you may claim this bonus.");
                return;
            }

            int points = 750;
            int exp = 275;

            // Premium bonuses
            if (await user.IsPremiumAsync() || server.IsPremium)
            {
                points = (int)(points * 1.50);
                exp = (int)(exp * 1.50);
            }
            
            user.Points += points;
            user.Experience += exp;
            user.LastDailyBonus = DateTime.Now.ToOADate();

            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} You have received " +
                                             $"`+{points:N0} points` and `+{exp:N0} exp`!");
            await DatabaseQueries.UpdateAsync(user);
        }
    }
}
