using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Warn : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("warn")]
        [Alias("w")]
        [Summary("Adds a warning to a user. If the server has a preconfigured warning-punishment scheme " +
                 "(via the `warnpunishments` command), the user will be actioned accordingly. Upon receiving " +
                 "a warning, the user is DM'd with who warned them, why they were warned, and what server they received the warning from.")]
        [Remarks("<user> [reason]\nSmellyPanda#1955\nRealMeanUser#0000 You can't say that here!")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AddWarn(IGuildUser user, [Remainder] string reason = null)
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            server.TotalAdminActions++;

            if (reason == null)
                reason = "No reason specified.";

            var wu = new WarnedUser
            {
                ServerId = Context.Guild.Id,
                UserId = user.Id,
                ModeratorName = Context.User.ToString(),
                Reason = reason,
                Date = DateTime.Now.ToOADate()
            };

            ServerQueries.AddWarnedUser(wu);

            await user.SendMessageAsync(embed: WarnEmbed(wu, Context).Build());
            await ReplyAsync(embed: Reply(wu, user, Context).Build());

            if (server.IsPremium && server.ModLog != 0)
            {
                var premLog = new PremiumModerationLog
                {
                    Server = server,
                    Moderator = (SocketGuildUser) Context.User,
                    ActionRecipient = (SocketGuildUser) user,
                    Reason = reason,
                    Action = PremiumModActionHandler.WARN
                };

                var logChannel = ConfigProperties.client.GetGuild(server.Id).GetTextChannel(server.ModLog);
                await logChannel.SendMessageAsync(embed: PremiumModerationLog.ModerationLogEmbed(premLog).Build());
            }

            ServerQueries.UpdateServer(server);
        }

        private static KaguyaEmbedBuilder WarnEmbed(WarnedUser user, ICommandContext context)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Title = "⚠️ Warning Received",
                Description = $"Warned from `[Server: {context.Guild} | ID: {context.Guild.Id}]`\n" +
                              $"Warned by: `[User: {context.User} | ID: {context.User.Id}]`\n" +
                              $"Reason: `{user.Reason}`",
                Footer = new EmbedFooterBuilder
                {
                    Text =
                        $"You currently have {ServerQueries.GetWarnedUser(user.ServerId, user.UserId).Count} warnings."
                }
            };
            embed.SetColor(EmbedColor.RED);
            return embed;
        }

        private static KaguyaEmbedBuilder Reply(WarnedUser user, IGuildUser warnedUser, ICommandContext context)
        {
            var warnCount = ServerQueries.GetWarnedUser(user.ServerId, user.UserId).Count;
            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully warned user `{warnedUser}`\nReason: `{user.Reason}`",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{warnedUser.Username} currently has {warnCount} warnings."
                }
            };
            return embed;
        }
    }
}