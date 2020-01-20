using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;


namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class CreateRole : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("CreateRole")]
        [Alias("cr")]
        [Summary("Creates a role, or a list of roles. New roles are separated by spaces. If a role " +
                 "has spaces in the name, surround it with `\"\"`.")]
        [Remarks("<role> {...}\n\"Some long role\" Moofins")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRole(params string[] roleNames)
        {
            if (roleNames.Count() > 1)
            {
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
                {
                    Title = "Roles Created"
                };

                foreach (var role in roleNames)
                {
                    embed.AddField("Role Created", $"`{role}` has been created.");
                    await Context.Guild.CreateRoleAsync(role);
                }
                await ReplyAsync(embed: embed.Build());
            }
            else if (roleNames.Count() == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"**{Context.User.Mention} Successfully created role `{roleNames[0]}`**"
                };
                await Context.Guild.CreateRoleAsync(roleNames[0]);

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"Please specify a role to create."
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}
