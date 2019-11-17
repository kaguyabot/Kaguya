using Discord;
using KaguyaProjectV2.Core.Handlers;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    /// <summary>
    /// A class containing all of the properties for premium features, such as logging warnings, unwarns, etc.
    /// All premium actions are listed in the `PremiumModerationAction` enum.
    /// </summary>
    public class PremiumModerationLog
    {
        public IGuildUser Moderator { get; set; }
        public IGuildUser ActionRecipient { get; set; }
        public PremiumModerationActionHandler Action { get; set; }
        public string Reason { get; set; }

        public static KaguyaEmbedBuilder ModerationLogEmbed(PremiumModerationLog log)
        {
            string actionTitle = "User ";
            string embedUrl = "";
            string reason = log.Reason ?? "None specified";
            
            switch (log.Action)
            {
                case PremiumModerationActionHandler.SHADOWBAN:
                    actionTitle += "Shadowbanned";
                    embedUrl = "https://i.imgur.com/86tSNSa.png";
                    break;
                case PremiumModerationActionHandler.UNSHADOWBAN:
                    actionTitle += "UnShadowbanned";
                    embedUrl = "https://i.imgur.com/szeC3hH.png";
                    break;
                case PremiumModerationActionHandler.MUTE:
                    actionTitle += "Muted";
                    embedUrl = "https://i.imgur.com/D1y3A7E.png";
                    break;
                case PremiumModerationActionHandler.UNMUTE:
                    actionTitle += "Unmuted";
                    embedUrl = "https://i.imgur.com/9x2MHFI.png";
                    break;
                case PremiumModerationActionHandler.WARN:
                    actionTitle += "Warned";
                    embedUrl = "https://i.imgur.com/LZmdn9k.png";
                    break;
                case PremiumModerationActionHandler.UNWARN:
                    actionTitle += "Unwarned";
                    embedUrl = "https://i.imgur.com/915ZT6q.png";
                    break;
            }

            return new KaguyaEmbedBuilder
            {
                Title = actionTitle,
                Description = $"User Actioned: `[Name: {log.ActionRecipient} | ID: {log.ActionRecipient.Id}]`\n" +
                              $"Punisher: `[Name: {log.Moderator} | ID: {log.Moderator.Id}]`\n" +
                              $"Reason: `{reason}`",
                ThumbnailUrl = embedUrl
            };
        }
    }

    /// <summary>
    /// An enum containing all of the premium loggable moderation actions.
    /// </summary>
    public enum PremiumModerationActionHandler
    {
        SHADOWBAN,
        MUTE,
        WARN,
        UNSHADOWBAN,
        UNMUTE,
        UNWARN
    }
}
