using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class GlobalProperties
    {
        public static DiscordShardedClient Client { get; set; }
        public static string OsuApiKey { get; set; }
        public static string TopGGApiKey { get; set; }
        public static string TopGGAuthorizationPassword { get; set; }
        public static string MySQL_Username { get; set; }
        public static string MySQL_Password { get; set; }
        public static string MySQL_Server { get; set; }
        public static string MySQL_Database { get; set; }
        public static LogLevel logLevel { get; set; }
    }
}
