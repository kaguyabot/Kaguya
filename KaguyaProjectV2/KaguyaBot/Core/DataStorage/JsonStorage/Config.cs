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
        string Token { get; set; }
        string DefaultPrefix { get; set; }
        string OsuApiKey { get; set; }
        string TillerinoApiKey { get; set; }
        string TopGGApiKey { get; set; }
        string TopGGAuthorizationPassword { get; set; }

        public Config()
        {
            Token = "";
            DefaultPrefix = "$";
            OsuApiKey = "";
            TillerinoApiKey = "";
            TopGGApiKey = "";
            TopGGAuthorizationPassword = "";
        }

        public static void JsonDeserialization()
        {
            string filePath = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\"))}\\Release\\Resources\\config.json";
            var json = JsonConvert.SerializeObject(filePath);


        }
    }
}
