using Discord.WebSocket;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System.Collections.Generic;
using Victoria;

namespace Kaguya
{
    internal static class Global
    {
        internal static DiscordShardedClient Client { get; set; }
        public static LavaShardClient lavaShardClient = new LavaShardClient();
        public static LavaRestClient lavaRestClient = new LavaRestClient();
        public static List<UserAccount> UserAccounts { get; set; }
        public static List<Server> Servers { get; set; }
        public static int TotalGuildCount { get; set; }
        public static int TotalMemberCount { get; set; }
        public static int ShardsToLogIn { get; set; }
        public static int ShardsLoggedIn { get; set; }
    }
}
