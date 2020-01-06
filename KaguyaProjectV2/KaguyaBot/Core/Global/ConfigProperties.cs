using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using TwitchLib.Api;

#pragma warning disable

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class ConfigProperties
    {
        public static DiscordShardedClient Client { get; set; }
        public static TwitchAPI TwitchApi { get; set; }
        public static ConfigModel BotConfig { get; set; }
        public static LogLvl LogLevel { get; set; }

        /// <summary>
        /// Returns the directory of the "KaguyaProjectV2" folder. By using @"KaguyaMainFolder\Resources", you
        /// may gain access to the resources folder. This goes for any other folder in the directory. This
        /// string may also be navigated up any folders using @".." if needed.
        /// </summary>
        public static string KaguyaMainFolder { get; } = Directory.GetCurrentDirectory();
        public static string KaguyaSupportDiscordServer { get; } = "https://discord.gg/aumCJhr";
        public static string Version { get; } = "2.0.A2020-1-6";
    }
}
