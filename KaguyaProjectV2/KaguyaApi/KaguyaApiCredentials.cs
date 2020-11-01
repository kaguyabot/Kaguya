using KaguyaProjectV2.KaguyaBot.Core.Interfaces;

namespace KaguyaProjectV2.KaguyaApi
{
    public class KaguyaApiCredentials
    {
        public string TopGgAuthorization { get; }

        public KaguyaApiCredentials(IBotConfig botConfig)
        {
            TopGgAuthorization = botConfig.TopGgApiKey;
        }
        
        public KaguyaApiCredentials() {}
    }
}