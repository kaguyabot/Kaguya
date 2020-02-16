using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable RedundantIfElseBlock

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class AddRoleReward : KaguyaBase
    {
        [ExpCommand]
        [Command("AddRoleReward")]
        [Alias("arr")]
        [Summary("Allows an Administrator to add a level-up triggered role reward. " +
                 "When a user reaches the specified level, the role you specify will " +
                 "automatically be assigned to them.")]
        [Remarks("<level> <role name or ID>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Command(int level, [Remainder]SocketRole role)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            int limit = server.IsPremium ? 15 : 3;

            if (level < 1)
            {
                await SendBasicErrorEmbedAsync("The `level` parameter must be at least `1`.");
                return;
            }

            if (level > 100000)
            {
                await SendBasicErrorEmbedAsync($"The maximum level for a role reward is `{100000:N0}`");
                return;
            }

            if (server.RoleRewards.Count() == limit)
            {
                string limitStr = (server.IsPremium ? $"Your premium limit: 15." : $"Your non-premium limit: 3.");
                string baseLimitStr = "You have reached your limit of allowed " +
                                      $"concurrent role rewards. Please delete one " +
                                      $"if you still wish to create this reward.\n\n {limitStr}";

                if (server.IsPremium)
                    throw new KaguyaSupportException(baseLimitStr);
                else
                    throw new KaguyaPremiumException("More than 3 role rewards.\n" + baseLimitStr);
            }

            var rr = new ServerRoleReward
            {
                ServerId = Context.Guild.Id,
                RoleId = role.Id,
                Level = level,
                Server = server
            };

            await DatabaseQueries.InsertIfNotExistsAsync(rr);

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Kaguya Role Rewards",
                Description = $"{Context.User.Mention} Whenever a user reaches " +
                              $"`Server Level {level}`, I will now reward them with {role.Mention}."
            };

            await SendEmbedAsync(embed);
        }
    }
}
