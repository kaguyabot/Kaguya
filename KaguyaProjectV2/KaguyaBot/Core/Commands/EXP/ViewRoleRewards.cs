using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.Common;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class ViewRoleRewards : KaguyaBase
    {
        [AdminCommand]
        [Command("ViewRoleRewards")]
        [Alias("rolerewards", "rrs", "vrrs", "vrr")]
        [Summary("Allows an administrator to view the current role rewards configuration.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var rr = server.RoleRewards.ToList();

            string rrStr = "";
            int i = 0;
            foreach (var r in rr)
            {
                i++;
                rrStr += $"**#{i}.** Level {r.Level} - Role: <@&{r.RoleId}>\n";
            }

            var embed = new KaguyaEmbedBuilder(rrStr.IsNullOrEmpty() ? EmbedColor.RED : EmbedColor.BLUE)
            {
                Title = $"Kaguya Role Rewards Configuration",
                Description = rrStr.IsNullOrEmpty() ? "`No configuration found.`" : rrStr
            };

            await SendEmbedAsync(embed);
        }
    }
}
