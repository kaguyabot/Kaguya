using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Kaguya
{
    internal static class Global
    {
        internal static DiscordSocketClient Client { get; set; }
    }
}
