using System.ComponentModel.DataAnnotations;

namespace Kaguya.Discord.options
{
    public class DiscordConfigurations
    {
        // login details
        [MinLength(1)]
        public string BotToken { get; set; }

        public int? MessageCacheSize { get; set; }
        public bool? AlwaysDownloadUsers { get; set; }
    }
}