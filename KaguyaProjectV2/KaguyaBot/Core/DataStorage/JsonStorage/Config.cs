using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

#region This file will load all Config file data into memory for the bot to use. This file contains very important credentials.
#endregion

namespace KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage
{
    public class Config
    {
        public static async Task<ConfigModel> GetOrCreateConfigAsync()
        {
            //Navigates a few directories up
            string filePath = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Core_Data\\Resources\\config.json";
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

        public static async Task<string> CreateConfigAsync(string filepath, ConfigModel model = null)
        {
            if (model == null)
            {
                model = new ConfigModel();
            }

            var json = JsonConvert.SerializeObject(model);
            using (var writer = File.CreateText(filepath))
            {
                await writer.WriteAsync(json);
            }

            return json;
        }
    }

    #region Model
    public class ConfigModel
    {
        public string Token { get; set; }
        public string DefaultPrefix { get; set; } = "$";
        public string OsuApiKey { get; set; }
        public string TillerinoApiKey { get; set; }
        public string TopGGApiKey { get; set; }
        public string TopGGAuthorizationPassword { get; set; }
    }
    #endregion
}