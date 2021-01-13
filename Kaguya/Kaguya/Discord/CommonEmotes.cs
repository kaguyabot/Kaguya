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

        public readonly IEmote[] EmojisOneThroughFive =
        {
            new Emoji("1️⃣"),
            new Emoji("2️⃣"),
            new Emoji("3️⃣"),
            new Emoji("4️⃣"),
            new Emoji("5️⃣")
        };
        public IEmote CheckMarkEmoji => new Emoji("✅");
        public IEmote RedCrossEmote => GetEmote("RedCross");

        private IEmote GetEmote(string emoteName)
        {
            SocketGuild emoteGuild = _client.GetGuild(_configurations.Value.EmoteGuildId);

            if (emoteGuild == null)
            {
                _logger.LogWarning("Emote server could not be found. Some emotes will not be populated.");
            }

            IEmote toModify = emoteGuild?.Emotes.FirstOrDefault(x => x.Name.ToLower() == emoteName.ToLower());

            if (toModify == null)
            {
                _logger.LogWarning("Emote not populated. Using default ❌ emote.");
                toModify = new Emoji("🦆");
            }

            return toModify;
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