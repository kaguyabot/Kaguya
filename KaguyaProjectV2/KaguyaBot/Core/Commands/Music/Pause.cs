using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using Victoria.Enums;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Pause : KaguyaBase
    {
        [MusicCommand]
        [Command("Pause")]
        [Summary("Pauses the music player if it is playing.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var node = ConfigProperties.LavaNode;
            var player = node.GetPlayer(Context.Guild);

            if (player == null)
            {
                await SendBasicErrorEmbedAsync($"There needs to be an active music player in the " +
                                               $"server for this command to work. Start one " +
                                               $"by using `{server.CommandPrefix}play <song>`!");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                await player.PauseAsync();
                await SendBasicSuccessEmbedAsync($"Successfully paused the player.");
            }
            else
            {
                await SendBasicErrorEmbedAsync($"There is no song currently playing, therefore I have nothing to pause.");
            }
        }
    }
}
