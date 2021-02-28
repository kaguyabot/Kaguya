using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Music;
using Victoria;
using Victoria.Enums;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("pause")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Pause : KaguyaBase<Pause>
    {
        private readonly ILogger<Pause> _logger;
        private readonly LavaNode _lavaNode;
        public Pause(ILogger<Pause> logger, LavaNode lavaNode) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
        }

        [Command]
        [Summary("Toggles whether this player is currently paused. Use while songs are playing to " +
                 "pause the player, use while a player is paused to resume it.")]
        public async Task PauseCommand()
        {
            if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
            {
                await SendBasicErrorEmbedAsync("Failed to join voice channel. Are you in a voice channel?");

                return;
            }

            LavaPlayer player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Paused)
            {
                await player.ResumeAsync();
                await SendBasicSuccessEmbedAsync("Resumed the player.");
            }
            else if (player.PlayerState == PlayerState.Playing)
            {
                await player.PauseAsync();
                await SendBasicSuccessEmbedAsync("Paused the player.");
            }
            else
            {
                await SendBasicErrorEmbedAsync("The player must be either in a playing or paused state to use this command.\n" +
                                               $"Current state is {player.PlayerState.Humanize().AsBold()}.");
            }
        }
    }
}