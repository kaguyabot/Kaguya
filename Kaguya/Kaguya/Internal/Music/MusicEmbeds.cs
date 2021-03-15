using Discord;
using Kaguya.Discord;
using Kaguya.Internal.Extensions.DiscordExtensions;
using System.Text;
using Victoria;

namespace Kaguya.Internal.Music
{
	public static class MusicEmbeds
	{
		public static Embed GetNowPlayingEmbedForTrack(LavaTrack track, bool autoPlay = false)
		{
			string title = autoPlay ? "Now playing (auto-play)" : "Now playing";
			return new KaguyaEmbedBuilder(Color.Blue).WithDescription($"🎵 {title}:\n" +
			                                                          $"Title: {track.Title.AsBold()}\n" +
			                                                          $"Duration: {track.Duration.HumanizeTraditionalReadable().AsBold()}")
			                                         .Build();
		}

		public static Embed GetQueuedEmbedForTrack(LavaTrack track, int queueSize)
		{
			return new KaguyaEmbedBuilder(Color.Purple).WithDescription("⏳ Queued:\n" +
			                                                            $"Title: {track.Title.AsBold()}\n" +
			                                                            $"Duration: {track.Duration.HumanizeTraditionalReadable().AsBold()}\n" +
			                                                            $"Queue Position: {queueSize.ToString().AsBold()}.")
			                                           .Build();
		}

		public static Embed GetQueueEmbed(DefaultQueue<LavaTrack> queue, LavaTrack currentlyPlayingTrack)
		{
			var descSb = new StringBuilder();

			if (currentlyPlayingTrack != null)
			{
				string trackDurCur, trackDurTotal;

				if (currentlyPlayingTrack.Duration.TotalMinutes > 60)
				{
					trackDurCur = currentlyPlayingTrack.Position.ToString("hh\\:mm\\:ss");
					trackDurTotal = currentlyPlayingTrack.Duration.ToString("hh\\:mm\\:ss");
				}
				else
				{
					trackDurCur = currentlyPlayingTrack.Position.ToString("mm\\:ss");
					trackDurTotal = currentlyPlayingTrack.Duration.ToString("mm\\:ss");
				}

				descSb.AppendLine("Now Playing:\n".AsBold() +
				                  $"Title: {currentlyPlayingTrack.Title.AsBold()} " +
				                  $"[{trackDurCur} / {trackDurTotal}]\n\n" +
				                  "Up Next ⬇️");
			}

			if (queue.Count == 0)
			{
				descSb.AppendLine("No tracks enqueued.".AsItalics());
			}

			int dispCount = 1;
			foreach (var track in queue)
			{
				descSb.AppendLine($"#{dispCount}. {track.Title.AsBold()} - {track.Author} ({track.Duration:mm\\:ss})");
				dispCount++;
			}

			return new KaguyaEmbedBuilder(Color.DarkTeal).WithTitle("🗒️ Kaguya Music Queue").WithDescription(descSb.ToString()).Build();
		}
	}
}