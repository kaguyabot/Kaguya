using Discord;
using Kaguya.Discord;
using Kaguya.Discord.DiscordExtensions;
using Victoria;

namespace Kaguya.Internal.Music
{
    public static class MusicEmbeds
    {
        public static Embed GetNowPlayingEmbedForTrack(LavaTrack track, bool autoPlay = false)
        {
            string title = autoPlay ? "Now playing (auto-play):" : "Now playing";
            return new KaguyaEmbedBuilder(Color.Blue)
                   .WithDescription($"🎵 {title}:\n" +
                                    $"Title: {track.Title.AsBold()}\n" +
                                    $"Duration: {track.Duration.HumanizeTraditionalReadable().AsBold()}")
                   .Build();
        }
        
        public static Embed GetQueuedEmbedForTrack(LavaTrack track, int queueSize)
        {
            return new KaguyaEmbedBuilder(Color.Purple)
                   .WithDescription($"⏳ Queued:\n" +
                                    $"Title: {track.Title.AsBold()}\n" +
                                    $"Duration: {track.Duration.HumanizeTraditionalReadable().AsBold()}\n" +
                                    $"Queue Position: {queueSize.ToString().AsBold()}.")
                   .Build();
        }
    }
}