using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class UnWarn : InteractiveBase<ShardedCommandContext>
    {
        [Command("unwarn")]
        [Alias("uw")]
        [Summary("Removes a warning from a user. A list of the user's 7 most recent warnings will be displayed in chat. The " +
                 "moderator executing this command may then choose which warning to remove, based on this index.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UnWarnUser(IGuildUser user)
        {
            var server = ServerQueries.GetServer(Context.Guild.Id);
            var warnings = ServerQueries.GetWarnedUser(Context.Guild.Id, user.Id);
            var fields = new List<EmbedFieldBuilder>();

            int j = 0;

            if (j > warnings.Count && !server.IsPremium)
                j = 4;
            else if (j > warnings.Count && server.IsPremium)
                j = 10;
            else
                j = warnings.Count;

            if (warnings.Count == 0)
            {
                var reply = new KaguyaEmbedBuilder
                {
                    Description = $"{user.Username} has no warnings to remove!"
                };
                reply.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: reply.Build());
            }

            for (int i = 0; i < j; i++)
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
                Fields = fields
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}