using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class GlobalProperties
    {
        public static LogLevel logLevel { get; set; }
        public static DiscordShardedClient client { get; set; }
    }
}
