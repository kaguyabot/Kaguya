namespace KaguyaProjectV2.KaguyaApi.Database
{
    public abstract class DbConfig
    {
        public abstract string ServerIp { get; }
        public abstract ushort Port { get; }
        public abstract string SchemaName { get; }
        public abstract string Username { get; }
        public abstract string Password { get; }
        public string CharSet { get; } = "utf8mb4";
    }
}