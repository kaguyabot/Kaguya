using Discord.WebSocket;
using DiscordBotsList.Api;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using NekosSharp;
using System.IO;
using TwitchLib.Api;
using Victoria;

#pragma warning disable

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class ConfigProperties
    {
        public static DiscordShardedClient Client { get; set; }
        public static LavaNode LavaNode { get; set; }
        public static AuthDiscordBotListApi TopGGApi { get; set; }
        public static TwitchAPI TwitchApi { get; set; }
        public static NekoClient NekoClient = new NekoClient("Kaguya");
        public static ConfigModel BotConfig { get; set; }
        public static LogLvl LogLevel { get; set; }

        /// <summary>
        /// Returns the directory of the "KaguyaProjectV2" folder. By using @"KaguyaMainFolder\Resources", you
        /// may gain access to the resources folder. This goes for any other folder in the directory. This
        /// string may also be navigated up any folders using @".." if needed.
        /// </summary>
        public static string KaguyaMainFolder { get; } = Directory.GetCurrentDirectory();
        public static string KaguyaSupportDiscordServer { get; } = "https://discord.gg/aumCJhr";
        public static string Version { get; } = "2.5.1";
        public static string KaguyaStore = "https://the-kaguya-project.myshopify.com/";
    }
}