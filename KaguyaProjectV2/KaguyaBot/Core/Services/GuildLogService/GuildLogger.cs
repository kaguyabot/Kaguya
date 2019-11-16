using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration.LogCommands;
using KaguyaProjectV2.KaguyaBot.Core.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.GuildLogService
{
    public class GuildLogger
    {
        static DiscordShardedClient _client = GlobalProperties.client;
        static KaguyaEmbedBuilder embed;

        public static async void GuildLogListener()
        {
            //KaguyaServerLog
            _client.MessageDeleted += _client_MessageDeleted;
            _client.MessageUpdated += _client_MessageUpdated;
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeft;
            //AntiRaid
            //UserKicked
            _client.UserBanned += _client_UserBanned;
            _client.UserUnbanned += _client_UserUnbanned;
            //FilteredPhrase
            _client.UserVoiceStateUpdated += _client_UserVoiceStateUpdated;
            //LevelUps
            //Shadowbans
            //UnShadowbans
            //Warns
            //Unwarns
            //Twitch, youtube, reddit, twitter notifications
        }

        private static async Task _client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            Server server;

            if (arg2.VoiceChannel is null)
                server = ServerQueries.GetServer(arg3.VoiceChannel.Guild.Id);
            else
                server = ServerQueries.GetServer(arg2.VoiceChannel.Guild.Id);

            string oldVoice = "";
            string newVoice = "";

            if (server.LogVoiceChannelConnections != 0)
            {
                if(arg2.VoiceChannel is null)
                {
                    oldVoice = "No prior channel.";
                    newVoice = $"New connection to {arg3.VoiceChannel.Name}";
                }
                if(arg2.VoiceChannel != null && arg3.VoiceChannel != null)
                {
                    oldVoice = $"{arg2.VoiceChannel.Name}";
                    newVoice = $"{arg3.VoiceChannel.Name}";
                }
                if(arg3.VoiceChannel is null)
                {
                    oldVoice = $"{arg2.VoiceChannel.Name}";
                    newVoice = "No new channel, user has disconnected.";
                }

                string embedURL = "";

                if (oldVoice.Contains("No prior channel."))
                {
                    embedURL = "https://www.flaticon.com/premium-icon/icons/svg/2198/2198066.svg";
                }
                if(newVoice.Contains("No new channel,"))
                {
                    embedURL = "https://www.flaticon.com/premium-icon/icons/svg/2198/2198068.svg";
                }
                if(!newVoice.Contains("No new channel,") && !oldVoice.Contains("No prior channel."))
                {
                    embedURL = "https://www.flaticon.com/premium-icon/icons/svg/2202/2202034.svg";
                }

                embed = new KaguyaEmbedBuilder
                {
                    Title = "User Voice State Updated",
                    Description = $"User: `[Name: {arg1} | ID: {arg1.Id}]`\nOld Voice Channel: `{oldVoice}`\nNew Voice Channel: `{newVoice}`",
                    ThumbnailUrl = embedURL
                };
            }

            await _client.GetGuild(server.Id).GetTextChannel(server.LogVoiceChannelConnections).SendMessageAsync(embed: embed.Build());
        }

        private static Task _client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = ServerQueries.GetServer(arg2.Id);

            return Task.CompletedTask;
        }

        private static Task _client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = ServerQueries.GetServer(arg2.Id);
            return Task.CompletedTask;
        }

        private static Task _client_UserLeft(SocketGuildUser arg)
        {
            Server server = ServerQueries.GetServer(arg.Guild.Id);

            return Task.CompletedTask;
        }

        private static Task _client_UserJoined(SocketGuildUser arg)
        {
            Server server = ServerQueries.GetServer(arg.Guild.Id);

            return Task.CompletedTask;
        }

        private static Task _client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            return Task.CompletedTask;
        }

        private static Task _client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            return Task.CompletedTask;
        }
    }
}
