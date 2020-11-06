namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IBotConfig
    {
        public string Token { get; set; }
        public ulong BotOwnerId { get; set; }
        public int LogLevelNumber { get; set; }
        public string DefaultPrefix { get; set; }
        public string OsuApiKey { get; set; }
        public string TopGgApiKey { get; set; }
        public string MySqlUsername { get; set; }
        public string MySqlPassword { get; set; }
        public string MySqlServer { get; set; }
        public string MySqlSchema { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchAuthToken { get; set; }
        public string DanbooruUsername { get; set; }
        public string DanbooruApiKey { get; set; }
        public int TopGgWebhookPort { get; set; }
    }
}