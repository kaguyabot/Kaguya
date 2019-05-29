﻿using System.Collections.Generic;
using System.Linq;
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
            return GetOrCreateServer(guild.Id, guild.Name);
        }

        public static Server GetServer(ulong Id, string serverName)
        {
            return GetOrCreateServer(Id, serverName);
        }

        public static Server RemoveServer(ulong ID, string serverName)
        {
            var server = Servers.GetServer(ID, serverName);
            servers.Remove(server);
            return server;
        }

        public static List<Server> GetAllServers()
        {
            return servers;
        }

        private static Server GetOrCreateServer(ulong id, string serverName)
        {
            var result = from a in servers
                         where a.ID == id
                         select a;

            var server = result.FirstOrDefault();
            if (server == null) server = CreateServer(id, serverName);
            return server;
        }

        private static Server CreateServer(ulong id, string serverName)
        {
            var newServer = new Server(id, serverName);

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
