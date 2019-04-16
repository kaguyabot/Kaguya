using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Kaguya.Core.Server_Files
{
    public static class Servers
    {
        private static List<Server> servers;

        private static string serversFile = "Resources/servers.json";

        static Servers()
        {
            if (DataStorage2.SaveExists(serversFile))
            {
                servers = DataStorage2.LoadServers(serversFile).ToList();
            }
            else
            {
                servers = new List<Server>();
                SaveServers();
            }
        }

        public static Server GetServer(SocketGuild guild)
        {
            return GetOrCreateServer(guild.Id);
        }

        public static Server GetServer(ulong Id)
        {
            return GetOrCreateServer(Id);
        }

        public static List<Server> GetAllServers()
        {
            return servers;
        }

        private static Server GetOrCreateServer(ulong id)
        {
            var result = from a in servers
                         where a.ID == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id);
            return server;
        }

        private static Server CreateServer(ulong id)
        {
            var newServer = new Server(id);

            servers.Add(newServer);
            SaveServers();
            return newServer;
        }

        public static void SaveServers()
        {
            DataStorage2.SaveServers(servers, serversFile);
        }
    }
}
