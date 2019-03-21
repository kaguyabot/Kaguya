using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot.Core.UserAccounts;
using Discord_Bot.Core.Server_Files;
using Newtonsoft.Json;
using System.IO;

namespace Discord_Bot.Core
{
    public static class DataStorage2
    {
        public static void SaveUserAccounts(IEnumerable<UserAccount> accounts, string filePath) //Saves user accounts. Required after an edit (adding points for example)
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void SaveServers(IEnumerable<Server> servers, string filePath) //Same thing as above for servers
        {
            string json = JsonConvert.SerializeObject(servers, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        //Get all userAccounts
        public static IEnumerable<UserAccount> LoadUserAccounts(string filePath) // Add exception handling
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<UserAccount>>(json);
        }

        public static IEnumerable<Server> LoadServers(string filepath) //Gets servers
        {
            if (!File.Exists(filepath)) return null;
            string json = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<List<Server>>(json);
        }

        public static bool SaveExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
