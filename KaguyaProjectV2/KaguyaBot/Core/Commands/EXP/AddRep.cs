using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable PossibleNullReferenceException
namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class AddRep : KaguyaBase
    {
        [ExpCommand]
        [Command("Rep")]
        [Summary("Allows a user to add one rep point to another user in the server. " +
                 "This action may only be done once every 24 hours. Use without an " +
                 "argument to view your own rep.\n\n" +
                 "This rep is stored in your account and is carried with you globally. " +
                 "The `praise` command is server-specific.")]
        [Remarks("\n<user>\n<user> <reason>")]
        public async Task Command(IGuildUser guildUser = null, [Remainder] string reason = null)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            List<Rep> rep = await DatabaseQueries.GetAllForUserAsync<Rep>(user.UserId);
            int repCount = rep.Count;

            if (guildUser == null)
            {
                var curRepEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You have `{repCount}` rep."
                };

                await ReplyAsync(embed: curRepEmbed.Build());

                return;
            }

            if (!user.CanGiveRep)
            {
                var denyEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"{Context.User.Mention} you must wait " +
                                  $"`{(DateTime.FromOADate(user.LastGivenRep) - DateTime.Now.AddHours(-24)).Humanize()}` " +
                                  $"before giving rep again."
                };

                denyEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: denyEmbed.Build());

                return;
            }

            if (guildUser == Context.User)
            {
                var invalidUserEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You may not rep yourself!"
                };

                invalidUserEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: invalidUserEmbed.Build());

                return;
            }

            if (guildUser.IsBot)
            {
                var invalidUserEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"Sorry, rep can't be given to bots."
                };

                invalidUserEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: invalidUserEmbed.Build());

                return;
            }

            User target = await DatabaseQueries.GetOrCreateUserAsync(guildUser.Id);

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

            var newTargetRep = new Rep
            {
                UserId = guildUser.Id,
                GivenBy = user.UserId,
                TimeGiven = DateTime.Now.ToOADate(),
                Reason = reason ?? "No reason provided."
            };

            await DatabaseQueries.InsertAsync(newTargetRep);
            user.LastGivenRep = DateTime.Now.ToOADate();

            await DatabaseQueries.UpdateAsync(user);

            List<Rep> targetRepList = await DatabaseQueries.GetAllForUserAsync<Rep>(target.UserId);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully added one rep to `{guildUser}`! \nYou may give rep again in `24 hours`.",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{guildUser.Username} now has {targetRepList.Count} rep. You have {repCount} rep."
                }
            };

            embed.SetColor(EmbedColor.GOLD);

            await ReplyAsync(embed: embed.Build());
        }
    }
}