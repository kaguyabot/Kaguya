﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class SomeCommand : InteractiveBase<ShardedCommandContext>
    {
        [ExpCommand]
        [Command("Praise")]
        [Summary("Allows a user to `praise` another user in the server once every `24 hours`. You " +
                 "may praise a user with an optional reason. Server admins may change the cooldown " +
                 "using the `praiseconfigure` command.")]
        [Remarks("\n<user>\n<user> <reason>")]
        public async Task Command(IGuildUser user = null, [Remainder]string reason = null)
        {
            var server = await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var userPraise = await ServerQueries.GetPraiseAsync(Context.User.Id, server.Id);
            var lastGivenPraise = await ServerQueries.GetLastPraiseTimeAsync(Context.User.Id, Context.Guild.Id);

            if (user == null)
            {
                var curEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You currently have `{userPraise.Count}` praise.",
                };

                await Context.Channel.SendMessageAsync(embed: curEmbed.Build());
                return;
            }

            if (user == Context.User)
            {
                var userErrorEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You can't praise yourself!"
                };
                userErrorEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: userErrorEmbed.Build());
                return;
            }

            double cooldownTime = DateTime.Now.AddHours(-server.PraiseCooldown).ToOADate();

            if (!(lastGivenPraise < cooldownTime))
            {
                var timeErrorEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"Sorry, you must wait " +
                                  $"`{(DateTime.FromOADate(lastGivenPraise) - DateTime.FromOADate(cooldownTime)).Humanize()}` " +
                                  $"before giving praise again."
                };
                timeErrorEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: timeErrorEmbed.Build());
                return;
            }

            if (reason == null)
            {
                reason = "No reason provided.";
            }

            var praise = new Praise
            {
                UserId = user.Id,
                ServerId = server.Id,
                GivenBy = Context.User.Id,
                Reason = reason,
                Server = server,
                TimeGiven = DateTime.Now.ToOADate()
            };

            await ServerQueries.AddPraiseAsync(praise);

            var newTargetPraise = await ServerQueries.GetPraiseAsync(praise.UserId, praise.ServerId);
            int newTargetPraiseCount = newTargetPraise.Count;

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully awarded `{user}` with `+1` praise.\n" +
                              $"Reason: `{reason}`",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{user.Username} now has {newTargetPraiseCount} praise. " +
                           $"You have {userPraise.Count} praise."
                }
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}