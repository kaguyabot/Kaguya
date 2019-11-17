using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Warn : ModuleBase<ShardedCommandContext>
    {
        [Command("warn")]
        [Alias("w")]
        [Summary("Adds a warning to a user. If the server has a preconfigured warning-punishment scheme " +
                 "(via the `warnpunishments` command), the user will be actioned accordingly. Upon receiving " +
                 "a warning, the user is DM'd with who warned them, why they were warned, and what server they received the warning from.")]
        [Remarks("<user> [reason]\nSmellyPanda#1955\nRealMeanUser#0000 You can't say that here!")]
        public async Task AddWarn(IGuildUser user, [Remainder]string reason = null)
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);

            if (reason == null)
                reason = "No reason specified.";

            var wu = new WarnedUser
            {
                ServerId = Context.Guild.Id,
                UserId = user.Id,
                Reason = reason
            };

            ServerQueries.AddWarnedUser(wu);
            await DmWarnedUser(wu, Context);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully warned user `{user}`.\nReason: `{reason}`"
            };

            if (server.IsPremium && server.ModLog != 0)
            {
                var premLog = new PremiumModerationLog
                {
                    Server = server,
                    Moderator = (SocketGuildUser)Context.User,
                    ActionRecipient = (SocketGuildUser)user,
                    Action = PremiumModerationActions.WARN
                };

                var logChannel = GlobalProperties.client.GetGuild(server.Id).GetTextChannel(server.ModLog);
                await logChannel.SendMessageAsync(embed: PremiumModerationLog.ModerationLogEmbed(premLog).Build());
            }
        }

        private static async Task DmWarnedUser(WarnedUser user, ICommandContext context)
        {
            var dmChannel = await GlobalProperties.client.GetDMChannelAsync(user.UserId);
            await dmChannel.SendMessageAsync(embed: WarnEmbed(user, context).Build());
        }

        private static KaguyaEmbedBuilder WarnEmbed(WarnedUser user, ICommandContext context)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Title = "⚠️ Warning Received",
                Description = $"Warned from `[Server: {context.Guild} | ID: {context.Guild.Id}]`\n" +
                              $"Warned by: `[User: {context.User} | ID: {context.User.Id}]`\n" +
                              $"Reason: `{user.Reason}`"
            };
            embed.SetColor(EmbedColor.RED);
            return embed;
        }
    }
}
