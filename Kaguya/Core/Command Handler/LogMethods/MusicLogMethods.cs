using System;
using System.Threading.Tasks;
using Discord;
using Victoria;
using Victoria.Entities;

namespace Kaguya.Core.Command_Handler.LogMethods
{
    public class MusicLogMethods
    {
        readonly public IServiceProvider _services;
        readonly Logger logger = new Logger();
        readonly EmbedBuilder embed = new EmbedBuilder();
        readonly Color Pink = new Color(252, 132, 255);

        public Task MusicLogger(LogMessage msg)
        {
            logger.ConsoleMusicLog(msg);
            return Task.CompletedTask;
        }

        public Task OnTrackException(LavaPlayer lavaPlayer, LavaTrack lavaTrack, string errorMsg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Music Service Exception:" +
                $"\n" +
                $"\n[Guild: {lavaPlayer.VoiceChannel.Guild.Name} | {lavaPlayer.VoiceChannel.GuildId}]" +
                $"\n[Error: \"{errorMsg}\" for {lavaTrack.Title}]");
            return Task.CompletedTask;
        }

        public async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext())
                return;

            if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await player.TextChannel?.SendMessageAsync($"There are no more songs left in queue.");
                return;
            }

            await player.PlayAsync(nextTrack);

            embed.WithDescription($"**Finished Playing: `{track.Title}`\nNow Playing: `{nextTrack.Title}`**");
            embed.WithColor(Pink);
            await player.TextChannel.SendMessageAsync("", false, embed.Build());
            await player.TextChannel.SendMessageAsync(player.ToString());
        }

        private Task OnTrackStuck(LavaPlayer player, LavaTrack track, long threshold)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Music Service Stuck:" +
                $"\n" +
                $"\n[Guild: {player.VoiceChannel.Guild.Name} | {player.VoiceChannel.GuildId}]" +
                $"\n[Track: {track.Title} stuck after {threshold}ms]");
            return Task.CompletedTask;
        }
    }
}
