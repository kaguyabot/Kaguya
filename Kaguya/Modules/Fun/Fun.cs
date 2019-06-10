using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Embed;
using NekosSharp;
using Discord.Addons.Interactive;
using EmbedType = Kaguya.Core.Embed.EmbedColor;
using System.Collections.Generic;

namespace Kaguya.Modules.Fun
{
    public class Fun : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        readonly NekoClient nekoClient = new NekoClient("Kaguya");

        public async Task BE()
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("fact")]
        public async Task RandomFact()
        {
            var factsFile = File.ReadAllLines("Resources/Facts.txt");
            Random rng = new Random();
            int rngNum = rng.Next(factsFile.Count());
            string fact = factsFile.ElementAt(rngNum);

            embed.WithTitle($"Random Fact #{rngNum}");
            embed.WithDescription(fact);
            await BE();
        }

        [Command("echo")] //fun
        public async Task Echo([Remainder]string message = "")
        {
            var filteredWords = Servers.GetServer(Context.Guild).FilteredWords;

            if (message == "")
            {
                embed.WithDescription($"**{Context.User.Mention} No message specified!**");
                await BE(); return;
            }

            foreach(var word in filteredWords)
            {
                if (message.Contains(word))
                    return;
            }

            embed.WithDescription(message);
            await BE();
        }

        [Command("pick")] //fun
        public async Task PickOne([Remainder]string message = "")
        {
            if (message == "")
            {
                embed.WithTitle("Pick: Missing Options!");
                embed.WithDescription($"**{Context.User.Mention} No options specified!**");
                await BE();
            }

            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            embed.WithTitle("Choice for " + Context.User.Username);
            embed.WithDescription(selection);

            await BE();
        }

        [Command("8ball")]
        public async Task EightBall([Remainder]string question)
        {
            string filePath = "Resources/8ball.txt";
            string[] responses = File.ReadAllLines(filePath);
            Random rand = new Random();

            var num = rand.Next(14);

            embed.WithTitle("Magic 8Ball");
            embed.WithDescription($"**{Context.User.Mention} {responses[num]}**");
            await BE();
            
        }

        //[Command("slap")]
        //public async Task Slap(string target)
        //{
        //    var gif = await nekoClient.Action_v3.SlapGif();
        //    embed.WithTitle($"{Context.User.Username} slaped {target}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("slap")]
        //public async Task Slap(IGuildUser target)
        //{
        //    var gif = await nekoClient.Action_v3.SlapGif();
        //    embed.WithTitle($"{Context.User.Username} slaped {target.Username}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("hug")]
        //public async Task Hug(string target)
        //{
        //    var gif = await nekoClient.Action_v3.HugGif();
        //    embed.WithTitle($"{Context.User.Username} hugged {target}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("hug")]
        //public async Task Hug(IGuildUser target)
        //{
        //    var gif = await nekoClient.Action_v3.HugGif();
        //    embed.WithTitle($"{Context.User.Username} hugged {target.Username}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("kiss")]
        //public async Task Kiss(string target)
        //{
        //    var gif = await nekoClient.Action_v3.KissGif();
        //    embed.WithTitle($"{Context.User.Username} kissed {target}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("kiss")]
        //public async Task Kiss(IGuildUser target)
        //{
        //    var gif = await nekoClient.Action_v3.KissGif();
        //    embed.WithTitle($"{Context.User.Username} kissed {target.Username}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("pat")]
        //public async Task Pat(string target)
        //{
        //    var gif = await nekoClient.Action_v3.PatGif();
        //    embed.WithTitle($"{Context.User.Username} patted {target}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("pat")]
        //public async Task Pat(IGuildUser target)
        //{
        //    var gif = await nekoClient.Action_v3.PatGif();
        //    embed.WithTitle($"{Context.User.Username} patted {target.Username}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("poke")]
        //public async Task Poke(string target)
        //{
        //    var gif = await nekoClient.Action_v3.PokeGif();
        //    embed.WithTitle($"{Context.User.Username} poked {target}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("poke")]
        //public async Task Poke(IGuildUser target)
        //{
        //    var gif = await nekoClient.Action_v3.PokeGif();
        //    embed.WithTitle($"{Context.User.Username} poked {target.Username}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("tickle")]
        //public async Task Tickle(string target)
        //{
        //    var gif = await nekoClient.Action_v3.TickleGif();
        //    embed.WithTitle($"{Context.User.Username} tickled {target}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("tickle")]
        //public async Task Tickle(IGuildUser target)
        //{
        //    var gif = await nekoClient.Action_v3.TickleGif();
        //    embed.WithTitle($"{Context.User.Username} tickled {target.Username}!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("baka")]
        //public async Task Baka()
        //{
        //    var gif = await nekoClient.Image_v3.BakaGif();
        //    embed.WithTitle($"Baka!!");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("nekoavatar")]
        //public async Task NekoAvatar()
        //{
        //    var gif = await nekoClient.Image_v3.NekoAvatar();
        //    embed.WithTitle($"Neko Avatar for {Context.User.Username}");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("smug")]
        //public async Task Smug()
        //{
        //    var gif = await nekoClient.Image_v3.SmugGif();
        //    embed.WithTitle($"Smug（￣＾￣）");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("waifu")]
        //public async Task Waifu()
        //{
        //    var gif = await nekoClient.Image_v3.Waifu();
        //    embed.WithTitle($"Waifu (ﾉ≧ڡ≦)");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        //[Command("wallpaper")]
        //public async Task Wallpaper()
        //{
        //    var gif = await nekoClient.Image_v3.Wallpaper();
        //    embed.WithTitle($"Wallpaper for {Context.User.Username}");
        //    embed.WithImageUrl(gif.ImageUrl);
        //    await BE();
        //}

        private bool UserIsAdmin(SocketGuildUser user)
        {
            string targetRoleName = "Administrator";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
    }
}
