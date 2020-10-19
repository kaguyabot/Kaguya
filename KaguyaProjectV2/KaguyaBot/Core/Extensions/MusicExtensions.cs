using Victoria;
using Victoria.Enums;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class MusicExtensions
    {
        public static bool IsPlaying(this LavaPlayer player) => player.PlayerState == PlayerState.Playing;
        public static bool IsPaused(this LavaPlayer player) => player.PlayerState == PlayerState.Paused;
    }
}