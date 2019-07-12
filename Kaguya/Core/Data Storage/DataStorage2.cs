using System.Collections.Generic;
using Kaguya.Core.UserAccounts;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using Newtonsoft.Json;
using System.IO;
using System;

namespace Kaguya.Core
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

        public static void SaveCommands(IEnumerable<Command> commands, string filePath)
        {
            string json = JsonConvert.SerializeObject(commands, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void SaveServerLogging(IEnumerable<ServerMessageLog> logs, string filePath)
        {
            string json = JsonConvert.SerializeObject(logs, Formatting.Indented);
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

        public static IEnumerable<Command> LoadCommands(string filepath) //Gets commands
        {
            string json;

            if (!File.Exists(filepath))
            {
                Command command = new Command();
                json = JsonConvert.SerializeObject(command, Formatting.Indented);
                File.WriteAllText(filepath, json);
                return JsonConvert.DeserializeObject<List<Command>>(json);
            }
            else
            {
                json = File.ReadAllText(filepath);
                return JsonConvert.DeserializeObject<List<Command>>(json);
            }
        }

        private static List<ServerMessageLog> serverMessageLogs;

        public static IEnumerable<ServerMessageLog> LoadServerMessageLogs(string filepath)
        {
            try
            {
                if (!File.Exists(filepath)) return null;
                if (File.ReadAllText(filepath) == "")
                    NewLog(filepath);
                string json = File.ReadAllText(filepath);
                return JsonConvert.DeserializeObject<List<ServerMessageLog>>(json);
            }
            catch (Exception e)
            {
                return NewLog(filepath);
            }
        }

        private static IEnumerable<ServerMessageLog> NewLog(string filepath)
        {
            Logger logger = new Logger();
            logger.ConsoleCriticalAdvisory("ServerMessageLogs was broken so I fixed it.");
            File.Delete(filepath);
            File.WriteAllText(filepath, "[]");
            string json = File.ReadAllText(filepath);
            logger.ConsoleStatusAdvisory("Successfully wrote new ServerMessageLogs file.");
            return JsonConvert.DeserializeObject<List<ServerMessageLog>>(json);
        }

        public static bool SaveExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
