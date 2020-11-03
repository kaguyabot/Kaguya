namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IDbConfig
    {
        public string ServerIp { get; }
        public ushort Port { get; }
        public string SchemaName { get; }
        public string Username { get; }
        public string Password { get; }
        public string CharSet { get; }
    }
}