using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

#region This file will load all Config file data into memory for the bot to use. This file contains very important credentials.
#endregion

namespace KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage
{
    public class Config
    {
        public string Token { get; set; }
        public string DefaultPrefix { get; set; }
        public string OsuApiKey { get; set; }
        public string TillerinoApiKey { get; set; }
        public string TopGGApiKey { get; set; }
        public string TopGGAuthorizationPassword { get; set; }

        public Config()
        {
            Token = "";
            DefaultPrefix = "$";
            OsuApiKey = "";
            TillerinoApiKey = "";
            TopGGApiKey = "";
            TopGGAuthorizationPassword = "";
        }

        public static Config GetOrCreateConfig()
        {
            string json;

            //Navigates a few directories up
            string filePath = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Core_Data\\Resources\\config.json";
            if (!File.Exists(filePath))
            {
                Config config = new Config();


                json = JsonConvert.SerializeObject(config);
                File.Create(filePath);
                File.WriteAllText(filePath, json);
                return JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }
}