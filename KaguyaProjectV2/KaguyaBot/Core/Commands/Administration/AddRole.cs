using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AddRole : ModuleBase<ShardedCommandContext>
    {
        [Command("addrole")]
        [Alias("ar")]
        [Summary("Adds a role, or list of roles, to a user.")]
        [Remarks("ar <user> <role>\nar Stage Penguins \"Twitch Streamer\" \"Bot Dev\" SpaceMonkeys ...")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemovePhrase(IGuildUser user, params IRole[] args)
        {
            await user.AddRolesAsync(args);

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"{user} has been given {args.Length} roles."
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
