using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.UserAccounts;
using Discord;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Core;

namespace Kaguya.Modules.Supporter
{
    public class KaguyaSupporter : ModuleBase<ShardedCommandContext>
    {
        [Command("redeem")]
        public async Task KeyRedeem([Remainder]string key)
        {
            Logger logger = new Logger();
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);

            List<string> thirtyDayKeys = File.ReadAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/GitHub/Kaguya/30DayKeys.txt").ToList();
            List<string> sixtyDayKeys = File.ReadAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/GitHub/Kaguya/60DayKeys.txt").ToList();
            List<string> ninetyDayKeys = File.ReadAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/GitHub/Kaguya/90DayKeys.txt").ToList();

            var stage = Global.client.GetUser(146092837723832320);
            var guild = Servers.GetServer(546880579057221644);

            foreach (string thirtyDayKey in thirtyDayKeys)
            {
                if (key.Contains(thirtyDayKey))
                {
                    userAccount.KaguyaSupporterExpiration = DateTime.Now.AddDays(30);
                    userAccount.Diamonds += 600;
                    thirtyDayKeys.Remove(thirtyDayKey);
                    File.WriteAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/GitHub/Kaguya/30DayKeys.txt", thirtyDayKeys);

                    await GlobalCommandResponses.CreateSuccessfulRedemption(Context, thirtyDayKey,
                        "Successfully Redeemed Supporter Tag!",
                        "Thank you so much for supporting the Kaguya Project! **`30 Days`** of Kaguya Supporter time and **`600 Kaguya Diamonds`** have been added to your account.",
                        "The key you have just redeemed is no longer redeemable.");

                    var dmOwnerChannel = await stage.GetOrCreateDMChannelAsync();
                    var dmUserChannel = await Context.User.GetOrCreateDMChannelAsync();
                    try
                    {
                        await dmOwnerChannel.SendMessageAsync($"{Context.User} in {Context.Guild} has redeemed a supporter tag that's worth 30 days!\nKey used for this tag is `{key}`");
                        await dmUserChannel.SendMessageAsync("", false, GetDmRedeemEmbed(30, 1.12, guild.CommandPrefix.ToLower()));
                    }
                    catch (Exception e)
                    {
                        logger.ConsoleCriticalAdvisory(e.Message);
                    }
                    return;
                }
            }

            foreach (string sixtyDayKey in sixtyDayKeys)
            {
                if (key.Contains(sixtyDayKey))
                {
                    userAccount.KaguyaSupporterExpiration = DateTime.Now.AddDays(60);
                    userAccount.Diamonds += 1200;
                    thirtyDayKeys.Remove(sixtyDayKey);
                    File.WriteAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/GitHub/Kaguya/60DayKeys.txt", thirtyDayKeys);

                    await GlobalCommandResponses.CreateSuccessfulRedemption(Context, sixtyDayKey,
                        "Successfully Redeemed Supporter Tag!",
                        "Thank you so much for supporting the Kaguya Project! **`60 Days`** of Kaguya Supporter time and **`1,200 Kaguya Diamonds`**have been added to your account.",
                        "The key you have just redeemed is no longer redeemable.");

                    var dmOwnerChannel = await stage.GetOrCreateDMChannelAsync();
                    var dmUserChannel = await Context.User.GetOrCreateDMChannelAsync();
                    try
                    {
                        await dmOwnerChannel.SendMessageAsync($"{Context.User} in {Context.Guild} has redeemed a supporter tag that's worth 60 days!\nKey used for this tag is `{key}`");
                        await dmUserChannel.SendMessageAsync("", false, GetDmRedeemEmbed(60, 2.25   , guild.CommandPrefix.ToLower()));
                    }
                    catch (Exception e)
                    {
                        logger.ConsoleCriticalAdvisory(e.Message);
                    }
                    return;
                }
            }

            foreach (string ninetyDayKey in ninetyDayKeys)
            {
                if (key.Contains(ninetyDayKey))
                {
                    userAccount.KaguyaSupporterExpiration = DateTime.Now.AddDays(90);
                    userAccount.Diamonds += 1800;
                    thirtyDayKeys.Remove(ninetyDayKey);
                    File.WriteAllLines($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/GitHub/Kaguya/90DayKeys.txt", thirtyDayKeys);

                    await GlobalCommandResponses.CreateSuccessfulRedemption(Context, ninetyDayKey,
                        "Successfully Redeemed Supporter Tag!",
                        "Thank you so much for supporting the Kaguya Project! **`90 Days`** of Kaguya Supporter time and **`1,800 Kaguya Diamonds`** have been added to your account.",
                        "The key you have just redeemed is no longer redeemable.");

                    var dmOwnerChannel = await stage.GetOrCreateDMChannelAsync();
                    var dmUserChannel = await Context.User.GetOrCreateDMChannelAsync();
                    try
                    {
                        await dmOwnerChannel.SendMessageAsync($"{Context.User} in {Context.Guild} has redeemed a supporter tag that's worth 90 days!\nKey used for this tag is `{key}`");
                        await dmUserChannel.SendMessageAsync("", false, GetDmRedeemEmbed(90, 3.10, guild.CommandPrefix.ToLower()));
                    }
                    catch (Exception e)
                    {
                        logger.ConsoleCriticalAdvisory(e.Message);
                    }
                    return;
                }
            }
        }

        public Embed GetDmRedeemEmbed(int days, double servertime, string prefix)
        {
            return new KaguyaEmbedBuilder()
                .WithTitle("Kaguya Supporter Tag Redemption")
                .WithDescription($"Thanks so much for redeeming a {days} day Supporter Tag! You help keep me running for `{servertime} days!`\n\n **Here's what to do next**:" +
                $"\n - Most of your perks are already active, but if you want the cool Supporter role in my wonderful support Discord server, follow these instructions." +
                $"\n\n - Use the  `{prefix}invite` command to get a link to my support server." +
                $"\n - Once inside, use the `$sync` command in the `#bot-commands` chat." +
                $"\n - There you go! You now have the supporter role." +
                $"\n\nWhen your supporter tag expires, you will receive a notification from me." +
                $"\n\nThanks so much for your support!!").Build();
        }
    }
}
