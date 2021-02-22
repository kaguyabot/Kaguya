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
        public const string VideoTutorialUrl = "https://youtu.be/ZXUbPwNYex0";
        /// <summary>
        /// Formatted as such: [Kaguya Premium](StoreUrl)
        /// </summary>
        public const string StoreLink = "[Kaguya Premium](" + StoreUrl + ")";
        public const string SupportDiscordUrl = "https://discord.gg/aumCJhr";
        public const string InviteUrl = "https://discord.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=536341759";
        public const string LocalDebugInviteUrl = "https://discord.com/api/oauth2/authorize?client_id=664032361679159309&permissions=8&scope=bot";
        public const string TopGgUpvoteUrl = "https://top.gg/bot/538910393918160916";
       
        public const string WikiPremiumBenefitsUrl = "https://github.com/kaguyabot/Kaguya/wiki/Premium";
        public const string WikiPrivacyUrl = "https://github.com/kaguyabot/Kaguya/wiki/Privacy";
        public const string WikiQuickStartUrl = "https://github.com/kaguyabot/Kaguya/wiki/Using-Kaguya:-Quick-Start-Guide";
        public const string WikiV4WhatIsNew = "https://github.com/kaguyabot/Kaguya/wiki/What's-new-in-v4%3F";
        public const string WikiChangelog = "https://github.com/kaguyabot/Kaguya/wiki/Changelog";
        
        public const string DiscordTermsLink = "[Terms of Service](https://discord.com/terms)";
        public const string DiscordCommunityGuidelinesLink = "[Community Guidelines](https://discord.com/guidelines)";

        public static readonly string Version = "4.3.1";

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
            return (DateTimeOffset.Now - Process.GetCurrentProcess().StartTime).Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
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