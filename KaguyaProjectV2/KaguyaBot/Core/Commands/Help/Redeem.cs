using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Redeem : ModuleBase<ShardedCommandContext>
    {
        private const double MONTHLY_SERVER_FEE = 80.99;
        private const double AVERAGE_MONTHLY_SUPPORTER_PAYMENT = 3.99;

        [HelpCommand]
        [Command("Redeem")]
        [Summary("Allows a user to redeem a Kaguya Supporter or Kaguya Premium key. Supporter Keys may be " +
                 "purchased [at this link](https://stageosu.selly.store/)")]
        [Remarks("<key>")]
        public async Task RedeemKey(string userKey)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var existingSupporterKeys = await DatabaseQueries.GetAllAsync<SupporterKey>();
            var existingPremiumKeys = await DatabaseQueries.GetAllAsync<PremiumKey>();

            var supporterKey = existingSupporterKeys.FirstOrDefault(x => x.Key == userKey && x.UserId == 0);
            var premiumKey = existingPremiumKeys.FirstOrDefault(x => x.Key == userKey && x.UserId == 0 && x.ServerId == 0);

            if (supporterKey == null && premiumKey == null)
            {
                await Context.Message.DeleteAsync();
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

            IKey newKey;
            var typeString = "";
            var nameSwitch = "";
            double expirationDate = 0;

            if (supporterKey != null)
            {
                newKey = new SupporterKey
                {
                    Key = supporterKey.Key,
                    LengthInSeconds = supporterKey.LengthInSeconds,
                    KeyCreatorId = supporterKey.KeyCreatorId,
                    UserId = Context.User.Id,
                    Expiration = DateTime.Now.AddSeconds(supporterKey.LengthInSeconds).ToOADate()
                };

                typeString = "Kaguya Supporter";
                nameSwitch = "Your";
                expirationDate = user.SupporterExpirationDate;

                await DatabaseQueries.InsertOrReplaceAsync((SupporterKey) newKey);
            }

            else if (!string.IsNullOrEmpty(premiumKey.Key))
            {
                newKey = new PremiumKey
                {
                    Key = premiumKey.Key,
                    LengthInSeconds = premiumKey.LengthInSeconds,
                    KeyCreatorId = premiumKey.KeyCreatorId,
                    UserId = Context.User.Id,
                    ServerId = Context.Guild.Id,
                    Expiration = DateTime.Now.AddSeconds(premiumKey.LengthInSeconds).ToOADate()
                };

                typeString = "Kaguya Premium";
                nameSwitch = "This server's";
                expirationDate = server.PremiumExpirationDate;

                await DatabaseQueries.InsertOrReplaceAsync((PremiumKey)newKey);
            }

            #region Useless code to avoid compiler errors. -_-

            else
            {
                // This is only here to avoid compiler errors -_-
                newKey = new SupporterKey();
            }

            #endregion

            TimeSpan ts = $"{newKey.LengthInSeconds}s".ParseToTimespan();
            expirationDate += DateTime.Now.AddSeconds(ts.TotalSeconds).ToOADate();
            expirationDate -= DateTime.Now.ToOADate();

            user.TotalDaysSupported += (int)TimeSpan.FromSeconds(newKey.LengthInSeconds).TotalDays;
            int totalDaysSupported = user.TotalDaysSupported;

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully redeemed `" +
                              $"{ts.Humanize(maxUnit: TimeUnit.Day)}` of {typeString}!\n" +
                              $"{nameSwitch} subscription will expire on: `{DateTime.FromOADate(expirationDate).ToLongDateString()}`\n" +
                              $"You've supported for `{totalDaysSupported:N0}` days! " +
                              $"That's `{ServerUptimeCalcInDays(totalDaysSupported):N2} days` of server uptime 💙",
                Footer = new EmbedFooterBuilder
                {
                    Text = "It may not seem like a lot, but it all adds up. Thanks for your support!"
                }
            };
            embed.SetColor(EmbedColor.GOLD);

            await ReplyAsync(embed: embed.Build());
            await SendEmbedToBotOwner(Context, newKey);
            await DatabaseQueries.UpdateAsync(user);
        }

        private async Task SendEmbedToBotOwner(ICommandContext context, IKey key)
        {
            var owner = ConfigProperties.Client.GetUser(ConfigProperties.BotConfig.BotOwnerId);
            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder 
                {
                    IsInline = false, 
                    Name = "Key Properties",
                    Value = $"Key: `{key.Key}`\nCreated by: `{owner}`\nExpires " +
                            $"`{DateTime.FromOADate(key.Expiration).Humanize(false)}`"
                }
            };

            if (key.GetType() == typeof(SupporterKey))
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"User `{context.User}` has just redeemed a " +
                                  $"Kaguya Supporter key!",
                    Fields = fields
                };

                await owner.SendMessageAsync(embed: embed.Build());
            }

            if (key.GetType() == typeof(PremiumKey))
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"User `{context.User}` has just redeemed a " +
                                  $"Kaguya Premium key!",
                    Fields = fields
                };

                await owner.SendMessageAsync(embed: embed.Build());
            }
        }

        private double ServerUptimeCalcInDays(int totalDaysSupported)
        {
            // 13 cents per day for supporter.
            // $2.69 per day for the server.
            // 1 day of supporter = 0.04 days of server uptime
            return (AVERAGE_MONTHLY_SUPPORTER_PAYMENT / 30) / (MONTHLY_SERVER_FEE / 30) * totalDaysSupported;
        }
    }
}
