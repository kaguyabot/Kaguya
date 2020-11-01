using System;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;

namespace KaguyaProjectV2.KaguyaApi
{
    public sealed class KaguyaApiConfig : IDbConfig
    {
        public KaguyaApiConfig(IBotConfig botConfig, KaguyaApiCredentials credentials)
        {
            string[] dbSplits = botConfig.MySqlServer.Split(':');
            Console.WriteLine(botConfig.MySqlServer);
            Console.WriteLine(dbSplits.Humanize(""));
            ServerIp = dbSplits[0];
            Console.WriteLine(ServerIp);
            Port = ushort.Parse(dbSplits[1]);
            Console.WriteLine(Port);
            SchemaName = botConfig.MySqlSchema;
            Username = botConfig.MySqlUsername;
            Password = botConfig.MySqlPassword;
            Credentials = credentials;
            CharSet = "utf8mb4";
        }

        public KaguyaApiConfig() { Credentials = new KaguyaApiCredentials(); }
        public KaguyaApiCredentials Credentials { get; }
        public string ServerIp { get; }
        public ushort Port { get; }
        public string SchemaName { get; }
        public string Username { get; }
        public string Password { get; }
        public string CharSet { get; }
        public void CreateAppsettingsFile() { }
    }
}