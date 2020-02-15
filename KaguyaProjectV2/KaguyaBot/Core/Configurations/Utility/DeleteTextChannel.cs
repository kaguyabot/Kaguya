using System.Linq;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class DeleteTextChannel : KaguyaBase
    {
        [UtilityCommand]
        [Command("DeleteTextChannel")]
        [Alias("ctc")]
        [Summary("Deletes a standard text channel.")]
        [Remarks("<name>")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task Command(SocketTextChannel textChannel)
        {
            if (textChannel != null)
            {
                await textChannel.DeleteAsync();
                await SendBasicSuccessEmbedAsync($"Successfully deleted `{textChannel.Name}`.");
            }
            else
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} I couldn't find a text channel " +
                                               $"that matched the input you gave me.");
            }
            
        }
    }
}