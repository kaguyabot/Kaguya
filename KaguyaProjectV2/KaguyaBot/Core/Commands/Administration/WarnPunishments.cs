using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class WarnPunishments : InteractiveBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("WarnPunishments", RunMode = RunMode.Async)]
        [Alias("wp")]
        [Summary("Allows a server Administrator to configure the server's warn-punishment scheme. " +
                 "Admins have the ability to configure up to `four` actions that get triggered " +
                 "when a user reaches a set amount of warnings. These four options are `mute`, " +
                 "`kick`, `shadowban`, and `ban`")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command()
        {
            var server = await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var currentScheme = server.WarnActions.FirstOrDefault();

            if (currentScheme == null)
            {
                var initEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"This server currently has no configured punishment scheme. Would " +
                                  $"you like to set one up now?",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Clicking the check mark will begin the setup wizard. This will timeout " +
                               "in 30 seconds."
                    }
                };
                initEmbed.SetColor(EmbedColor.PINK);

                var initData = new ReactionCallbackData("", initEmbed.Build(), true, true,
                    TimeSpan.FromSeconds(30), async c =>
                        await c.Channel.SendMessageAsync($"{Context.User.Mention} " +
                                                         $"Punishment setup timed out."))
                    .AddCallBack(new Emoji("✅"), async (c, r) =>
                    {
                        var newScheme = new WarnSetting();

                        await ReplyAsync($"{Context.User.Mention} Alrighty {Context.User.Username}, let's go " +
                                         $"ahead and set this up then! I will prompt you four times. All you have " +
                                         $"to do is tell me how many warnings `(1-99)` you want there to be before the prompted " +
                                         $"action is triggered. All you do is reply with the number in chat - " +
                                         $"nothing else, otherwise it will be counted as a zero.");

                        await Task.Delay(5000);
                        await ReplyAsync("Got it? Alrighty then!");
                        await Task.Delay(1500);

                        await ReplyAsync("How many warnings before I should automatically `mute` users?");
                        var muteMsg = await NextMessageAsync();
                        var muteNum = muteMsg.Content.AsInteger();

                        if (muteNum > 1 && muteNum < 100)
                            newScheme.Mute = muteNum;

                        await ReplyAsync($"{Context.User.Mention} Awesome! What about kicks?");
                        var kickMsg = await NextMessageAsync();
                        var kickNum = kickMsg.Content.AsInteger();

                        if (kickNum > 1 && kickNum < 100)
                            newScheme.Kick = kickNum;

                        await ReplyAsync($"{Context.User.Mention} Doing great...how about shadowbans? If you don't " +
                                         $"know what a shadowban is, it basically strips a user of every role they " +
                                         $"have and disables their ability to see any channel at all. It's basically " +
                                         $"banning them but without actually ejecting the member from the server.");
                        var sbMsg = await NextMessageAsync();
                        var sbNum = sbMsg.Content.AsInteger();

                        if (sbNum > 1 && sbNum < 100)
                            newScheme.Shadowban = sbNum;

                        await ReplyAsync($"{Context.User.Mention} Got it. Final step...how many for bans?");
                        var banMsg = await NextMessageAsync();
                        var banNum = banMsg.Content.AsInteger();

                        if (banNum > 1 && banNum < 100)
                            newScheme.Ban = banNum;

                        var successEmbed = new KaguyaEmbedBuilder
                        {
                            Description = $"Awesome! Here's what I've got:\n\n" +
                                          $"Mutes: `{newScheme.Mute}` warnings" +
                                          $"Kicks: `{newScheme.Kick}` warnings" +
                                          $"Shadowbans: `{newScheme.Shadowban}` warnings" +
                                          $"Bans: `{newScheme.Ban}` warnings\n\n" +
                                          $"I've gone ahead and applied these settings for you. They are now in effect, " +
                                          $"and no more action is required."
                        };
                        successEmbed.SetColor(EmbedColor.GOLD);

                        await ReplyAsync(embed: successEmbed.Build());
                    })
                    .AddCallBack(new Emoji("⛔"), async (c, r) =>
                    {
                        await c.Channel.SendMessageAsync("Okay, we can configure this later.");
                    });

                await InlineReactionReplyAsync(initData);
            }
            else
            {
                var emojis = HelpfulObjects.EmojisOneThroughNine();

                int muteW = currentScheme.Mute;
                int kickW = currentScheme.Kick;
                int shadowbanW = currentScheme.Shadowban;
                int banW = currentScheme.Ban;

                string des = $"Here's what your current punishment scheme looks like:\n\n" +
                             $"`#1.` Mute: {(muteW.IsZero() ? "Not configured" : $"{muteW} warnings")}\n" +
                             $"`#2.` Kick: {(kickW.IsZero() ? "Not configured" : $"{kickW} warnings")}\n" +
                             $"`#3.` Shadowban: {(shadowbanW.IsZero() ? "Not configured" : $"{shadowbanW} warnings")}\n" +
                             $"`#4.` Ban: {(banW.IsZero() ? "Not configured" : $"{banW} warnings")}\n\n " +
                             $"What would you like to configure?";

                var curEmbed = new KaguyaEmbedBuilder
                {
                    Description = des,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Click the reaction corresponding to what you want to configure."
                    }
                };

                var curData = new ReactionCallbackData("", curEmbed.Build(), true, true,
                    TimeSpan.FromSeconds(300),
                    async c => { await c.Channel.SendMessageAsync("Warning configuration timed out."); });

                curData.AddCallBack(emojis[0], async (c, r) => {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} What would you like to have " +
                                                           $"your new warn-threshold for `mutes` be? Just reply " +
                                                           $"with the number (must be `1-99`).");
                    var nextMsg = await NextMessageAsync();
                    var nextInt = nextMsg.Content.AsInteger();

                    if (nextInt != 0 && nextInt > 0 && nextInt < 100)
                    {
                        var newSetting = new WarnSetting
                        {
                            ServerId = Context.Guild.Id,
                            Mute = nextInt,
                            Kick = currentScheme.Kick,
                            Shadowban = currentScheme.Shadowban,
                            Ban = currentScheme.Ban
                        };

                        await ServerQueries.AddOrReplaceWarnSettingAsync(newSetting);

                        await ReplyAsync($"{Context.User.Mention} Successfully updated your preferences!");
                        return;
                    }

                    await ReplyAsync($"{Context.User.Mention} That input was invalid. " +
                                     $"This operation will be cancelled.");
                });
                curData.AddCallBack(emojis[1], async (c, r) =>
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} What would you like to have " +
                                                           $"your new warn-threshold for `kicks` be? Just reply " +
                                                           $"with the number (must be `1-99`).");
                    var nextMsg = await NextMessageAsync();
                    var nextInt = nextMsg.Content.AsInteger();

                    if (nextInt != 0 && nextInt > 0 && nextInt < 100)
                    {
                        var newSetting = new WarnSetting
                        {
                            ServerId = Context.Guild.Id,
                            Mute = currentScheme.Mute,
                            Kick = nextInt,
                            Shadowban = currentScheme.Shadowban,
                            Ban = currentScheme.Ban
                        };

                        await ServerQueries.AddOrReplaceWarnSettingAsync(newSetting);

                        await ReplyAsync($"{Context.User.Mention} Successfully updated your preferences!");
                        return;
                    }

                    await ReplyAsync($"{Context.User.Mention} That input was invalid. " +
                                     $"This operation will be cancelled.");
                });
                curData.AddCallBack(emojis[2], async (c, r) =>
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} What would you like to have " +
                                                           $"your new warn-threshold for `shadowbans` be? Just reply " +
                                                           $"with the number (must be `1-99`).");
                    var nextMsg = await NextMessageAsync();
                    var nextInt = nextMsg.Content.AsInteger();

                    if (nextInt != 0 && nextInt > 0 && nextInt < 100)
                    {
                        var newSetting = new WarnSetting
                        {
                            ServerId = Context.Guild.Id,
                            Mute = currentScheme.Mute,
                            Kick = currentScheme.Kick,
                            Shadowban = nextInt,
                            Ban = currentScheme.Ban
                        };

                        await ServerQueries.AddOrReplaceWarnSettingAsync(newSetting);

                        await ReplyAsync($"{Context.User.Mention} Successfully updated your preferences!");
                        return;
                    }

                    await ReplyAsync($"{Context.User.Mention} That input was invalid. " +
                                     $"This operation will be cancelled.");
                });
                curData.AddCallBack(emojis[3], async (c, r) => {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} What would you like to have " +
                                                           $"your new warn-threshold for `bans` be? Just reply " +
                                                           $"with the number (must be `1-99`).");
                    var nextMsg = await NextMessageAsync();
                    var nextInt = nextMsg.Content.AsInteger();

                    if (nextInt != 0 && nextInt > 0 && nextInt < 100)
                    {
                        var newSetting = new WarnSetting
                        {
                            ServerId = Context.Guild.Id,
                            Mute = currentScheme.Mute,
                            Kick = currentScheme.Kick,
                            Shadowban = currentScheme.Shadowban,
                            Ban = nextInt
                        };

                        await ServerQueries.AddOrReplaceWarnSettingAsync(newSetting);

                        await ReplyAsync($"{Context.User.Mention} Successfully updated your preferences!");
                        return;
                    }

                    await ReplyAsync($"{Context.User.Mention} That input was invalid. " +
                                     $"This operation will be cancelled.");
                });
            }
        }
    }
}
