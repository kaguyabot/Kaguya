using System.ComponentModel.DataAnnotations;

namespace Kaguya.Discord.Options
{
    public class DiscordConfigurations
    {
	    public static string Position { get; } = "DiscordSettings";

	    // login details

	    [MinLength(1)]
        public string BotToken { get; set; }

        public int? MessageCacheSize { get; set; }
        public bool? AlwaysDownloadUsers { get; set; }
    }
}