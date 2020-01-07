using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class DailyLoot : ModuleBase<ShardedCommandContext>
    {
        [CurrencyCommand]
        [Command("DailyLoot")]
        [Alias("daily", "d")]
        [Summary("Claim your daily loot and receive rewards!")]
        [Remarks("")]
        public async Task Command()
        {
            Random r = new Random();

            var points = r.Next(35, 210);
            var fishType = (FishType)r.Next(5, 14);


        }
    }
}
