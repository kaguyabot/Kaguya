using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class ReverseReactionRole : KaguyaBase
    {
        //[AdminCommand]
        [Command("ReverseReactionRole")]
        [Alias("")]
        [Summary("Allows a server administrator to essentially undo all role changes made to users " +
                 "who used a Kaguya Reaction Role on the provided message ID. This will remove whatever " +
                 "role was linked to the emote from **all users** who clicked on the reaction, even if the " +
                 "role was already given to the user before they used the reaction role.")]
        [Remarks("<message ID>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Command(ulong messageId)
        {
            //todo: Write this out, link IEnumerable<ReactionRole> to Server object.
        }
    }
}