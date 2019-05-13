using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Core;
using System.Diagnostics;
using NekosSharp;
using System;

namespace Kaguya.Modules.NSFW
{
    [Group("n")]
    public class NSFW : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        Color Violet = new Color(238, 130, 238);
        Logger logger = new Logger();
        Stopwatch stopWatch = new Stopwatch();
        NekoClient nekoClient = new NekoClient("Kaguya");

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command()]
        [RequireNsfw]
        public async Task NFSWRandom()
        {
            stopWatch.Start();
            Random rand = new Random();
            var lewdNum = rand.Next(7, 117);
            var lewd = $"https://i.nhentai.net/galleries/1238853/{lewdNum}.png";
            embed.WithAuthor("NSFW: Hentai", $"{Context.User.GetAvatarUrl()}", $"{lewd}");
            embed.WithImageUrl(lewd);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("bomb")]
        [RequireNsfw]
        public async Task NSFWBomb()
        {
            stopWatch.Start();

            for (int i = 0; i < 5; i++)
            {
                var lewd = await nekoClient.Nsfw_v3.Hentai();
                embed.WithAuthor("NSFW: Hentai Bomb", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
                embed.WithImageUrl(lewd.ImageUrl);
                embed.WithColor(Violet);
                await BE();
            }
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, "Hentai Bomb Executed.");
        }

        [Command("lewd")]
        [RequireNsfw]
        public async Task LewdNeko()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Lewd();

            embed.WithAuthor("NSFW: Lewd", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("boobs")]
        [RequireNsfw]
        public async Task Boobs()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Boobs();

            embed.WithAuthor("NSFW: Boobs", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("anal")]
        [RequireNsfw]
        public async Task Anal()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Anal();

            embed.WithAuthor("NSFW: Anal", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("bdsm")]
        [RequireNsfw]
        public async Task BDSM()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Bdsm();

            embed.WithAuthor("NSFW: BDSM", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("bj")]
        [RequireNsfw]
        public async Task Blowjob()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Blowjob();

            embed.WithAuthor("NSFW: Blowjob", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("classic")]
        [RequireNsfw]
        public async Task Classic()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Classic();

            embed.WithAuthor("NSFW: Classic", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("cum")]
        [RequireNsfw]
        public async Task Cum()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Cum();

            embed.WithAuthor("NSFW: Cum", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("feet")]
        [RequireNsfw]
        public async Task Feet()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.EroFeet();

            embed.WithAuthor("NSFW: Ero Feet", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("eroyuri")]
        [RequireNsfw]
        public async Task EroYuri()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.EroYuri();

            embed.WithAuthor("NSFW: Ero Yuri", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("pussy")]
        [RequireNsfw]
        public async Task Pussy()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Pussy();

            embed.WithAuthor("NSFW: Pussy", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("solo")]
        [RequireNsfw]
        public async Task Solo()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Solo();

            embed.WithAuthor("NSFW: Solo", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("hentai")]
        [RequireNsfw]
        public async Task Hentai()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Hentai();

            embed.WithAuthor("NSFW: Hentai", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("avatar")]
        [RequireNsfw]
        public async Task KetaAvatar()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.KetaAvatar();

            embed.WithAuthor("NSFW: Avatar", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("trap")]
        [RequireNsfw]
        public async Task Trap()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Trap();

            embed.WithAuthor("NSFW: Trap", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("yuri")]
        [RequireNsfw]
        public async Task Yuri()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.Yuri();

            embed.WithAuthor("NSFW: Yuri", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("gif")]
        [RequireNsfw]
        public async Task Gif()
        {
            stopWatch.Start();
            var lewd = await nekoClient.Nsfw_v3.HentaiGif();

            embed.WithAuthor("NSFW: Hentai Gif", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }
    }
}
