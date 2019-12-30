using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using TwitchLib.Api;
using TwitchLib.Client;

#pragma warning disable

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class ConfigProperties
    {
        public static DiscordShardedClient client { get; set; }
        public static TwitchAPI twitchApi { get; set; }
        public static ConfigModel botConfig { get; set; }
        public static LogLevel logLevel { get; set; }
    }
}
