using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

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
        public async Task Command([Remainder]SocketGuildUser guildUser = null)
        {
            User targetUser = null;
            User curUser = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (guildUser != null && guildUser.IsBot)
            {
                await SendBasicErrorEmbedAsync("Your daily loot cannot be given to a bot.");
                return;
            }

            if (guildUser == (SocketGuildUser) Context.User)
            {
                await SendBasicErrorEmbedAsync("You cannot award yourself with bonus loot. You must execute this command " +
                                               "without mentioning yourself.");
                return;
            }
            
            if (guildUser != null)
                targetUser = await DatabaseQueries.GetOrCreateUserAsync(guildUser.Id);
            
            if (!curUser.CanGetDailyPoints)
            {
                var ts = DateTime.FromOADate(curUser.LastDailyBonus).AddHours(24) - DateTime.Now;
                await SendBasicErrorEmbedAsync($"You must wait `{ts.Humanize(2)}` " +
                                               $"before you may claim this bonus.");
                return;
            }

            int points = 750;
            int exp = 275;

            // Premium bonuses
            if (await curUser.IsPremiumAsync() && targetUser == null)
            {
                points *= 2;
                exp *= 2;
            }

            if (targetUser != null)
            {
                points = (int)(points * 1.07);
                exp = (int)(exp * 1.07);
            }
            
            if (targetUser == null)
            {
                curUser.Points += points;
                curUser.Experience += exp;
            }
            else
            {
                targetUser.Points += points;
                targetUser.Experience += exp;
            }
            
            curUser.LastDailyBonus = DateTime.Now.ToOADate();

            if (targetUser == null)
            {
                await SendBasicSuccessEmbedAsync($"{Context.User.Mention} You have received " +
                                                 $"`+{points:N0} points` and `+{exp:N0} exp`!");
            }
            else
            {
                await SendBasicSuccessEmbedAsync($"{guildUser.Mention} You have received " +
                                                 $"`+{points:N0} points` and `+{exp:N0} exp` from {Context.User.Mention}!");
                await DatabaseQueries.UpdateAsync(targetUser);
            }
         
            await DatabaseQueries.UpdateAsync(curUser);
        }
    }
}
