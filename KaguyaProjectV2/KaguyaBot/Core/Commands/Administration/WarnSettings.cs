using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable SwitchStatementMissingSomeCases

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class WarnSettings : KaguyaBase
    {
        [AdminCommand]
        [Command("WarnSettings")]
        [Alias("warnset", "ws")]
        [Summary("Allows a server Administrator to configure the server's warn-punishment scheme. " +
                 "Admins have the ability to configure up to `four` actions that get triggered " +
                 "when a user reaches a set amount of warnings. These four options are `mute`, " +
                 "`kick`, `shadowban`, and `ban`.\n\n" +
                 "Configure the action by typing the name of the action followed by the amount of " +
                 "warnings that should trigger the action, ranging from `1-99` warnings.\n\n" +
                 "If you want to `mute` users after `3` warnings, you would type the command " +
                 "followed by `mute 3`.\n\n" +
                 "__**To disable a trigger**__, set the number of warnings to `0`.\n" +
                 "__**To view your current settings**__, use the command without any arguments.")]
        [Remarks("\n<action> <warnings>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command(string action = null, int warnings = 0)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var serverActions = await DatabaseQueries.GetFirstForServerAsync<WarnSetting>(server.ServerId);

            if (action == null && warnings == 0)
            {
                if (serverActions == null)
                {
                    var nullErrorEmbed = new KaguyaEmbedBuilder
                    {
                        Description = "Nothing has been configured."
                    };
                    nullErrorEmbed.SetColor(EmbedColor.RED);

                    await ReplyAsync(embed: nullErrorEmbed.Build());
                    return;
                }

                var curSettingsEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"`{Context.Guild.Name}`. Here's what I've got:\n\n" +
                                  $"`Mute`: After `{(serverActions.Mute.IsZero() ? "N/A" : serverActions.Mute.ToString().Humanize())}` warnings.\n" +
                                  $"`Kick`: After `{(serverActions.Kick.IsZero() ? "N/A" : serverActions.Kick.ToString().Humanize())}` warnings.\n" +
                                  $"`Shadowban`: After `{(serverActions.Shadowban.IsZero() ? "N/A" : serverActions.Shadowban.ToString().Humanize())}` warnings.\n" +
                                  $"`Ban`: After `{(serverActions.Ban.IsZero() ? "N/A" : serverActions.Ban.ToString().Humanize())}` warnings.\n"
                };
                await ReplyAsync(embed: curSettingsEmbed.Build());
                return;
            }

            var actions = new string[] { "mute", "kick", "shadowban", "ban" };

            if (actions.All(x => x.ToLower() != action))
            {
                var errorEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"`{Context.Message.Content}` is not a valid action. The only valid " +
                                  $"actions are `mute`, `kick`, `shadowban`, and `ban`."
                };
                errorEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: errorEmbed.Build());
                return;
            }

            if (warnings < 0 || warnings > 99)
            {
                var numError = new KaguyaEmbedBuilder
                {
                    Description = "The amount of warnings must be between `0` and `99`."
                };
                numError.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: numError.Build());
                return;
            }

            var newSetting = new WarnSetting
            {
                ServerId = Context.Guild.Id
            };

            if (serverActions != null)
            {
                newSetting = serverActions;
            }

            switch (action.ToLower())
            {
                case "mute":
                    newSetting.Mute = warnings; break;
                case "kick":
                    newSetting.Kick = warnings; break;
                case "shadowban":
                    newSetting.Shadowban = warnings; break;
                case "ban":
                    newSetting.Ban = warnings;
                    break;
            }

            await DatabaseQueries.InsertOrReplaceAsync(newSetting);

            var successEmbed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully updated the warn triggers for " +
                              $"`{Context.Guild.Name}`. Here's what I've got:\n\n" +
                              $"`Mute`: After `{(newSetting.Mute.IsZero() ? "N/A" : newSetting.Mute.ToString().Humanize())}` warnings.\n" +
                              $"`Kick`: After `{(newSetting.Kick.IsZero() ? "N/A" : newSetting.Kick.ToString().Humanize())}` warnings.\n" +
                              $"`Shadowban`: After `{(newSetting.Shadowban.IsZero() ? "N/A" : newSetting.Shadowban.ToString().Humanize())}` warnings.\n" +
                              $"`Ban`: After `{(newSetting.Ban.IsZero() ? "N/A" : newSetting.Ban.ToString().Humanize())}` warnings.\n"
            };
            successEmbed.SetColor(EmbedColor.PINK);

            await ReplyAsync(embed: successEmbed.Build());
        }
    }
}
