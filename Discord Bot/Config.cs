using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kaguya
{
    public class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static BotConfig bot;

        static Config()
        {
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))
            {
                bot = new BotConfig();
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
    }

    public struct BotConfig
    {
        public string Token { get; set; }
        public string CmdPrefix { get; set; }
        public string OsuApiKey { get; set; }
        public string TillerinoApiKey { get; set; }
        public string PatreonAccessToken { get; set; }
        public string PatreonClientId { get; set; }
        public string BotUserID { get; set; }
        public string DblApiKey { get; set; }
        public DateTime LastSeenMessage { get; set; } 
    }
}
