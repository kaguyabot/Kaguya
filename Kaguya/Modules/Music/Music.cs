using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.UserAccounts;
using System.Net;
using System.Timers;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using Kaguya.Core;
using System.Diagnostics;

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
            => await ReplyAsync("", false, await musicService.ListAsync(Context.Guild.Id));
    }
}
