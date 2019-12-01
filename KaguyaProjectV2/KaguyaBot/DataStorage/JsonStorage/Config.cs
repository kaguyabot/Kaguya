using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

#region This file will load all Config file data into memory for the bot to use. This file contains very important credentials.
#endregion

namespace KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage
{
    public class Config
    {
        private static readonly string path = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Resources";

        public static async Task<ConfigModel> GetOrCreateConfigAsync()
        {
            //Navigates a few directories up
            string filePath = path + "\\config.json";

            if (!Directory.Exists(path))
            {
                EnsurePathExists(path);
            }

            if (!File.Exists(filePath))
            {
                //Creates JSON from model.
                return JsonConvert.DeserializeObject<ConfigModel>(await CreateConfigAsync(filePath));
            }
            else
            {
                //Reads config file.
                var model = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(filePath));
                if (model == null)
                {
                    return JsonConvert.DeserializeObject<ConfigModel>(await CreateConfigAsync(filePath));
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
    public enum LogLevel
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
    }
    #endregion
}