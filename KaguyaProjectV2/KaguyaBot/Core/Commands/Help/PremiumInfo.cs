using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class PremiumInfo : KaguyaBase
    {
        [HelpCommand]
        [Command("Premium")]
        [Summary("Displays information on the [Kaguya Premium](" + ConfigProperties.KaguyaStore + "subscription.")]
        [Remarks("")]
        public async Task Command()
        {
            // Pasted from the store.
            string premString = "All perks below will last until your time as a premium subscriber runs out.\n**(SW)** = Works across all servers you own!!\n\n****Unlimited access to features that require a vote to use.***\n* Bet many more points than usual on gambling games.\n* 25,000 points for every 30 days of premium time purchased.\n* 2x points and exp from $daily\n* 2x points and exp from $vote\n* 25% off $bait cost\n* Special $profile badge\n* Significantly reduced $fish cooldown\n* More lenient rate limit (able to use commands more frequently than other users).\n* Access to $doujin\n* Access to $react\n* Access to $weekly\n* Access to $serverstats\n* Access to $hyperban\n* Access to $deleteunusedroles\n* Access to $soundcloud, $twitchaudio\n* Bonus luck on all gambling commands (including $fish)\n* Store up to 1,000 fish bait instead of 100\n* Deleted messages logged via \"$log DeletedMessages \" will now include archives of deleted images and attachments. **(SW)**\n* Unlimited role rewards **(SW)**\n* Access to the $logtype \"ModLog\" - logs many various administrative actions **(SW)**\n* View more of a user's warn history via $unwarn **(SW)**\n* Unlimited song duration (compared to 10 minutes) **(SW)**\n* Unlimited music queue size **(SW)**\n* Unlimited + enhanced $nsfw usage **(SW)**\n\n__**Purchase Kaguya Premium at the [Kaguya Store](https://sellix.io/KaguyaStore) for only $4.99 a month!**__";
            var embed = new KaguyaEmbedBuilder
            {
                Title = "Kaguya Premium",
                Description = premString
            };

            await SendEmbedAsync(embed);
        }
    }
}    