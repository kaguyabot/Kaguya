using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

#region This file will load all Config file data into memory for the bot to use. This file contains very important credentials.
#endregion

namespace KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage
{
    public class Config
    {
        private static readonly string _resourcesPath = $"{ConfigProperties.KaguyaMainFolder}\\Resources\\";
        private const int CORRECT_ARG_COUNT = 15;

        /// <summary>
        /// Retreives a populated <see cref="ConfigModel"/> and also re-populates any necessary data from the Resources folder.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<ConfigModel> GetOrCreateConfigAsync(string[] args)
        {
            string[] directories =
            {
                _resourcesPath + "Images",
                _resourcesPath + "Logs"
            };

            string configFilePath = _resourcesPath + "config.json";

            if (!Directory.Exists(_resourcesPath))
                CreateIfNotExists(_resourcesPath);

            foreach (string dir in directories)
                CreateIfNotExists(dir); // If these directories don't exist, create them.

            // Populate resources folder, if needed.
            using (var wc = new WebClient())
            {
                // $profile image
                if (!File.Exists($@"{directories[0]}\ProfileSmall.png"))
                {
                    byte[] data = await wc.DownloadDataTaskAsync("https://i.imgur.com/Ae2BBiC.png");
                    await File.WriteAllBytesAsync($@"{directories[0]}\ProfileSmall.png", data);

                    await ConsoleLogger.LogAsync("Downloaded and saved ProfileSmall.png image.", LogLvl.INFO);
                }

                // Exp level-up image
                if (!File.Exists($@"{directories[0]}\XpLevelUpSmall.png"))
                {
                    byte[] data = await wc.DownloadDataTaskAsync("https://i.imgur.com/fgNNX8H.png");
                    await File.WriteAllBytesAsync($@"{directories[0]}\XpLevelUpSmall.png", data);

                    await ConsoleLogger.LogAsync("Downloaded and saved XpLevelUpSmall.png image.", LogLvl.INFO);
                }
            }

            if (args.Length != CORRECT_ARG_COUNT)
            {
                throw new Exception("The correct amount of arguments was not specified. " +
                                    $"Expected {CORRECT_ARG_COUNT}, received {args.Length}");
            }

            var model = new ConfigModel
            {
                Token = args[0],
                BotOwnerId = args[1].Trim().AsUlong(),
                LogLevelNumber = args[2].AsInteger(),
                DefaultPrefix = args[3],
                OsuApiKey = args[4],
                TopGgApiKey = args[5],
                MySqlUsername = args[6],
                MySqlPassword = args[7],
                MySqlServer = args[8],
                MySqlDatabase = args[9],
                TwitchClientId = args[10],
                TwitchAuthToken = args[11],
                DanbooruUsername = args[12],
                DanbooruApiKey = args[13],
                TopGgWebhookPort = args[14].AsInteger()
            };

            if (!File.Exists(configFilePath) || !model.Equals(new ConfigModel()))
            {
                //Creates JSON from model.
                var modelToSave = JsonConvert.DeserializeObject<ConfigModel>(await CreateConfigAsync(configFilePath, model));
                await File.WriteAllTextAsync(configFilePath, JsonConvert.SerializeObject(modelToSave, Formatting.Indented));

                await ConsoleLogger.LogAsync($"Wrote new config file.", LogLvl.INFO);
            }
            else if (args.Length != 14)
            {
                //Reads config file.
                model = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(configFilePath));

                return model ?? JsonConvert.DeserializeObject<ConfigModel>(await CreateConfigAsync(configFilePath));
            }

            return model;
        }

        private static void CreateIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<string> CreateConfigAsync(string filepath, ConfigModel model = null)
        {
            if (model == null)
                model = new ConfigModel();

            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (StreamWriter writer = File.CreateText(filepath))
                await writer.WriteAsync(json);

            return json;
        }
    }

    /// <summary>
    /// LogLevels arranged in order of importance (least to greatest importance): Trace, Debug, Info, Warn, Error.
    /// Info should be used for commands and other general information.
    /// </summary>
    public enum LogLvl
    {
        TRACE = 0,
        DEBUG = 1,
        INFO = 2,
        WARN = 3,
        ERROR = 4
    }

#region Model
    public class ConfigModel
    {
        public string Token { get; set; }
        public ulong BotOwnerId { get; set; }
        public int LogLevelNumber { get; set; } = 1;
        public string DefaultPrefix { get; set; } = "$";
        public string OsuApiKey { get; set; }
        public string TopGgApiKey { get; set; }
        public string MySqlUsername { get; set; }
        public string MySqlPassword { get; set; }
        public string MySqlServer { get; set; }
        public string MySqlDatabase { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchAuthToken { get; set; }
        public string DanbooruUsername { get; set; }
        public string DanbooruApiKey { get; set; }
        public int TopGgWebhookPort { get; set; }
    }
#endregion

}