namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IBotConfig
    {
        public string Token { get; }
        public ulong BotOwnerId { get; }
        public int LogLevelNumber { get; }
        public string DefaultPrefix { get; }
        public string OsuApiKey { get; }
        public string TopGgApiKey { get; }
        public string MySqlUsername { get; }
        public string MySqlPassword { get; }
        public string MySqlServer { get; }
        public string MySqlSchema { get; }
        public string TwitchClientId { get; }
        public string TwitchAuthToken { get; }
        public string DanbooruUsername { get; }
        public string DanbooruApiKey { get; }
        public int TopGgWebhookPort { get; }
    }
}