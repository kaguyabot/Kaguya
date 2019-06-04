#region NSFW commands...Proceed with caution!!
#endregion

using Discord.Commands;
using Kaguya.Core;
using NekosSharp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Kaguya.Core.Embed;
using EmbedType = Kaguya.Core.Embed.EmbedColor;
using Kaguya.Core.UserAccounts;
using Kaguya.Core.Server_Files;

namespace Kaguya.Modules.NSFW
{
    [Group("n")] //NSFW Commands...Proceed with caution!
    public class NSFW : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        NekoClient nekoClient = new NekoClient("Kaguya");

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command()]
        [RequireNsfw]
        public async Task NFSWRandom()
        {
            Random rand = new Random();
            var lewdNum = rand.Next(7, 117);
            var lewd = $"https://i.nhentai.net/galleries/1238853/{lewdNum}.png";
            embed.WithAuthor("NSFW: Hentai", $"{Context.User.GetAvatarUrl()}", $"{lewd}");
            embed.WithImageUrl(lewd);
            await BE();
        }

        [Command("bomb")]
        [RequireNsfw]
        public async Task NSFWBomb()
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            var difference = userAccount.NBombCooldownReset - DateTime.Now;
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            bool isSupporter = userAccount.IsSupporter;

            if(difference.TotalSeconds < 0)
            {
                userAccount.NBombUsesThisHour = 10;
                userAccount.NBombCooldownReset = DateTime.Now + TimeSpan.FromMinutes(60);
                UserAccounts.SaveAccounts();
            }

            if (!isSupporter && userAccount.NBombUsesThisHour <= 0)
            {
                embed.WithDescription($"{Context.User.Mention} You are out of `{cmdPrefix}n bomb` uses for this hour." +
                    $"\nTo reset the cooldown, use `{cmdPrefix}vote` followed by `{cmdPrefix}voteclaim`.");
                embed.WithFooter($"Supporters have no cooldown. For more information, use {cmdPrefix}supporter");
                await BE();
                return;
            }

            if (!isSupporter)
            {
                userAccount.NBombUsesThisHour -= 1;
                UserAccounts.SaveAccounts();
            }

            for (int i = 0; i < 5; i++)
            {
                var lewd = await nekoClient.Nsfw_v3.Hentai();
                embed.WithAuthor("NSFW: Hentai Bomb", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
                embed.WithImageUrl(lewd.ImageUrl);
                await BE();
            }

            if (!isSupporter && userAccount.NBombUsesThisHour == 2)
            {
                embed.WithDescription($"{Context.User.Mention} You only have 2 `{cmdPrefix}n bomb` uses left for this hour!");
                embed.SetColor(EmbedType.RED);
                await BE();
            }

        }

        [Command("lewd")]
        [RequireNsfw]
        public async Task LewdNeko()
        {
            var lewd = await nekoClient.Nsfw_v3.Lewd();

            embed.WithAuthor("NSFW: Lewd", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("boobs")]
        [RequireNsfw]
        public async Task Boobs()
        {
            var lewd = await nekoClient.Nsfw_v3.Boobs();

            embed.WithAuthor("NSFW: Boobs", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("anal")]
        [RequireNsfw]
        public async Task Anal()
        {
            var lewd = await nekoClient.Nsfw_v3.Anal();

            embed.WithAuthor("NSFW: Anal", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("bdsm")]
        [RequireNsfw]
        public async Task BDSM()
        {
            var lewd = await nekoClient.Nsfw_v3.Bdsm();

            embed.WithAuthor("NSFW: BDSM", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("bj")]
        [RequireNsfw]
        public async Task Blowjob()
        {
            var lewd = await nekoClient.Nsfw_v3.Blowjob();

            embed.WithAuthor("NSFW: Blowjob", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("classic")]
        [RequireNsfw]
        public async Task Classic()
        {
            var lewd = await nekoClient.Nsfw_v3.Classic();

            embed.WithAuthor("NSFW: Classic", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("cum")]
        [RequireNsfw]
        public async Task Cum()
        {
            var lewd = await nekoClient.Nsfw_v3.Cum();

            embed.WithAuthor("NSFW: Cum", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("feet")]
        [RequireNsfw]
        public async Task Feet()
        {
            var lewd = await nekoClient.Nsfw_v3.EroFeet();

            embed.WithAuthor("NSFW: Ero Feet", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("eroyuri")]
        [RequireNsfw]
        public async Task EroYuri()
        {
            var lewd = await nekoClient.Nsfw_v3.EroYuri();

            embed.WithAuthor("NSFW: Ero Yuri", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("pussy")]
        [RequireNsfw]
        public async Task Pussy()
        {
            var lewd = await nekoClient.Nsfw_v3.Pussy();

            embed.WithAuthor("NSFW: Pussy", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("solo")]
        [RequireNsfw]
        public async Task Solo()
        {
            var lewd = await nekoClient.Nsfw_v3.Solo();

            embed.WithAuthor("NSFW: Solo", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("hentai")]
        [RequireNsfw]
        public async Task Hentai()
        {
            var lewd = await nekoClient.Nsfw_v3.Hentai();

            embed.WithAuthor("NSFW: Hentai", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("avatar")]
        [RequireNsfw]
        public async Task KetaAvatar()
        {
            var lewd = await nekoClient.Nsfw_v3.KetaAvatar();

            embed.WithAuthor("NSFW: Avatar", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("trap")]
        [RequireNsfw]
        public async Task Trap()
        {
            var lewd = await nekoClient.Nsfw_v3.Trap();

            embed.WithAuthor("NSFW: Trap", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("yuri")]
        [RequireNsfw]
        public async Task Yuri()
        {
            var lewd = await nekoClient.Nsfw_v3.Yuri();

            embed.WithAuthor("NSFW: Yuri", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }

        [Command("gif")]
        [RequireNsfw]
        public async Task Gif()
        {
            var lewd = await nekoClient.Nsfw_v3.HentaiGif();

            embed.WithAuthor("NSFW: Hentai Gif", $"{Context.User.GetAvatarUrl()}", $"{lewd.ImageUrl}");
            embed.WithImageUrl(lewd.ImageUrl);
            await BE();
        }
    }
}
