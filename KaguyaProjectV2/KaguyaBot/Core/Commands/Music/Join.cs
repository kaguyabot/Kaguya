using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable PossibleNullReferenceException

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Join : KaguyaBase
    {
        [MusicCommand]
        [Command("Join")]
        [Summary("Joins Kaguya into the voice channel you are currently in. This step is optional, and may " +
                 "be skipped with the `play` command. You may also specify the name (or part of a name) of a voice " +
                 "channel that you want Kaguya to join instead of the voice channel you currently are in. If Kaguya " +
                 "is already in one voice channel, it will move to the specified channel.")]
        [Remarks("[channel]")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder]string vcMatch = null)
        {
            var node = ConfigProperties.LavaNode;
            var botCurVc = Context.Guild.CurrentUser.VoiceChannel;
            var vc = vcMatch == null 
                ? (Context.User as SocketGuildUser).VoiceChannel 
                : Context.Guild.VoiceChannels.First(x => x.Name.ToLower().Contains(vcMatch.ToLower()));

            if (node.HasPlayer(Context.Guild) && vc != null)
            {
                await node.MoveChannelAsync(vc);
                await SendBasicSuccessEmbedAsync(
                    $"{Context.User.Mention} Successfully moved to `{vc.Name}`.");
            }
            else
            {
                if (vc == botCurVc)
                {
                    await SendBasicErrorEmbedAsync($"I am already connected to this voice channel.");
                    return;
                }
                await node.JoinAsync(vc);
                await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Successfully joined `{vc.Name}`.");
            }
        }
    }
}
