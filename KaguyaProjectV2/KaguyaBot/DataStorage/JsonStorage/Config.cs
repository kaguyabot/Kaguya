using System;
using System.IO;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using Newtonsoft.Json;

#region This file will load all Config file data into memory for the bot to use. This file contains very important credentials.
#endregion

namespace KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage
{
    public class Config
    {
        private static readonly string resourcesPath = $"{ConfigProperties.KaguyaMainFolder}\\Resources\\";

        public static async Task<ConfigModel> GetOrCreateConfigAsync(string[] args)
        {
            string[] directories =
            {
                resourcesPath + "Logs",
                resourcesPath + "Images",
                resourcesPath + "Images\\Hentai",
                resourcesPath + "Logs",
                resourcesPath + "Logs\\Debug"
            };

            string configFilePath = resourcesPath + "config.json";

            if (!Directory.Exists(resourcesPath))
            {
                EnsurePathExists(resourcesPath);
            }

            foreach (var dir in directories)
            {
                EnsurePathExists(dir); // If these directories don't exist, create them.
            }

            if (args.Length != 0 && args.Length == 15)
            {
                var model = new ConfigModel
                {
                    Token = args[0],
                    BotOwnerId = args[1].AsUlong(),
                    LogLevelNumber = args[2].AsInteger(),
                    DefaultPrefix = args[3],
                    OsuApiKey = args[4],
                    TopGGApiKey = args[5],
                    TopGGAuthorizationPassword = args[6],
                    MySQL_Username = args[7],
                    MySQL_Password = args[8],
                    MySQL_Server = args[9],
                    MySQL_Database = args[10],
                    TwitchClientId = args[11],
                    TwitchAuthToken = args[12],
                    DanbooruUsername = args[13],
                    DanbooruApiKey = args[14]
                };

                return model;
            }

            if (!File.Exists(configFilePath))
            {
                //Creates JSON from model.
                return JsonConvert.DeserializeObject<ConfigModel>(await CreateConfigAsync(configFilePath));
            }
            else
            {
                //Reads config file.
                var model = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(configFilePath));
                if (model == null)
                {
                    return JsonConvert.DeserializeObject<ConfigModel>(await CreateConfigAsync(configFilePath));
                }
                else
                {
                    return model;
                }
            }
        }

        private static void EnsurePathExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<string> CreateConfigAsync(string filepath, ConfigModel model = null)
        {
            if (model == null)
            {
                model = new ConfigModel();
            }

            var json = JsonConvert.SerializeObject(model, Formatting.Indented);
            using (var writer = File.CreateText(filepath))
            {
                await writer.WriteAsync(json);
            }

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
        ERROR = 4,
    }

    #region Model
    public class ConfigModel
    {
        public string Token { get; set; }
        public ulong BotOwnerId { get; set; }
        public int LogLevelNumber { get; set; } = 1;
        public string DefaultPrefix { get; set; } = "$";
        public string OsuApiKey { get; set; }
        public string TopGGApiKey { get; set; }
        public string TopGGAuthorizationPassword { get; set; }
        public string MySQL_Username { get; set; }
        public string MySQL_Password { get; set; }
        public string MySQL_Server { get; set; }
        public string MySQL_Database { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchAuthToken { get; set; }
        public string DanbooruUsername { get; set; }
        public string DanbooruApiKey { get; set; }
    }
    #endregion
}