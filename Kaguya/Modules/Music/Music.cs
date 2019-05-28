using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Commands;
using System.Threading.Tasks;

namespace Kaguya.Modules.Music
{
    [Group("m")]
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly MusicService musicService = new MusicService();

        [Command("join")]
        public async Task MusicJoin()
            => await ReplyAsync("", false, await musicService.JoinOrPlayAsync((SocketGuildUser)Context.User, Context.Channel, Context.Guild.Id));

        [Command("play")]
        public async Task MusicPlay([Remainder]string search) 
            => await ReplyAsync("", false, await musicService.JoinOrPlayAsync((SocketGuildUser)Context.User, Context.Channel, Context.Guild.Id, search));

        [Command("leave")]
        public async Task MusicLeave()
            => await ReplyAsync("", false, await musicService.LeaveAsync((SocketGuildUser)Context.User, Context.Guild.Id));

        [Command("queue")]
        public async Task MusicQueue()
            => await ReplyAsync("", false, await musicService.ListAsync(Context.Guild.Id, Context.Guild.Name));

        [Command("skip")]
        public async Task SkipTrack()
            => await ReplyAsync("", false, await musicService.SkipTrackAsync(Context.Guild.Id, Context.Guild.Name));

        [Command("volume")]
        public async Task Volume(int volume)
            => await ReplyAsync("", false, await musicService.VolumeAsync(Context.Guild.Id, volume));

        [Command("Pause")]
        public async Task Pause()
           => await ReplyAsync("", false, await musicService.Pause(Context.Guild.Id));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync("", false, await musicService.Pause(Context.Guild.Id));

        [Command("Jump")]
        public async Task Jump(int jumpNum)
            => await ReplyAsync("", false, await musicService.Jump(Context.Guild.Id, jumpNum));
    }
}
