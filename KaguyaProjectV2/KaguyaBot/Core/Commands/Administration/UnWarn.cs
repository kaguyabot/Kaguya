using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class UnWarn : InteractiveBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("Unwarn")]
        [Alias("uw")]
        [Summary("Removes a warning from a user. A list of the user's 4 most recent warnings (9 if server is premium) " +
                 "will be displayed in chat. The moderator executing this command may then choose which warnings to " +
                 "remove by clicking on the supplied reactions. A reason may be provided, and if the server is " +
                 "of premium status, it will be logged.")]
        [Remarks("<user> [reason]")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UnWarnUser(IGuildUser user, string reason = null)
        {
            var server = await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var warnings = await ServerQueries.GetWarningsForUserAsync(Context.Guild.Id, user.Id);
            var fields = new List<EmbedFieldBuilder>();

            int warnCount = warnings.Count;

            if (warnCount > 4 && !server.IsPremium)
                warnCount = 4;
            if (warnCount > 9 && server.IsPremium)
                warnCount = 9;

            if (warnings.Count == 0)
            {
                var reply = new KaguyaEmbedBuilder
                {
                    Description = $"{user.Username} has no warnings to remove!"
                };
                reply.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: reply.Build());
                return;
            }

            for (int i = 0; i < warnCount; i++)
            {
                var field = new EmbedFieldBuilder
                {
                    Name = $"Warning #{i + 1}",
                    Value = $"Reason: `{warnings.ElementAt(i).Reason}`"
                };

                fields.Add(field);
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Warnings for {user}",
                Fields = fields,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Select a reaction to remove the warning."
                }
            };

            await ReactionReply(user, warnings, embed.Build(), warnCount, server, reason);
        }

        private async Task ReactionReply(IGuildUser user, List<WarnedUser> warnings, Embed embed, 
            int warnCount, Server server, string reason)
        {
            var emojis = HelpfulObjects.EmojisOneThroughNine();

            var data = new ReactionCallbackData("", embed, false, false, TimeSpan.FromSeconds(300), c => 
                c.Channel.SendMessageAsync(embed: TimeoutEmbed()));
            var callbacks = new List<(IEmote, Func<SocketCommandContext, SocketReaction, Task>)>();

            for (int j = 0; j < warnCount; j++)
            {
                int j1 = j;
                callbacks.Add((emojis[j], async (c, r) =>
                {
                    await ServerQueries.RemoveWarnedUserAsync(warnings.ElementAt(j1));
                    await c.Channel.SendMessageAsync($"{r.User.Value.Mention} " +
                                                     $"`Successfully removed warning #{j1 + 1}`");

                    if (server.IsPremium && server.ModLog != 0)
                    {
                        var logChannel = 
                            ConfigProperties.Client.GetChannel(server.ModLog) as SocketTextChannel;

                        await PremiumModerationLog.SendModerationLog(new PremiumModerationLog
                        {
                            Server = server,
                            Moderator = Context.User as SocketGuildUser,
                            ActionRecipient = user as SocketGuildUser,
                            Action = PremiumModActionHandler.UNWARN,
                            Reason = reason
                        });
                    }
                }));
            }

            data.SetCallbacks(callbacks);
            await InlineReactionReplyAsync(data);
        }

        private static Embed TimeoutEmbed()
        {
            KaguyaEmbedBuilder timeoutEmbed = new KaguyaEmbedBuilder
            {
                Description = "Warn remove has timed out. (5 minutes)"
            };

            timeoutEmbed.SetColor(EmbedColor.RED);
            return timeoutEmbed.Build();
        }
    }
}