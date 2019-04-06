using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.WebSocket;

namespace Discord_Bot
{
    internal static class Global
    {
        internal static DiscordSocketClient Client { get; set; }

        internal static InteractiveService Interactive { get; set; }
    }
}
