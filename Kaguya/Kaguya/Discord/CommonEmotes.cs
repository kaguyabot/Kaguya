using System.Linq;
using Discord;
using Discord.WebSocket;
using Kaguya.Discord.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaguya.Discord
{
    public class CommonEmotes
    {
        private readonly ILogger<CommonEmotes> _logger;
        private readonly DiscordShardedClient _client;
        private readonly IOptions<DiscordConfigurations> _configurations;
        public IEmote CheckMarkEmoji => new Emoji("✅");

        private IEmote _redCrossEmote { get; set; }

        public IEmote RedCrossEmote
        {
            get
            {
                if (_redCrossEmote == null)
                {
                    SocketGuild emoteGuild = _client.GetGuild(_configurations.Value.EmoteGuildId);

                    if (emoteGuild == null)
                    {
                        _logger.LogWarning("Emote server could not be found. Some emotes will not be populated.");
                    }

                    _redCrossEmote = emoteGuild?.Emotes.FirstOrDefault(x => x.Name.ToLower() == "redcross");

                    if (_redCrossEmote == null)
                    {
                        _logger.LogWarning("Redcross emote not populated. Using default ❌ emote.");
                        _redCrossEmote = new Emoji("❌");
                    }
                }
                
                return _redCrossEmote;
            }
        }
        
        public CommonEmotes(ILogger<CommonEmotes> logger, DiscordShardedClient client, 
            IOptions<DiscordConfigurations> configurations)
        {
            _logger = logger;
            _client = client;
            _configurations = configurations;
        }
    }
}