using System;
using System.Collections.Generic;
using System.Diagnostics;
using Humanizer;
using Humanizer.Localisation;

namespace Kaguya
{
    public static class Global
    {
        public const string WebsiteUrl = "http://kaguyabot.xyz/";
        public const string StoreUrl = "https://sellix.io/KaguyaStore";
        /// <summary>
        /// Formatted as such: [Kaguya Premium](StoreUrl)
        /// </summary>
        public const string StoreLink = "[Kaguya Premium](" + StoreUrl + ")";
        public const string SupportDiscordUrl = "https://discord.gg/aumCJhr";
        public const string InviteUrl = "https://discord.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=536341759";
        public const string BetaInviteUrl = "https://discord.com/oauth2/authorize?client_id=367403886841036820&scope=bot&permissions=1610083583";
        public const string TopGgUpvoteUrl = "https://top.gg/bot/538910393918160916";
        public const string WikiPrivacyUrl = "https://github.com/kaguyabot/Kaguya/wiki/Privacy";
        public const string WikiQuickStartUrl = "https://github.com/kaguyabot/Kaguya/wiki/Using-Kaguya:-Quick-Start-Guide";
        public const string DiscordTermsLink = "[Terms of Service](https://discord.com/terms)";
        public const string DiscordCommunityGuidelinesLink = "[Community Guidelines](https://discord.com/guidelines)";
        
        public static readonly string Version = "v4.0-beta-" + GetStartDate();

        /// <summary>
        /// The number of shards currently logged into Discord.
        /// </summary>
        public static List<int> ShardsReady { get; } = new List<int>();
        
        /// <summary>
        /// Gets the current uptime for the program in a user-friendly readable format.
        /// </summary>
        /// <returns></returns>
        public static string GetUptimeString()
        {
            return (DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
        }

        public static string GetStartDate() { return Process.GetCurrentProcess().StartTime.ToLocalTime().ToShortDateString().Replace('/', '.'); }

        /// <summary>
        /// Increases the number of logged in shards by 1.
        /// </summary>
        public static void AddReadyShard(int shardId)
        {
            if (!ShardsReady.Contains(shardId))
            {
                ShardsReady.Add(shardId);
            }
        }
    }
}