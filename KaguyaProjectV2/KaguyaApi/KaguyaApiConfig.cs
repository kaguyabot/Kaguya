using KaguyaProjectV2.KaguyaApi.Database;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;

namespace KaguyaProjectV2.KaguyaApi
{
    public sealed class KaguyaApiConfig : DbConfig
    {
        public override string ServerIp { get; }
        public override ushort Port { get; }
        public override string SchemaName { get; }
        public override string Username { get; }
        public override string Password { get; }
        public new string CharSet => base.CharSet;
        
        public KaguyaApiCredentials Credentials { get; }
        
        // todo: public void CreateAppsettings.json() {}

        public KaguyaApiConfig(IBotConfig botConfig, KaguyaApiCredentials credentials)
        {
            string[] dbSplits = botConfig.MySqlServer.Split(':');
            this.ServerIp = dbSplits[0];
            this.Port = ushort.Parse(dbSplits[1]);
            this.SchemaName = botConfig.MySqlDatabase;
            this.Username = botConfig.MySqlUsername;
            this.Password = botConfig.MySqlPassword;
            this.Credentials = credentials;
        }

        public KaguyaApiConfig()
        {
            this.Credentials = new KaguyaApiCredentials();
        }
    }
}