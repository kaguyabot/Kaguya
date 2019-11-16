using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class GlobalProperties
    {
        public static DiscordShardedClient client { get; set; }
        public static string osuApiKey { get; set; }
        public static string topGGApiKey { get; set; }
        public static string topGGAuthorizationPassword { get; set; }
        public static string mySQL_Username { get; set; }
        public static string mySQL_Password { get; set; }
        public static string mySQL_Server { get; set; }
        public static string mySQL_Database { get; set; }
        public static LogLevel logLevel { get; set; }
    }
}
