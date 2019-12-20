using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Humanizer;
// ReSharper disable PossibleNullReferenceException

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class GlobalRep : ModuleBase<ShardedCommandContext>
    {
        [ExpCommand]
        [Command("GlobalRep")]
        [Alias("grep")]
        [Summary("Allows a user to add one rep point to another user in the server. " +
                 "This action may only be done once every 24 hours. Use without an " +
                 "argument to view your own rep.\n\n" +
                 "This rep is stored in your account and is carried with you globally. " +
                 "The `rep` command is server-specific.")]
        [Remarks("\n<user>")]
        public async Task Command(IGuildUser user = null)
        {
            User curUser = await UserQueries.GetOrCreateUser(Context.User.Id);
            
            if (user == null)
            {
                var curRepEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You have `{curUser.Rep}` rep."
                };
                await ReplyAsync(embed: curRepEmbed.Build());
                return;
            }

            if (!curUser.CanGiveRep)
            {
                var denyEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"{Context.User.Mention} you must wait " +
                                  $"`{(DateTime.FromOADate(curUser.LastGivenRep) - DateTime.Now.AddHours(-24)).Humanize()}` " +
                                  $"before giving rep again."
                };
                denyEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: denyEmbed.Build());
                return;
            }

            if (user == Context.User)
            {
                var invalidUserEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You may not rep yourself!"
                };
                invalidUserEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: invalidUserEmbed.Build());
                return;
            }

            if (user.IsBot)
            {
                var invalidUserEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"Sorry, rep can't be given to bots."
                };
                invalidUserEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: invalidUserEmbed.Build());
                return;
            }


            User target = await UserQueries.GetOrCreateUser(user.Id);

            if (target.IsBlacklisted)
            {
                var invalidUserEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"Sorry, rep can't be given to blacklisted users. " +
                                  $"This person may appeal their blacklist by filling " +
                                  $"out [this form](https://forms.gle/bnLFWbNiEyF4uE9E9)"
                };
                invalidUserEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: invalidUserEmbed.Build());
                return;
            }

            target.Rep += 1;
            curUser.LastGivenRep = DateTime.Now.ToOADate();

            await UserQueries.UpdateUser(curUser);
            await UserQueries.UpdateUser(target);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully added one rep to `{user}`! \nYou may give rep again in `24 hours`.",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{user.Username} now has {target.Rep} rep. You have {curUser.Rep} rep."
                }
            };
            embed.SetColor(EmbedColor.GOLD);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
