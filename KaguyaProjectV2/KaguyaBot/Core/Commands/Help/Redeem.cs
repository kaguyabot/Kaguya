using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Redeem : ModuleBase<ShardedCommandContext>
    {
        [HelpCommand]
        [Command("redeem")]
        [Summary("Allows a user to redeem a Kaguya Supporter key. Supporter Keys may be " +
                 "purchased [at this link](https://stageosu.selly.store/)")]
        [Remarks("<key>")]
        public async Task RedeemKey(string userKey)
        {
            var existingKeys = UtilityQueries.GetAllKeys();
            var key = existingKeys.FirstOrDefault(x => x.Key == userKey && x.UserId == 0);

            if (key == null) //Could need to do try-catch instead.
                return;

        }
    }
}
