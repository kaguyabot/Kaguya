using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AssignAllRoles : KaguyaBase
    {
        [PremiumServerCommand]
        [Command("AssignAllRoles", RunMode = RunMode.Async)]
        [Alias("aaroles", "aa")]
        [Summary("Assigns every single role in the server to a user, or list of users.")]
        [Remarks("<user> {...}")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command(params SocketGuildUser[] users)
        {
            if (users.Length == 0)
            {
                var errorEmbed = new KaguyaEmbedBuilder
                {
                    Description = "Please specify one (or more) users to action."
                };

                await ReplyAsync(embed: errorEmbed.Build());
                return;
            }

            await ReplyAsync($"{Context.User.Mention} Processing, please wait...");

            var roles = Context.Guild.Roles.Where(x => !x.IsManaged && x.Name != "@everyone").ToList();
            var namesList = new List<string>(users.Length);

            int errorRoles = Context.Guild.Roles.Count(x => x.IsManaged);
            foreach (var user in users)
            {
                namesList.Add(user.ToString());
                await user.AddRolesAsync(roles);
            }

            var nameString = $"";
            foreach (var name in namesList)
            {
                nameString += $"`{name}`, ";
            }

            nameString = nameString.Substring(0, nameString.Length - 2);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully applied `{(Context.Guild.Roles.Count - errorRoles).ToWords()}` roles to " +
                              $"{nameString}."
            };

            if (errorRoles > 0)
            {
                embed.Description += $"\n\nI failed to assign `{errorRoles.ToWords()}` roles. These " +
                                     $"roles are managed by integrations or other bots, therefore they " +
                                     $"cannot be assigned to any users.";
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
