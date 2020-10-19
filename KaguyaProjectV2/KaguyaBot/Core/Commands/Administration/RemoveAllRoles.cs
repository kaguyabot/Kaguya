using Discord.Commands;
using Discord.WebSocket;
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

            List<SocketRole> roles = Context.Guild.Roles.Where(x => !x.IsManaged && x.Name != "@everyone").ToList();
            var namesList = new List<string>(users.Length);

            int errorRoles = Context.Guild.Roles.Count(x => x.IsManaged);
            foreach (SocketGuildUser user in users)
            {
                namesList.Add(user.ToString());
                await user.RemoveRolesAsync(roles);
            }

            string nameString = $"";
            foreach (string name in namesList)
                nameString += $"`{name}`, ";

            nameString = nameString.Substring(0, nameString.Length - 2);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully removed all roles from " +
                              $"{nameString}. Note that if there are any roles " +
                              $"managed by integrations or bots, these may not be removed."
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}