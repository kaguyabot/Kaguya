using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Core.Server_Files
{
    public static class ServerMessageLogs
    {
        private static List<ServerMessageLog> serverMessageLogs;

        private static string logFile = "Resources/servermessagelogs.json";

        static ServerMessageLogs()
        {
            if(DataStorage2.SaveExists(logFile))
            {
                serverMessageLogs = DataStorage2.LoadServerMessageLogs(logFile).ToList();
            }
            else
            {
                serverMessageLogs = new List<ServerMessageLog>();
                SaveServerLogging();
            }
        }

        public static ServerMessageLog GetLog(SocketGuild guild)
        {
            return GetOrCreateLog(guild.Id);
        }

        private static ServerMessageLog GetOrCreateLog(ulong id)
        {
            var result = from a in serverMessageLogs
                         where a.ID == id
                         select a;

            var serverMessageLog = result.FirstOrDefault();
            if (serverMessageLog == null) serverMessageLog = CreateLog(id);
            return serverMessageLog;
        }

        private static ServerMessageLog CreateLog(ulong id)
        {
            var newLog = new ServerMessageLog(id);

            serverMessageLogs.Add(newLog);
            SaveServerLogging();
            return newLog;
        }

        public static void SaveServerLogging()
        {
            DataStorage2.SaveServerLogging(serverMessageLogs, logFile);
        }
    }
}
