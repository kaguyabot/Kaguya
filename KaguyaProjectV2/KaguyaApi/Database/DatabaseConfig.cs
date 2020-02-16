namespace KaguyaProjectV2.KaguyaApi.Database
{
    public class DatabaseConfig
    {
        public string ServerIp { get; set; }
        public ushort Port { get; set; }
        public string SchemaName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CharSet { get; } = "utf8mb4";

        public DatabaseConfig()
        {
        }
    }
}
