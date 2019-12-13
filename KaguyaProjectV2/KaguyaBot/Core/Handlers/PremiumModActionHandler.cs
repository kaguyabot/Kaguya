using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

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
            var logChannel = ConfigProperties.client.GetGuild(log.Server.Id).GetTextChannel(log.Server.ModLog);
            string actionTitle = "User ";
            string embedUrl = "";
            string reason = log.Reason ?? "None specified";

            switch (log.Action)
            {
                case PremiumModActionHandler.SHADOWBAN:
                    actionTitle += "Shadowbanned ";
                    embedUrl = "https://i.imgur.com/hCHifVn.png";
                    break;
                case PremiumModActionHandler.UNSHADOWBAN:
                    actionTitle += "UnShadowbanned ";
                    embedUrl = "https://i.imgur.com/NmLwYJB.png";
                    break;
                case PremiumModActionHandler.MUTE:
                    actionTitle += "Muted ";
                    embedUrl = "https://i.imgur.com/nnc3h7D.png";
                    break;
                case PremiumModActionHandler.UNMUTE:
                    actionTitle += "Unmuted ";
                    embedUrl = "https://i.imgur.com/eIBBSMw.png";
                    break;
                case PremiumModActionHandler.WARN:
                    actionTitle += "Warned ";
                    embedUrl = "https://i.imgur.com/IXvCjEg.png";
                    break;
                case PremiumModActionHandler.UNWARN:
                    actionTitle += "Unwarned ";
                    embedUrl = "https://i.imgur.com/QyNpRuW.png";
                    break;
                case PremiumModActionHandler.MESSAGEPURGE:
                    actionTitle = "Messages Bulk-Deleted ";
                    embedUrl = "https://i.imgur.com/2uqH08b.png";
                    break;
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = actionTitle + $"| `Case: #{log.Server.TotalAdminActions:N0}`",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Moderator/Admin",
                        Value = $"`[Name: {log.Moderator} | ID: {log.Moderator.Id}]`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Action Recipient",
                        Value = $"`[Name: {log.ActionRecipient} | ID: { log.ActionRecipient.Id}]`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Reason Given",
                        Value = $"`{reason}`"
                    }
                },
                ThumbnailUrl = embedUrl
            };

            log.Server.TotalAdminActions++;
            await ServerQueries.UpdateServer(log.Server);

            await logChannel.SendMessageAsync(embed: embed.Build());
            await ConsoleLogger.Log($"Premium moderation log sent for server {log.Server.Id}.", LogLevel.DEBUG);
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
        UNWARN,
        MESSAGEPURGE
    }
}