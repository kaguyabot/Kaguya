using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class DailyLoot : ModuleBase<ShardedCommandContext>
    {
        [CurrencyCommand]
        [Command("DailyLoot")]
        [Alias("daily", "d")]
        [Summary("Claim your daily loot or give it to somebody else! Rewards increased " +
                 "if given to another user.")]
        [Remarks("<user>")]
        public async Task Command()
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (!user.CanGetTimelyPoints)
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"You must wait " +
                                                               $"`{(DateTime.Now - DateTime.FromOADate(user.LatestTimelyBonus)).Humanize()}` " +
                                                               $"before you may claim this bonus.");
                return;
            }

            Random r = new Random();

            var points = r.Next(35, 210);
            var exp = r.Next(8, 112);


        }
    }
}
