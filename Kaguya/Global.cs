using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Victoria;

namespace Kaguya
{
    internal static class Global
    {
        internal static DiscordShardedClient Client { get; set; }

        public static LavaShardClient lavaShardClient = new LavaShardClient();
        public static LavaRestClient lavaRestClient = new LavaRestClient();
    }
}
