using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility.SocialMedia
{
    public class Twitch : InteractiveBase<ShardedCommandContext>
    {
        [UtilityCommand]
        [Command("addtwitch")]
        [Alias("at")]
        [Summary("Takes in a Twitch channel name, Discord text channel, and a true/false statement to enable " +
                 "logging livestream notifications for a Twitch channel. The true/false at the end of the command " +
                 "determines whether the live-stream notification should mention everyone when posting the announcement.")]
        [Remarks("<TwitchChannel> <Discord chat channel> <true/false>\nTheKaguyaBot #live-streams true\nTheKaguyaBot #live-streams false")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task AddTwitchChannel(string twitchChannelName, SocketTextChannel channel, bool mentionEveryone)
        {
            KaguyaEmbedBuilder embed;

            Server server = ServerQueries.GetServer(Context.Guild.Id);
            var twitchChannels = ServerQueries.GetAllTwitchChannels().Where(x => x.ServerId == Context.Guild.Id).ToList();
            var twitchApi = GlobalProperties.twitchApi;
            var userIndex = await twitchApi.V5.Users.GetUserByNameAsync(twitchChannelName);

            if (await PremiumCheck(twitchChannels, server)) return;

            try
            {
                var match = userIndex.Matches[0];
                var tchannel = new TwitchChannel
                {
                    ServerId = Context.Guild.Id,
                    TextChannelId = channel.Id,
                    ChannelName = match.Name,
                    MentionEveryone = mentionEveryone
                };

                string tChannelData = $"Twitch Channel: `{tchannel.ChannelName}`\nMention everyone when announcing? `{mentionEveryone}`";

                if (twitchChannels.Any(item => item.ChannelName == tchannel.ChannelName && item.TextChannelId == tchannel.TextChannelId))
                {
                    string textChannelName = GlobalProperties.client.GetGuild(server.Id).GetTextChannel(tchannel.TextChannelId).Name;
                    embed = new KaguyaEmbedBuilder
                    {
                        Description = $"This Twitch stream is already being monitored in `#{textChannelName}`!\n\n" +
                                      $"Do you want me to send the notifications here instead? ✅\n" +
                                      $"Alternatively, I can send the notifications to both channels.❔\n" +
                                      $"If you want me to do nothing, react with ⛔ (or don't react at all).",
                    };
                    embed.SetColor(EmbedColor.VIOLET);

                    KaguyaEmbedBuilder succEmbed = NotificationEmbedBuilder(channel, tchannel, out KaguyaEmbedBuilder altEmbed, out KaguyaEmbedBuilder nothingEmbed);
                    await InlineReactionReply(embed, tchannel, succEmbed, altEmbed, nothingEmbed);

                    return;
                }

                ServerQueries.AddTwitchChannel(tchannel);

                string mentionString = "";
                switch (mentionEveryone)
                {
                    case true:
                        mentionString = "**I will mention everyone** when sending these notifications.";
                        break;
                    case false:
                        mentionString = "**I will not mention everyone** when sending these notifications.";
                        break;
                }

                embed = new KaguyaEmbedBuilder
                {
                    Title = "Twitch Channel Configuration",
                    Description = $"I will now send notifications to the channel `#{channel.Name}` " +
                                  $"for the Twitch channel `{twitchChannelName}`. {mentionString}"
                };

                await ReplyAsync(embed: embed.Build());
            }
            catch (IndexOutOfRangeException e)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Channel `{twitchChannelName}` was not found."
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
            }
        }

        private async Task InlineReactionReply(KaguyaEmbedBuilder embed, TwitchChannel tchannel, KaguyaEmbedBuilder succEmbed,
            KaguyaEmbedBuilder altEmbed, KaguyaEmbedBuilder nothingEmbed)
        {
            await InlineReactionReplyAsync(new ReactionCallbackData("", embed.Build(), true, true, TimeSpan.FromSeconds(60),
                    c =>
                        c.Channel.SendMessageAsync("Response has timed out."))
                .WithCallback(new Emoji("✅"), (c, r) =>
                {
                    ServerQueries.RemoveTwitchChannel(tchannel);
                    ServerQueries.AddTwitchChannel(tchannel);
                    return c.Channel.SendMessageAsync(embed: succEmbed.Build());
                })
                .WithCallback(new Emoji("❔"), (c, r) =>
                {
                    ServerQueries.AddTwitchChannel(tchannel);
                    return c.Channel.SendMessageAsync(embed: altEmbed.Build());
                })
                .WithCallback(new Emoji("⛔"), (c, r) => c.Channel.SendMessageAsync(embed: nothingEmbed.Build())));
        }

        private static KaguyaEmbedBuilder NotificationEmbedBuilder(SocketTextChannel channel, TwitchChannel tchannel,
        out KaguyaEmbedBuilder altEmbed, out KaguyaEmbedBuilder nothingEmbed)
        {
            var succEmbed = new KaguyaEmbedBuilder
            {
                Description = $"I will switch the notifications from `#{tchannel.ChannelName}` to `#{channel.Name}.`"
            };

            altEmbed = new KaguyaEmbedBuilder
            {
                Description = $"I will send two notifications when`#{tchannel.ChannelName}` to `#{channel.Name}.`"
            };

            nothingEmbed = new KaguyaEmbedBuilder
            {
                Description = $"I won't make any changes."
            };
            return succEmbed;
        }

        private async Task<bool> PremiumCheck(List<TwitchChannel> twitchChannels, Server server)
        {
            KaguyaEmbedBuilder embed;

            if (twitchChannels.Count >= 3 && !server.IsPremium)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = "This server must be premium to support more than 3 Twitch channels simultaneously."
                };

                await ReplyAsync(embed: embed.Build());
                return true;
            }

            if (twitchChannels.Count >= 35 && server.IsPremium)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = "Premium servers may not register more than 35 Twitch channels."
                };

                await ReplyAsync(embed: embed.Build());
                return true;
            }

            return false;
        }
    }
}
