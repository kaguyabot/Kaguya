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
using System.IO;
using System.Linq;
using Discord.Addons.Interactive;
using Discord;

namespace Kaguya.Modules.NSFW
{
    [Group("n")] //NSFW Commands...Proceed with caution!
    public class NSFW : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command()]
        [RequireNsfw]
        public async Task NFSWRandom()
        {
            Random rand = new Random();
            var imageCollection = Global.stillsCollection;
            await Context.Channel.SendFileAsync(imageCollection[rand.Next(imageCollection.Length)]);
        }

        [Command("bomb", RunMode = RunMode.Async)]
        [RequireNsfw]
        public async Task NSFWBomb()
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            var difference = userAccount.NBombCooldownReset - DateTime.Now;
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            bool isSupporter = userAccount.IsSupporter;
            Logger logger = new Logger();
            Random rand = new Random();

            if (difference.TotalSeconds < 0)
            {
                userAccount.NBombUsesThisHour = 5;
                userAccount.NBombCooldownReset = DateTime.Now + TimeSpan.FromMinutes(60);
                UserAccounts.SaveAccounts();
            }

            if(!isSupporter && userAccount.NBombUsesThisHour <= 0 && userAccount.Diamonds >= 50)
            {
                embed.WithDescription($"{Context.User.Mention} You are out of `{cmdPrefix}n bomb` uses for this hour." +
                    $"\nHowever, I see that you have more than 50<a:KaguyaDiamonds:581562698228301876> in your account. Would " +
                    $"you like to use 50<a:KaguyaDiamonds:581562698228301876> and reset your cooldown? (You have 10 seconds to respond)");
                embed.WithFooter($"Current Balance: {userAccount.Diamonds} Diamonds");

                Emoji[] reactions = { new Emoji("✅"), new Emoji("❌") };

                ReactionCallbackData data = new ReactionCallbackData("", embed.Build(), timeout: TimeSpan.FromSeconds(10));

                var reactionCallback = await Interactive.SendMessageWithReactionCallbacksAsync(Context, data);
                await reactionCallback.AddReactionsAsync(reactions);

                await Task.Delay(10000); //Wait 10 seconds before processing the reactions.

                var reactors = await reactionCallback.GetReactionUsersAsync(reactions[0], 30).FlattenAsync();

                if (reactors.Contains(Context.User))
                {
                    userAccount.NBombUsesThisHour = 5;
                    userAccount.Diamonds -= 50;
                    UserAccounts.SaveAccounts();
                    embed.WithDescription($"{Context.User.Mention} I have deducted 50<a:KaguyaDiamonds:581562698228301876> from your " +
                        $"account and your `{cmdPrefix}n bomb` cooldown has been reset.");
                    embed.SetColor(EmbedType.BLUE);
                    await BE();
                    return;
                }
                else
                    await reactionCallback.DeleteAsync();
            }

            if (!isSupporter && userAccount.NBombUsesThisHour <= 0)
            {
                logger.ConsoleStatusAdvisory($"User {Context.User.Username} is out of \"$n bomb\" uses for this hour.");
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
                var imageCollection = Global.stillsCollection;
                await Context.Channel.SendFileAsync(imageCollection[rand.Next(imageCollection.Length)]);
            }
        }

        [Command("gif")]
        [RequireNsfw]
        public async Task Gif()
        {
            Random rand = new Random();
            var imageCollection = Global.gifsCollection;
            var gif = imageCollection[rand.Next(imageCollection.Length)];
            await Context.Channel.SendFileAsync(gif);
        }

        //[Command("lewd")]
        //[RequireNsfw]
        //public async Task LewdNeko()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("boobs")]
        //[RequireNsfw]
        //public async Task Boobs()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("anal")]
        //[RequireNsfw]
        //public async Task Anal()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("bdsm")]
        //[RequireNsfw]
        //public async Task BDSM()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("bj")]
        //[RequireNsfw]
        //public async Task Blowjob()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("classic")]
        //[RequireNsfw]
        //public async Task Classic()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("cum")]
        //[RequireNsfw]
        //public async Task Cum()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("feet")]
        //[RequireNsfw]
        //public async Task Feet()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("eroyuri")]
        //[RequireNsfw]
        //public async Task EroYuri()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("pussy")]
        //[RequireNsfw]
        //public async Task Pussy()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("solo")]
        //[RequireNsfw]
        //public async Task Solo()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("hentai")]
        //[RequireNsfw]
        //public async Task Hentai()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("avatar")]
        //[RequireNsfw]
        //public async Task KetaAvatar()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("trap")]
        //[RequireNsfw]
        //public async Task Trap()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}

        //[Command("yuri")]
        //[RequireNsfw]
        //public async Task Yuri()
        //{
        //    string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
        //    embed.WithDescription($"All tags have temporarily been disabled to ensure compliance with Discord's Terms of Service. " +
        //        $"Please use `{cmdPrefix}n`, `{cmdPrefix}n bomb`, or `{cmdPrefix}n gif` for the time being. These images are guaranteed to be TOS compliant.");
        //    embed.WithFooter("We apologize for any inconvenience. We are working hard to return all tags as soon as possible.");
        //    await BE();
        //}


    }
}
