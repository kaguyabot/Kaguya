using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable SwitchStatementMissingSomeCases

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class WarnPunishments : InteractiveBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("WarnSettings", RunMode = RunMode.Async)]
        [Alias("warnset", "ws")]
        [Summary("Allows a server Administrator to configure the server's warn-punishment scheme. " +
                 "Admins have the ability to configure up to `four` actions that get triggered " +
                 "when a user reaches a set amount of warnings. These four options are `mute`, " +
                 "`kick`, `shadowban`, and `ban`.\n\n" +
                 "Configure the action by typing the name of the action followed by the amount of " +
                 "warnings that should trigger the action, ranging from `1-99` warnings.\n\n" +
                 "If you want to `mute` users after `3` warnings, you would type the command " +
                 "followed by `mute 3`")]
        [Remarks("<action> <warnings>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command(string action, int warnings)
        {
            var actions = new string[] {"mute", "kick", "shadowban", "ban"};

            if(actions.All(x => x.ToLower() != action))
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

            if (warnings < 1 || warnings > 99)
            {
                var numError = new KaguyaEmbedBuilder
                {
                    Description = "The amount of warnings must be between `1` and `99`."
                };
                numError.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: numError.Build());
                return;
            }

            var server = await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var serverActions = await ServerQueries.GetWarnConfigForServerAsync(server.Id);
            var newSetting = new WarnSetting();

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

            await ServerQueries.AddOrReplaceWarnSettingAsync(newSetting);

            var successEmbed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully updated the warn triggers for " +
                              $"`{Context.Guild.Name}`. Here's what I've got:\n\n" +
                              $"`Mute`: After `{newSetting.Mute.ToWords()}` warnings." +
                              $"`Kick`: After `{newSetting.Kick.ToWords()}` warnings." +
                              $"`Shadowban`: After `{newSetting.Shadowban.ToWords()}` warnings." +
                              $"`Ban`: After `{newSetting.Ban.ToWords()}` warnings."
            };
            successEmbed.SetColor(EmbedColor.PINK);

            await ReplyAsync(embed: successEmbed.Build());
        }
    }
}
