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
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility.SocialMedia
{
    public class Twitch : ModuleBase<ShardedCommandContext>
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
            var existingChannels = ServerQueries.GetAllTwitchChannels().Where(x => x.ServerId == Context.Guild.Id);
            var twitchApi = GlobalProperties.twitchApi;
            var userIndex = await twitchApi.V5.Users.GetUserByNameAsync(twitchChannelName);

            IEnumerable<TwitchChannel> twitchChannels = existingChannels as TwitchChannel[] ?? existingChannels.ToArray();

            if (twitchChannels.Count() >= 3 && !server.IsPremium)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = "This server must be premium to support more than 3 Twitch channels simultaneously."
                };

                await ReplyAsync(embed: embed.Build());
                return;
            }

            if (twitchChannels.Count() >= 35 && server.IsPremium)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = "Premium servers may not register more than 35 Twitch channels."
                };

                await ReplyAsync(embed: embed.Build());
                return;
            }

            //Continue here

            try
            {
                var match = userIndex.Matches[0];
                var tchannel = new TwitchChannel
                {
                    ServerId = Context.Guild.Id,
                    ChannelName = match.Name.Trim(),
                    MentionEveryone = mentionEveryone
                };

                ServerQueries.AddTwitchChannel(tchannel);

                string mentionString = "";

                switch (mentionEveryone)
                {
                    case true:
                        mentionString = "I will mention everyone when sending these notifications.";
                        break;
                    case false:
                        mentionString = "I will not mention everyone when sending these notifications.";
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
    }
}
