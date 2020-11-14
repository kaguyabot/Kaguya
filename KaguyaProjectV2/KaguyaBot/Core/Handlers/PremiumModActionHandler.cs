using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    /// <summary>
    /// A class containing all of the properties for premium features, such as logging warnings, unwarns, etc.
    /// All premium actions are listed in the `PremiumModerationAction` enum.
    /// </summary>
    public class PremiumModerationLog
    {
        public Server Server { get; set; }
        public IUser Moderator { get; set; }
        public IUser ActionRecipient { get; set; }
        public PremiumModActionHandler Action { get; set; }
        public string Reason { get; set; }

        // Todo: Redesign to match 1-line design of other log types.
        public static async Task SendModerationLog(PremiumModerationLog log)
        {
            if (!log.Server.IsPremium)
                return;

            SocketTextChannel logChannel = ConfigProperties.Client.GetGuild(log.Server.ServerId).GetTextChannel(log.Server.ModLog);

            if (logChannel == null)
                return;

            string actionTitle = "User ";
            string embedUrl = "";
            string reason = log.Reason ?? "None specified";

            switch (log.Action)
            {
                case PremiumModActionHandler.AUTOBAN:
                    actionTitle += "Auto-Banned ";
                    embedUrl = "https://i.imgur.com/P6Cgm8Z.png";

                    break;
                case PremiumModActionHandler.AUTOKICK:
                    actionTitle += "Auto-Kicked ";
                    embedUrl = "https://i.imgur.com/lUjW0uu.png";

                    break;
                case PremiumModActionHandler.AUTOMUTE:
                    actionTitle += "Auto-Muted ";
                    embedUrl = "https://i.imgur.com/nnc3h7D.png";

                    break;
                case PremiumModActionHandler.AUTOSHADOWBAN:
                    actionTitle += "Auto-Shadowbanned ";
                    embedUrl = "https://i.imgur.com/hCHifVn.png";

                    break;
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
                        Value = $"`[Name: {log.ActionRecipient} | ID: {log.ActionRecipient.Id}]`"
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
            await DatabaseQueries.UpdateAsync(log.Server);

            await logChannel.SendEmbedAsync(embed);
            await ConsoleLogger.LogAsync($"Premium moderation log sent for server {log.Server.ServerId}.", LogLvl.DEBUG);
        }
    }

    /// <summary>
    /// An enum containing all of the premium loggable moderation actions.
    /// </summary>
    public enum PremiumModActionHandler
    {
        AUTOBAN,
        AUTOKICK,
        AUTOMUTE,
        AUTOSHADOWBAN,
        MESSAGEPURGE,
        MUTE,
        SHADOWBAN,
        WARN,
        UNSHADOWBAN,
        UNMUTE,
        UNWARN
    }
}