using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Constants;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using Newtonsoft.Json;
using TwitchLib.Api.ThirdParty.ModLookup;

#region This file will load all Config file data into memory for the bot to use. This file contains very important credentials.
#endregion

namespace KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage
{
    public class Config
    {
        private const int CORRECT_ARG_COUNT = 15;
        private static readonly string _resourcesPath = Path.Combine(FileConstants.RootDir, "Resources");
        /// <summary>
        ///     Retreives a populated <see cref="BotConfig" /> and also re-populates any necessary data from the Resources folder.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<IBotConfig> GetOrCreateConfigAsync(string[] args)
        {
            string[] directories =
            {
                _resourcesPath + "Images",
                _resourcesPath + "Logs"
            };

            string configFilePath = Path.Combine(_resourcesPath, "config.json");
            string profileSmallPath = Path.Combine(directories[0], "ProfileSmall.png");
            string xpLevelSmallPath = Path.Combine(directories[1], "XpLevelUpSmall.png");
            
            if (!Directory.Exists(_resourcesPath))
                CreateIfNotExists(_resourcesPath);

            foreach (string dir in directories)
                CreateIfNotExists(dir); // If these directories don't exist, create them.

            // Populate resources folder, if needed.
            using (var wc = new WebClient())
            {
                // $profile image
                if (!File.Exists(profileSmallPath))
                {
                    byte[] data = await wc.DownloadDataTaskAsync("https://i.imgur.com/Ae2BBiC.png");
                    await File.WriteAllBytesAsync(profileSmallPath, data);

                    await ConsoleLogger.LogAsync($@"Missing file ProfileSmall.png downladed and saved to {profileSmallPath}.", LogLvl.INFO);
                }

                // Exp level-up image
                if (!File.Exists(xpLevelSmallPath))
                {
                    byte[] data = await wc.DownloadDataTaskAsync("https://i.imgur.com/fgNNX8H.png");
                    await File.WriteAllBytesAsync(xpLevelSmallPath, data);

                    await ConsoleLogger.LogAsync($@"Missing file XpLevelUpSmall.png downladed and saved to {xpLevelSmallPath}.", LogLvl.INFO);
                }
            }

            IBotConfig model = new BotConfig();
            if (!File.Exists(configFilePath) && args.Length != CORRECT_ARG_COUNT)
            {
                string text = JsonConvert.SerializeObject(model, Formatting.Indented);
                await File.WriteAllTextAsync(configFilePath, text);
                await ConsoleLogger.LogAsync($"Attention: A new configuration file has been created at " +
                                             $"{configFilePath}. Please visit this location and configure the file " +
                                             $"according to the instructions on github: https://github.com/stageosu/Kaguya/blob/master/README.md", LogLvl.WARN);

                return model;
            }
            
            if (File.Exists(configFilePath) && args.Length != CORRECT_ARG_COUNT)
            {
                try
                {
                    model = JsonConvert.DeserializeObject<BotConfig>(configFilePath);

                    return model;
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync($"Your config file is improperly configured at " +
                                                 $"{configFilePath}. Please visit this location and configure the file " +
                                                 $"according to the instructions on github: https://github.com/stageosu/Kaguya/blob/master/README.md", LogLvl.WARN);
                }
            }

            model = BotConfig.GetConfig();
            model.Token = args[0];
            model.Token = args[0];
            model.BotOwnerId = args[1].Trim().AsUlong();
            model.LogLevelNumber = args[2].AsInteger();
            model.DefaultPrefix = args[3];
            model.OsuApiKey = args[4];
            model.TopGgApiKey = args[5];
            model.MySqlUsername = args[6];
            model.MySqlPassword = args[7];
            model.MySqlServer = args[8];
            model.MySqlSchema = args[9];
            model.TwitchClientId = args[10];
            model.TwitchAuthToken = args[11];
            model.DanbooruUsername = args[12];
            model.DanbooruApiKey = args[13];
            model.TopGgWebhookPort = args[14].AsInteger();
            
            //Creates JSON from model.
            var modelToSave = JsonConvert.DeserializeObject<BotConfig>(await CreateConfigAsync(configFilePath, model));
            await File.WriteAllTextAsync(configFilePath, JsonConvert.SerializeObject(modelToSave, Formatting.Indented));

            await ConsoleLogger.LogAsync($"Config file at {configFilePath} has been populated with arguments provided.", LogLvl.INFO);
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

        private static async Task<string> CreateConfigAsync(string filepath, IBotConfig model = null)
        {
            model ??= new BotConfig();

            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            await File.WriteAllTextAsync(filepath, json);
            return json;
        }
    }

    /// <summary>
    ///     LogLevels arranged in order of importance (least to greatest importance): Trace, Debug, Info, Warn, Error.
    ///     Info should be used for commands and other general information.
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
    public sealed class BotConfig : IBotConfig
    {
        private static IBotConfig _instance;
        public static IBotConfig GetConfig()
        {
            if (_instance == null)
            {
                _instance = new BotConfig();
            }
            return _instance;
        }

        public string Token { get; set; }
        public ulong BotOwnerId { get; set; }
        public int LogLevelNumber { get; set; }
        public string DefaultPrefix { get; set; }
        public string OsuApiKey { get; set; }
        public string TopGgApiKey { get; set; }
        public string MySqlUsername { get; set; }
        public string MySqlPassword { get; set; }
        public string MySqlServer { get; set; }
        public string MySqlSchema { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchAuthToken { get; set; }
        public string DanbooruUsername { get; set; }
        public string DanbooruApiKey { get; set; }
        public int TopGgWebhookPort { get; set; }
    }
#endregion

}