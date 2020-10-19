using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.SchemaProvider;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class ConfigureAntiraidNotification : KaguyaBase
    {
        [AdminCommand]
        [Command("ConfigureAntiraidDM")]
        [Alias("ardm", "configureardm")]
        [Summary("This command allows a server administrator to configure the direct message sent to any user " +
            "punished by the anti-raid service. The ideal use for this feature is to notify a user of how or why " +
            "they were punished from the server.\n\n" +
            "**Example:**\n" +
            "- \"User, you were banned from our server by our anti-raid system. Rejoin here! <link>\"\n\n" +
            "**Customization:**\n" +
            "The arguments below may be supplied to customize your notification message further. Include these exactly " +
            "as shown, if desired, to have said information be displayed in the DM.\n\n" +
            "`{USERNAME}` = The user's name and tag. Example: Kaguya#2708\n" +
            "`{USERMENTION}` = Mention the user. Example: <@538910393918160916>\n" +
            "`{SERVER}` = The current name of the Discord server they were punished in.\n" +
            "`{PUNISHMENT}` = The type of punishment, or action, the user received, in past-tense form. " +
            "Example: Banned.\n\n" +
            "*Set your content as `{DISABLED}` to disable the DM notification feature.*\n" +
            "*Use without any arguments to display the current message.*")]
        [Remarks(
            "\n<content>\n{USERMENTION}, you were {PUNISHMENT} from {SERVER} because of our anti-raid service! Rejoin " +
            "here: mydiscord.url or contact us for more help.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command([Remainder] string content = null)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            string curArdm = server.AntiraidPunishmentDirectMessage;

            if (string.IsNullOrWhiteSpace(content))
            {
                var curContentEmbed = new KaguyaEmbedBuilder
                {
                    Title = "Kaguya Anti-Raid DM Notification"
                };

                string descr = string.Empty;
                if (string.IsNullOrWhiteSpace(curArdm))
                    descr = $"The anti-raid notifications for {Context.Guild.Name} are currently disabled.";
                else
                {
                    descr = $"Current Message:\n`{server.AntiraidPunishmentDirectMessage}`\n\n" +
                            $"Example: {GenerateDmString(curArdm, server.AntiRaid?.Action)}\n";
                }

                curContentEmbed.Description = descr;
                await SendEmbedAsync(curContentEmbed);

                return;
            }

            if (content == "{DISABLED}")
            {
                if (server.AntiraidPunishmentDirectMessage == null)
                {
                    await SendBasicErrorEmbedAsync($"The anti-raid DM notifications for {Context.Guild.Name} are " +
                                                   "already disabled!");

                    return;
                }

                server.AntiraidPunishmentDirectMessage = null;
                await DatabaseQueries.UpdateAsync(server);

                await SendBasicSuccessEmbedAsync($"Successfully disabled anti-raid DM notifications " +
                                                 $"for {Context.Guild.Name}.");

                return;
            }

            string action = server.AntiRaid?.Action;
            if (!string.IsNullOrEmpty(action))
            {
                // We have to hardcode this because Humanizer doesn't have functionality for converting to past tense.
                action = action.ToLower() switch
                {
                    "ban" => "banned",
                    "mute" => "muted",
                    "kick" => "kicked",
                    "shadowban" => "shadowbanned",
                    _ => throw new KaguyaSupportException("An unexpected value was encountered when determining the " +
                                                          "past-tense value for this server's anti-raid action. Please " +
                                                          "reconfigure your anti-raid and join our Support Discord if " +
                                                          "this error continues to occur.")
                };

                action = action.ToLower();
            }

            if (string.IsNullOrEmpty(action) && content.Contains("{PUNISHMENT}"))
            {
                await SendBasicErrorEmbedAsync($"You must configure an anti-raid first before you can supply the " +
                                               $"`{{PUNISHMENT}}` argument. Do so with the `{server.CommandPrefix}antiraid` command.");

                return;
            }

            server.AntiraidPunishmentDirectMessage = content;
            await DatabaseQueries.UpdateAsync(server);

            StringBuilder sb = GenerateDmString(content, action);
            var embed = new KaguyaEmbedBuilder(EmbedColor.VIOLET)
            {
                Title = "Anti-Raid DM Configuration",
                Description = "Successfully set your custom anti-raid DM notification. This message " +
                              "will be sent to all users who are punished by the anti-raid service for this server.\n\n" +
                              "__Example Message:__\n" +
                              sb,
                Footer = new EmbedFooterBuilder
                {
                    Text = "To disable this feature, re-use this command with {DISABLED} as your message's content."
                }
            };

            await SendEmbedAsync(embed);
        }

        private StringBuilder GenerateDmString(string content, string action)
        {
            var sb = new StringBuilder(content);
            sb = sb.Replace("{USERNAME}", Client.CurrentUser.UsernameAndDescriminator());
            sb = sb.Replace("{USERMENTION}", Client.CurrentUser.Mention);
            sb = sb.Replace("{SERVER}", Context.Guild.Name);
            sb = sb.Replace("{PUNISHMENT}", action);

            return sb;
        }
    }
}