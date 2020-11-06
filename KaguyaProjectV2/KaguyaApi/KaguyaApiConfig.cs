using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;

namespace KaguyaProjectV2.KaguyaApi
{
    public sealed class KaguyaApiConfig : IDbConfig
    {
        public KaguyaApiConfig(IBotConfig botConfig)
        {
            string[] dbSplits = botConfig.MySqlServer.Split(':');
            ServerIp = dbSplits[0];
            Port = ushort.Parse(dbSplits[1]);
            SchemaName = botConfig.MySqlSchema;
            Username = botConfig.MySqlUsername;
            Password = botConfig.MySqlPassword;
            CharSet = "utf8mb4";
        }

        public KaguyaApiConfig() : this(ConfigProperties.BotConfig) { }
        public string ServerIp { get; }
        public ushort Port { get; }
        public string SchemaName { get; }
        public string Username { get; }
        public string Password { get; }
        public string CharSet { get; }
    }
}