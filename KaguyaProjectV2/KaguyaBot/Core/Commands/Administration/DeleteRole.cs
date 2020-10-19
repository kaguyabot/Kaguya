using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class DeleteRole : KaguyaBase
    {
        [AdminCommand]
        [Command("DeleteRole")]
        [Alias("dr")]
        [Summary("Deletes a role from the server. If multiple roles with the same name are found, " +
                 "a context menu will appear where you can choose to delete individual roles or all roles " +
                 "with that name.")]
        [Remarks("<role>\nPenguins\nSome role with spaces")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRole([Remainder] string targetRole)
        {
            var roles = new List<SocketRole>();

            roles = Context.Guild.Roles.Where(r => r.Name.ToLower() == targetRole.ToLower()).ToList();

            if (roles.Count > 1)
            {
                var emojis = new Emoji[]
                {
                    new Emoji("1⃣"),
                    new Emoji("2⃣"),
                    new Emoji("3⃣"),
                    new Emoji("4⃣"),
                    new Emoji("5⃣"),
                    new Emoji("6⃣"),
                    new Emoji("7⃣"),
                    new Emoji("8⃣"),
                    new Emoji("9⃣")
                };

                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"I found `{roles.Count.ToWords()}` roles that match this name. Please " +
                                  $"select the role that you want to delete, or use the ⛔ reaction " +
                                  $"to delete all roles with this name.",
                    Fields = new List<EmbedFieldBuilder>()
                };

                var callbacks = new List<(IEmote, Func<SocketCommandContext, SocketReaction, Task>)>();

                for (int i = 0; i < roles.Count; i++)
                {
                    int roleIndex = i + 1;
                    SocketRole role = roles.ElementAt(i);

                    embed.Fields.Add(new EmbedFieldBuilder
                    {
                        Name = $"Role #{roleIndex}",
                        Value = $"Exact Name: `{role.Name}`\n" +
                                $"Number of users who have this role: " +
                                $"`{Context.Guild.Users.Count(x => x.Roles.Contains(role))}`\n" +
                                $"Permissions: `{roles.Count}`\n" +
                                $"Created: `{role.CreatedAt.Humanize()}`\n" +
                                $"Position in role list (higher number = higher position): `{role.Position}`"
                    });

                    callbacks.Add((emojis[i], async (c, r) =>
                            {
                                await role.DeleteAsync();
                                await ReplyAsync($"{Context.User.Mention} `Successfully deleted Role #{roleIndex}`");
                            }
                        ));
                }

                callbacks.Add((new Emoji("⛔"), async (c, r) =>
                        {
                            foreach (SocketRole role in roles)
                                await role.DeleteAsync();

                            await ReplyAsync($"{Context.User.Mention} Successfully deleted `{roles.Count.ToWords()}` roles.");
                        }
                    ));

                var data = new ReactionCallbackData("", embed.Build(), false, false, TimeSpan.FromSeconds(120));
                data.SetCallbacks(callbacks);
                await InlineReactionReplyAsync(data);
            }
            else if (roles.Count == 1)
            {
                SocketRole role = roles.First();
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"{Context.User.Mention} Successfully deleted role `{role.Name}`"
                };

                await role.DeleteAsync();

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"I could not find the specified role."
                };

                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}