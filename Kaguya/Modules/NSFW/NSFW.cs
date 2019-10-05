#region NSFW commands...Proceed with caution!!
#endregion

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Kaguya.Core;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Threading.Tasks;

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

        [Command("", RunMode = RunMode.Async)]
        [RequireNsfw]
        public async Task NFSWRandom()
        {
            Random rand = new Random();
            var imageCollection = Global.stillsCollection;
            var userAccount = UserAccounts.GetAccount(Context.User);

            if (userAccount.NSFWAgeVerified == "false")
            {
                embed.WithTitle("NSFW Age Verification");
                embed.WithDescription($"{Context.User.Mention} You must be 18 years old to use the NSFW module. To acknowledge " +
                    $"that you are 18 years of age, please reply with \"I confirm.\" (without quotation marks).");

                var message = await ReplyAsync(embed: embed.Build());
                var response = await NextMessageAsync();

                if (!response.Content.ToLower().Contains("i confirm"))
                {
                    embed.WithDescription($"{Context.User.Mention} You did not reply with the proper response needed for verification. " +
                        $"Therefore, your NSFW status remains unverified, and this operation will be cancelled.");
                    await BE();
                    await message.DeleteAsync();
                    return;
                }
                else
                {
                    userAccount.NSFWAgeVerified = $"Successfully verified their age at {DateTime.Now}";
                    embed.WithDescription($"{Context.User.Mention} You have successfully verified that you are of age to view " +
                        $"this content.");
                    await BE();
                    await message.DeleteAsync();
                }
            }
            await Context.Channel.SendFileAsync(imageCollection[rand.Next(imageCollection.Length)]);
        }

        [Command("bomb", RunMode = RunMode.Async)]
        [RequireNsfw]
        public async Task NSFWBomb()
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            var difference = userAccount.NBombCooldownReset - DateTime.Now;
            var cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;
            bool isSupporter = userAccount.IsSupporter;
            Logger logger = new Logger();
            Random rand = new Random();

            if(userAccount.NSFWAgeVerified == "false")
            {
                embed.WithTitle("NSFW Age Verification");
                embed.WithDescription($"{Context.User.Mention} You must be 18 years old to use the NSFW module. To acknowledge " +
                    $"that you are 18 years of age, please reply with \"I confirm.\" (without quotation marks).");

                var message = await ReplyAsync(embed: embed.Build());
                var response = await NextMessageAsync();

                if (!response.Content.ToLower().Contains("i confirm"))
                {
                    embed.WithDescription($"{Context.User.Mention} You did not reply with the proper response needed for verification. " +
                        $"Therefore, your NSFW status remains unverified, and this operation will be cancelled.");
                    await BE();
                    await message.DeleteAsync();
                    return;
                }
                else
                {
                    userAccount.NSFWAgeVerified = $"Successfully verified their age at {DateTime.Now}";
                    embed.WithDescription($"{Context.User.Mention} You have successfully verified that you are of age to view " +
                        $"this content.");
                    await BE();
                    await message.DeleteAsync();
                }
            }

            if (difference.TotalSeconds < 0)
            {
                userAccount.NBombUsesThisHour = 5;
                userAccount.NBombCooldownReset = DateTime.Now + TimeSpan.FromMinutes(60);
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
            }

            for (int i = 0; i < 5; i++)
            {
                var imageCollection = Global.stillsCollection;
                await Context.Channel.SendFileAsync(imageCollection[rand.Next(imageCollection.Length)]);
            }
        }

        [Command("gif", RunMode = RunMode.Async)]
        [RequireNsfw]
        public async Task Gif()
        {
            Random rand = new Random();
            var imageCollection = Global.gifsCollection;
            var gif = imageCollection[rand.Next(imageCollection.Length)];
            var userAccount = UserAccounts.GetAccount(Context.User);

            if (userAccount.NSFWAgeVerified == "false")
            {
                embed.WithTitle("NSFW Age Verification");
                embed.WithDescription($"{Context.User.Mention} You must be 18 years old to use the NSFW module. To acknowledge " +
                    $"that you are 18 years of age, please reply with \"I confirm.\" (without quotation marks).");

                var message = await ReplyAsync(embed: embed.Build());
                var response = await NextMessageAsync();

                if (!response.Content.ToLower().Contains("i confirm"))
                {
                    embed.WithDescription($"{Context.User.Mention} You did not reply with the proper response needed for verification. " +
                        $"Therefore, your NSFW status remains unverified, and this operation will be cancelled.");
                    await BE();
                    await message.DeleteAsync();
                    return;
                }
                else
                {
                    userAccount.NSFWAgeVerified = $"Successfully verified their age at {DateTime.Now}";
                    embed.WithDescription($"{Context.User.Mention} You have successfully verified that you are of age to view " +
                        $"this content.");
                    await BE();
                    await message.DeleteAsync();
                }
            }

            await Context.Channel.SendFileAsync(gif);
        }
    }
}
