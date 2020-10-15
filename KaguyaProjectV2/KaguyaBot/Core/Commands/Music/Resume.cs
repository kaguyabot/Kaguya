using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using Victoria.Enums;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Resume : KaguyaBase
    {
        [DisabledCommand]
        [MusicCommand]
        [Command("Resume")]
        [Summary("Resumes the music player if it was previously paused.")]
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

            if (player.PlayerState == PlayerState.Paused)
            {
                await player.ResumeAsync();
                await SendBasicSuccessEmbedAsync($"Successfully resumed the player.");
            }
            else
            {
                await SendBasicErrorEmbedAsync($"The player is already actively playing.");
            }
        }
    }
}
