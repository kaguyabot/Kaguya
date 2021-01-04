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
        public const string StoreNameWithLink = "[Kaguya Premium](" + StoreUrl + ")";
        public const string SupportDiscordUrl = "https://discord.gg/aumCJhr";
        public const string InviteUrl = "https://discord.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=536341759";
        public static readonly string Version = "v4.0-beta-" + Process.GetCurrentProcess().StartTime.ToShortDateString().Replace('/', '.');

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