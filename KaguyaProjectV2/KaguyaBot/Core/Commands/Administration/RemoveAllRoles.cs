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
    public class RemoveAllRoles : KaguyaBase
    {
        [AdminCommand]
        [Command("RemoveAllRoles")]
        [Alias("rar")]
        [Summary("Removes all roles from a user, or a list of users.")]
        [Remarks("<user> {...}")]
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
                await user.RemoveRolesAsync(roles);
            }

            var nameString = $"";
            foreach (var name in namesList)
            {
                nameString += $"`{name}`, ";
            }

            nameString = nameString.Substring(0, nameString.Length - 2);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully removed `{(Context.Guild.Roles.Count - errorRoles).ToWords()}` roles from " +
                              $"{nameString}."
            };

            if (errorRoles > 0)
            {
                embed.Description += $"\n\nI failed to remove `{errorRoles.ToWords()}` roles. These " +
                                     $"roles are managed by integrations or other bots, therefore they " +
                                     $"cannot be assigned to any users.";
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
