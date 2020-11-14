using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Warn : KaguyaBase
    {
        [AdminCommand]
        [Command("Warn")]
        [Alias("w")]
        [Summary("Adds a warning to a user. If the server has a preconfigured warning-punishment scheme " +
                 "(via the `warnset` command), the user will be actioned accordingly. Upon receiving " +
                 "a warning, the user is DM'd with who warned them, why they were warned, and what server " +
                 "they received the warning from.")]
        [Remarks("<user> [reason]")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AddWarn(SocketGuildUser user, [Remainder] string reason = null)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

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

            await DatabaseQueries.InsertAsync(wu);
            WarnEvent.Trigger(server, wu);

            try
            {
                await user.SendMessageAsync(embed: (await WarnEmbed(wu, Context)).Build());
            }
            catch (Exception)
            {
                await ConsoleLogger.LogAsync($"Failed to DM user {user.Id} that they have been " +
                                             $"warned in guild {server.ServerId}. " +
                                             $"Database updated regardless.", LogLvl.DEBUG);
            }

            await ReplyAsync(embed: (await Reply(wu, user)).Build());

            await DatabaseQueries.UpdateAsync(server);
        }

        private static async Task<KaguyaEmbedBuilder> WarnEmbed(WarnedUser user, ICommandContext context)
        {
            List<WarnedUser> curWarns = await DatabaseQueries.GetAllForServerAndUserAsync<WarnedUser>(user.UserId, context.Guild.Id);
            int curCount = curWarns.Count;
            var embed = new KaguyaEmbedBuilder
            {
                Title = "⚠️ Warning Received",
                Description = $"Warned from `[Server: {context.Guild} | ID: {context.Guild.Id}]`\n" +
                              $"Warned by: `[User: {context.User} | ID: {context.User.Id}]`\n" +
                              $"Reason: `{user.Reason}`",
                Footer = new EmbedFooterBuilder
                {
                    Text =
                        $"You currently have {curCount} warnings."
                }
            };

            embed.SetColor(EmbedColor.RED);

            return embed;
        }

        private static async Task<KaguyaEmbedBuilder> Reply(WarnedUser user, SocketGuildUser warnedUser)
        {
            List<WarnedUser> curWarns = await DatabaseQueries.GetAllForServerAndUserAsync<WarnedUser>(user.UserId, warnedUser.Guild.Id);
            int curCount = curWarns.Count;

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully warned user `{warnedUser}`\nReason: `{user.Reason}`",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{warnedUser.Username} now has {curCount} warnings."
                }
            };

            return embed;
        }
    }
}