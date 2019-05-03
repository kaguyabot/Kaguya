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

        public static List<ServerMessageLog> GetAllLogs()
        {
            return serverMessageLogs;
        }

        public static ServerMessageLog GetLog(SocketGuild guild)
        {
            return GetOrCreateLog(guild.Id);
        }

        public static ServerMessageLog GetLog(ulong ID)
        {
            return GetOrCreateLog(ID);
        }

        public static ServerMessageLog DeleteLog(SocketGuild guild)
        {
            return DeleteLog(guild.Id);
        }

        public static ServerMessageLog RemoveLog(ulong ID)
        {
            var server = ServerMessageLogs.GetLog(ID);
            serverMessageLogs.Remove(server);
            return server;
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

        private static ServerMessageLog DeleteLog(ulong id)
        {
            var log = GetLog(id);
            serverMessageLogs.Remove(log);
            return log;
        }

        public static void SaveServerLogging()
        {
            DataStorage2.SaveServerLogging(serverMessageLogs, logFile);
        }
    }
}
