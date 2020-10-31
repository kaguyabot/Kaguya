using KaguyaProjectV2.KaguyaApi.Database;

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
    }
}