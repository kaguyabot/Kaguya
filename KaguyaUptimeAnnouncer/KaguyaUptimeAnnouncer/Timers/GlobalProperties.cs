using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace KaguyaUptimeAnnouncer.Timers
{
    public static class GlobalProperties
    {
        public static DiscordSocketClient client { get; set; }
    }
}
