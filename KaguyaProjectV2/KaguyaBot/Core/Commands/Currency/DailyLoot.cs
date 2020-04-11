﻿using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;

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
                var dt = DateTime.FromOADate(user.LastDailyBonus).AddHours(24);
                await SendBasicErrorEmbedAsync($"You must wait `{dt.Humanize(false)}` " +
                                               $"before you may claim this bonus.");
                return;
            }

            var points = 750;
            var exp = 275;

            user.Points += points;
            user.Experience += exp;
            user.LastDailyBonus = DateTime.Now.ToOADate();

            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} You have received " +
                                             $"`+{points} points` and `+{exp} exp`!");
            await DatabaseQueries.UpdateAsync(user);
        }
    }
}
