using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class DeleteVoiceChannel : KaguyaBase
    {
        [UtilityCommand]
        [Command("DeleteVoiceChannel")]
        [Alias("dvc")]
        [Summary("Deletes a standard voice channel. The name does not need to be an exact match of the voice " +
            "channel's name. This command will delete the voice channel that matches closest to what you wrote. " +
            "For example, if I have a voice channel named `Music-128KBPS`, I could write " +
            "`dvc Music-128`.")]
        [Remarks("<name>")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder] string name)
        {
            SocketVoiceChannel vc = Context.Guild.VoiceChannels.First(x => x.Name.ToLower().Contains(name.ToLower()));
            if (vc != null)
                await SendBasicSuccessEmbedAsync($"Successfully deleted the voice channel `{vc.Name}`");
            else
            {
                await SendBasicErrorEmbedAsync($"I couldn't find a voice channel that contains `{name}` in " +
                                               "it's name.");
            }
        }
    }
}