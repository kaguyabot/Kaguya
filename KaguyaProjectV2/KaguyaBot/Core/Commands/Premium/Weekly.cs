using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Premium
{
    public class Weekly : KaguyaBase
    {
        [PremiumUserCommand]
        [CurrencyCommand]
        [Command("Weekly")]
        [Alias("week", "wk")]
        [Summary("Grants the user a generous amount of bonus points, redeemable once per week.")]
        [Remarks("")]
        public async Task Command()
        {
            const int PAYOUT = 7500;
            
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            if (user.CanGetWeeklyPoints)
            {
                user.Points += PAYOUT;
                user.LastWeeklyBonus = DateTime.Now.ToOADate();

                try
                {
                    await DatabaseQueries.UpdateAsync(user);
                }
                catch (Exception e)
                {
                    throw new KaguyaSupportException("An error occurred when trying to add your weekly points! " +
                                                     "Your weekly cooldown has not been triggered. Please report this bug!");
                }
                
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Weekly Reward | 💰",
                    Description = $"{Context.User.Mention} You have successfully redeemed your weekly bonus!\n" +
                                  $"Bonus Points: `{PAYOUT:N0}`",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"New points balance: {user.Points:N0}"
                    }
                };

                await SendEmbedAsync(embed);
            }
            else
            {
                var ts = DateTime.FromOADate(user.LastWeeklyBonus).AddDays(7) - DateTime.Now;
                await SendBasicErrorEmbedAsync($"You must wait `{ts.Humanize(2)}` " +
                                               $"before you may claim this bonus.");
            }
        }
    }
}