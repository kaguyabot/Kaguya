using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using System.Net;
using Kaguya.Core.Commands;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.UserAccounts;

namespace Kaguya.Modules.Supporter
{
    public class KaguyaSupporter : ModuleBase<SocketCommandContext>
    {
        [Command("redeem")]
        public async Task KeyRedeem([Remainder]string key)
        {
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);

            List<string> thirtyDayKeys = File.ReadAllLines("C:/Users/stage/Documents/GitHub/Kaguya/30DayKeys.txt").ToList();
            List<string> sixtyDayKeys = File.ReadAllLines("C:/Users/stage/Documents/GitHub/Kaguya/60DayKeys.txt").ToList();
            List<string> ninetyDayKeys = File.ReadAllLines("C:/Users/stage/Documents/GitHub/Kaguya/90DayKeys.txt").ToList();

            foreach (string thirtyDayKey in thirtyDayKeys)
            {
                if (key.Contains(thirtyDayKey))
                {
                    userAccount.KaguyaSupporterExpiration = DateTime.Now.AddDays(30);
                    userAccount.KaguyaDiamonds += 600;
                    UserAccounts.SaveAccounts();
                    thirtyDayKeys.Remove(thirtyDayKey);
                    File.WriteAllLines("C:/Users/stage/Documents/GitHub/Kaguya/30DayKeys.txt", thirtyDayKeys);

                    await GlobalCommandResponses.CreateSuccessfulRedemption(Context, thirtyDayKey,
                        "Successfully Redeemed Supporter Tag!",
                        "Thank you so much for supporting the Kaguya Project! **`30 Days`** of Kaguya Supporter time have been added to your account.",
                        "The key you have just redeemed is no longer redeemable.");
                }
            }

            foreach (string sixtyDayKey in sixtyDayKeys)
            {
                if (key.Contains(sixtyDayKey))
                {
                    userAccount.KaguyaSupporterExpiration = DateTime.Now.AddDays(60);
                    userAccount.KaguyaDiamonds += 1200;
                    UserAccounts.SaveAccounts();
                    thirtyDayKeys.Remove(sixtyDayKey);
                    File.WriteAllLines("C:/Users/stage/Documents/GitHub/Kaguya/60DayKeys.txt", thirtyDayKeys);

                    await GlobalCommandResponses.CreateSuccessfulRedemption(Context, sixtyDayKey,
                        "Successfully Redeemed Supporter Tag!",
                        "Thank you so much for supporting the Kaguya Project! **`60 Days`** of Kaguya Supporter time and **`1,200 Kaguya Diamonds`**have been added to your account.",
                        "The key you have just redeemed is no longer redeemable.");
                }
            }

            foreach (string ninetyDayKey in ninetyDayKeys)
            {
                if (key.Contains(ninetyDayKey))
                {
                    userAccount.KaguyaSupporterExpiration = DateTime.Now.AddDays(90);
                    userAccount.KaguyaDiamonds += 1800;
                    UserAccounts.SaveAccounts();
                    thirtyDayKeys.Remove(ninetyDayKey);
                    File.WriteAllLines("C:/Users/stage/Documents/GitHub/Kaguya/90DayKeys.txt", thirtyDayKeys);

                    await GlobalCommandResponses.CreateSuccessfulRedemption(Context, ninetyDayKey,
                        "Successfully Redeemed Supporter Tag!",
                        "Thank you so much for supporting the Kaguya Project! **`90 Days`** of Kaguya Supporter time and **`1,800 Kaguya Diamonds`** have been added to your account.",
                        "The key you have just redeemed is no longer redeemable.");
                }
            }
        }
    }
}
