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
        public static AuthDiscordBotListApi TopGgApi { get; set; }
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

        public const string KAGUYA_SUPPORT_DISCORD_URL = "https://discord.gg/aumCJhr";
        public const string KAGUYA_STORE_URL = "https://sellix.io/KaguyaStore";
        public const string KAGUYA_DEV_INVITE_URL = "https://discord.com/api/oauth2/authorize?client_id=664032361679159309&permissions=8&scope=bot";

        public const string KAGUYA_INVITE_URL =
            "https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=469101694";

        // Github contributors, please do not edit the Version number.
        public static string Version { get; } = "2.16";
    }
}