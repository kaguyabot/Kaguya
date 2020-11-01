using KaguyaProjectV2.KaguyaApi.Database;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;

namespace KaguyaProjectV2.KaguyaApi
{
    public sealed class KaguyaApiConfig : IDbConfig
    {
        public KaguyaApiConfig(IBotConfig botConfig, KaguyaApiCredentials credentials)
        {
            string[] dbSplits = botConfig.MySqlServer.Split(':');
            ServerIp = dbSplits[0];
            Port = ushort.Parse(dbSplits[1]);
            SchemaName = botConfig.MySqlDatabase;
            Username = botConfig.MySqlUsername;
            Password = botConfig.MySqlPassword;
            Credentials = credentials;
            CharSet = "utf8mb4";
        }

        public KaguyaApiConfig() { Credentials = new KaguyaApiCredentials(); }

        public string ServerIp { get; }
        public ushort Port { get; }
        public string SchemaName { get; }
        public string Username { get; }
        public string Password { get; }
        public string CharSet { get; }
        public KaguyaApiCredentials Credentials { get; }
        public void CreateAppsettingsFile() { }
    }
}