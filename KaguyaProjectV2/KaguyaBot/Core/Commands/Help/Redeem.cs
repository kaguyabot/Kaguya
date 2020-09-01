using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Net;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Redeem : KaguyaBase
    {
        private const double MONTHLY_SERVER_FEE = 80.99;
        private const double PREMIUM_COST = 4.99;

        [HelpCommand]
        [Command("Redeem")]
        [Summary("Allows a user to redeem a Kaguya Premium key in a server. Premium keys may be " +
                 "purchased [at this link](https://sellix.io/KaguyaStore). Be sure to execute this " +
                 "command in the server where you want to redeem your premium key!")]
        [Remarks("<key>")]
        public async Task RedeemKey(params string[] keys)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var existingPremiumKeys = await DatabaseQueries.GetAllAsync<PremiumKey>();

            foreach (var keyString in keys)
            {
                var premiumKey = existingPremiumKeys.FirstOrDefault(x => x.Key == keyString && x.UserId == 0 && x.ServerId == 0);
            
                if (premiumKey == null)
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

                if (!string.IsNullOrEmpty(premiumKey?.Key))
                {
                    newKey = new PremiumKey
                    {
                        Key = premiumKey.Key,
                        LengthInSeconds = premiumKey.LengthInSeconds,
                        KeyCreatorId = premiumKey.KeyCreatorId,
                        UserId = Context.User.Id,
                        ServerId = Context.Guild.Id,
                    };

                    await DatabaseQueries.InsertOrReplaceAsync((PremiumKey)newKey);
                }
                else
                {
                    await Context.Message.DeleteAsync();
                    throw new KaguyaSupportException("Failed to redeem your key. Please " +
                                                       "join our support server for assistance.");
                }

                TimeSpan ts = $"{newKey.LengthInSeconds}s".ParseToTimespan();

                user.TotalDaysSupported += (int)TimeSpan.FromSeconds(newKey.LengthInSeconds).TotalDays;
                int totalDaysSupported = user.TotalDaysSupported;

                var userPremiumExpiration = user.PremiumExpiration;
                if (userPremiumExpiration < DateTime.Now.ToOADate())
                {
                    userPremiumExpiration = DateTime.Now.ToOADate();
                }

                userPremiumExpiration = DateTime.FromOADate(userPremiumExpiration).AddSeconds(premiumKey.LengthInSeconds).ToOADate();
                user.PremiumExpiration = userPremiumExpiration;

                // If the server has never been premium before, or was but it has expired, 
                // we need to reset the expiration to Now + the key's length.
                if (server.PremiumExpiration < DateTime.Now.ToOADate())
                {
                    server.PremiumExpiration = DateTime.Now.AddSeconds(premiumKey.LengthInSeconds).ToOADate();
                }
                else
                {
                    server.PremiumExpiration = DateTime.FromOADate(server.PremiumExpiration)
                        .AddSeconds(premiumKey.LengthInSeconds).ToOADate();
                }
                
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully redeemed `" +
                                  $"{ts.Humanize(maxUnit: TimeUnit.Day)}` of Kaguya Premium!\n" +
                                  $"This server's subscription will expire on: `{DateTime.FromOADate(server.PremiumExpiration).ToLongDateString()}`\n" +
                                  $"Your personal subscription will expire on: `{DateTime.FromOADate(userPremiumExpiration).ToLongDateString()}`\n" +
                                  $"You've supported for `{totalDaysSupported:N0}` days! " +
                                  $"That's `{ServerUptimeCalcInDays(totalDaysSupported):N2} days` of server uptime 💙",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "It may not seem like a lot, but it all adds up. Thanks for your support!"
                    }
                };
                embed.SetColor(EmbedColor.GOLD);

                await ReplyAsync(embed: embed.Build());
#if !DEBUG
                await SendEmbedToBotOwner(Context, newKey);
#endif
                await DatabaseQueries.UpdateAsync(user);
                await DatabaseQueries.UpdateAsync(server);
                
                await ApplyRewardsToUser(user, Context.User, premiumKey);
            }
        }

        private async Task SendEmbedToBotOwner(ICommandContext context, IKey key)
        {
            var owner = Client.GetUser(ConfigProperties.BotConfig.BotOwnerId);
            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "Key Properties",
                    Value = $"Key: `{key.Key}`\nCreated by: `{owner}`\nExpires: " +
                            $"`{DateTime.Now.AddSeconds(key.LengthInSeconds).Humanize(false)}`"
                }
            };

            if (key.GetType() == typeof(PremiumKey))
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"User `{context.User}` has just redeemed a " +
                                  $"Kaguya Premium key!",
                    Fields = fields
                };

                try
                {
                    await owner.SendMessageAsync(embed: embed.Build());
                }
                catch (HttpException)
                {
                    await ConsoleLogger.LogAsync("Attempted to DM an owner a notification about a " +
                                                 "Kaguya Premium key redemption, but a " +
                                                 "Discord.Net.HttpException was thrown.", LogLvl.WARN);
                }
            }
        }

        private async Task ApplyRewardsToUser(User user, IUser discordUser, IKey key)
        {
            int points = (int)(25000 * (TimeSpan.FromSeconds(key.LengthInSeconds).TotalDays / 30));
            user.AddPoints(points);

            var embed = new KaguyaEmbedBuilder(EmbedColor.GOLD)
            {
                Description = $"{discordUser.Mention} You have been awarded `{points:N0}` points!"
            };

            await DatabaseQueries.UpdateAsync(user);
            await discordUser.SendMessageAsync(embed: embed.Build());
        }
        
        private double ServerUptimeCalcInDays(int totalDaysSupported)
        {
            // 13 cents per day for supporter.
            // $2.69 per day for the server.
            // 1 day of supporter = 0.04 days of server uptime
            return PREMIUM_COST / 30 / (MONTHLY_SERVER_FEE / 30) * totalDaysSupported;
        }
    }
}
