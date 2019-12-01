using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
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
            var user = UserQueries.GetUser(Context.User.Id);
            var server = ServerQueries.GetServer(Context.Guild.Id);
            var existingKeys = UtilityQueries.GetAllKeys();

            var key = existingKeys.FirstOrDefault(x => x.Key == userKey && x.UserId == 0);

            if (key == null)
            {
                var embed0 = new KaguyaEmbedBuilder
                {
                    Description = "Key does not exist or has already been redeemed.",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"If you need assistance, please join the server provided in {server.CommandPrefix}invite"
                    }

                };
                embed0.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed0.Build());
                return;
            }

            var newKey = new SupporterKey
            {
                Key = key.Key,
                Length = key.Length,
                UserId = Context.User.Id,
                Expiration = DateTime.Now.AddDays(key.Length).ToOADate()
            };

            UtilityQueries.UpdateKey(key, newKey);

            string s = ""; //Grammar
            if (newKey.Length != 1)
                s = "s";

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully redeemed `{newKey.Length} day{s}` of Kaguya Supporter!\n" +
                              $"Your tag will expire on: `{DateTime.FromOADate(user.SupporterExpirationDate).ToLongDateString()}`"
            };
            embed.SetColor(EmbedColor.GOLD);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
