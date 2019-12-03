using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    /// <summary>
    /// A class containing all of the properties for premium features, such as logging warnings, unwarns, etc.
    /// All premium actions are listed in the `PremiumModerationAction` enum.
    /// </summary>
    public class PremiumModerationLog
    {
        public Server Server { get; set; }
        public SocketGuildUser Moderator { get; set; }
        public SocketGuildUser ActionRecipient { get; set; }
        public PremiumModActionHandler Action { get; set; }
        public string Reason { get; set; }

        public static async Task SendModerationLog(PremiumModerationLog log)
        {
            string actionTitle = "User ";
            string embedUrl = "";
            string reason = log.Reason ?? "None specified";
            
            switch (log.Action)
            {
                case PremiumModActionHandler.SHADOWBAN:
                    actionTitle += "Shadowbanned ";
                    embedUrl = "https://i.imgur.com/86tSNSa.png";
                    break;
                case PremiumModActionHandler.UNSHADOWBAN:
                    actionTitle += "UnShadowbanned ";
                    embedUrl = "https://i.imgur.com/szeC3hH.png";
                    break;
                case PremiumModActionHandler.MUTE:
                    actionTitle += "Muted ";
                    embedUrl = "https://i.imgur.com/D1y3A7E.png";
                    break;
                case PremiumModActionHandler.UNMUTE:
                    actionTitle += "Unmuted ";
                    embedUrl = "https://i.imgur.com/9x2MHFI.png";
                    break;
                case PremiumModActionHandler.WARN:
                    actionTitle += "Warned ";
                    embedUrl = "https://i.imgur.com/LZmdn9k.png";
                    break;
                case PremiumModActionHandler.UNWARN:
                    actionTitle += "Unwarned ";
                    embedUrl = "https://i.imgur.com/915ZT6q.png";
                    break;
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = actionTitle + $"| `Case: #{log.Server.TotalAdminActions}`",
                Description = $"User Actioned: `[Name: {log.ActionRecipient} | ID: {log.ActionRecipient.Id}]`\n" +
                              $"Punisher: `[Name: {log.Moderator} | ID: {log.Moderator.Id}]`\n" +
                              $"Reason: `{reason}`",
                ThumbnailUrl = embedUrl
            };

            await ConfigProperties.client.GetGuild(log.Server.Id).GetTextChannel(log.Server.ModLog)
                .SendMessageAsync(embed: embed.Build());

            await ConsoleLogger.Log($"Premium moderation log sent for a server.", LogLevel.DEBUG);
        }
    }

    /// <summary>
    /// An enum containing all of the premium loggable moderation actions.
    /// </summary>
    public enum PremiumModActionHandler
    {
        SHADOWBAN,
        MUTE,
        WARN,
        UNSHADOWBAN,
        UNMUTE,
        UNWARN
    }
}