using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class GuildLogger
    {
        private static readonly DiscordShardedClient _client = ConfigProperties.Client;
        private static KaguyaEmbedBuilder _embed;

        public static void InitializeGuildLogListener()
        {
            _client.MessageDeleted += _client_MessageDeleted;
            _client.MessageUpdated += _client_MessageUpdated;
            _client.UserJoined += _client_UserJoined;
            _client.UserLeft += _client_UserLeft;
            AntiRaidEvent.OnRaid += OnAntiRaid;
            //UserKicked
            _client.UserBanned += _client_UserBanned;
            _client.UserUnbanned += _client_UserUnbanned;
            //FilteredPhrase
            _client.UserVoiceStateUpdated += _client_UserVoiceStateUpdated;
            //LevelUps
        }

        private static async Task _client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(((SocketGuildChannel) arg2).Guild.Id);

            if (server.LogDeletedMessages == 0)
                return;

            if (server.IsCurrentlyPurgingMessages)
                return;

            IMessage message = arg1.Value;

            if (message is null || message.Author.IsBot)
                return;

            KaguyaEmbedBuilder embed;
            string content = string.IsNullOrEmpty(message.Content)
                ? "<Message contained no text>"
                : $"{message.Content}";

            if (message.Attachments.Count == 0)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Title = "Message Deleted",
                    Description = $"User: `[Name: {message.Author} | ID: {message.Author.Id}]`\n" +
                                  $"Content: `{content}`\nChannel: `{message.Channel}`\nDate Created: `{message.CreatedAt}`\n",
                    ThumbnailUrl = "https://i.imgur.com/hooIc7u.png"
                };
            }
            else
            {
                if (server.IsPremium)
                {
                    embed = new KaguyaEmbedBuilder
                    {
                        Title = "Message Deleted",
                        Description = $"User: `[Name: {message.Author} | ID: {message.Author.Id}]`\n" +
                                      $"Content: `{content}`\nChannel: `{message.Channel}`\nDate Created: `{message.CreatedAt}`\n" +
                                      $"Number of Attachments: `{message.Attachments.Count}`\nAttachment URL: {message.Attachments.FirstOrDefault()?.ProxyUrl}",
                        ThumbnailUrl = "https://i.imgur.com/hooIc7u.png",
                        ImageUrl = message.Attachments.FirstOrDefault()?.ProxyUrl
                    };
                }
                else
                {
                    embed = new KaguyaEmbedBuilder
                    {
                        Title = "Message Deleted",
                        Description = $"User: `[Name: {message.Author} | ID: {message.Author.Id}]`\n" +
                                      $"Content: `{content}`\nChannel: `{message.Channel}`\nDate Created: `{message.CreatedAt}`\n" +
                                      $"Number of Attachments: `{message.Attachments.Count}`",
                        ThumbnailUrl = "https://i.imgur.com/hooIc7u.png"
                    };
                }
            }

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogDeletedMessages)
                         .SendEmbedAsync(embed);
        }

        private static async Task _client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (arg3 is SocketGuildChannel channel)
            {
                Server server = await DatabaseQueries.GetOrCreateServerAsync(channel.Guild.Id);

                if (server.LogUpdatedMessages == 0)
                    return;

                IMessage oldMsg = arg1.Value;
                string content = oldMsg.Content;

                if (oldMsg.Author.IsBot) return;
                if (string.IsNullOrEmpty(content)) content = "<No previous text>";

                if (content == arg2.Content)
                    return;

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Message Updated",
                    Description = $"User: `[Name: {oldMsg.Author} | ID: {oldMsg.Author.Id}]`\n" +
                                  $"Old Message: `{content}`\nNew Message: `{arg2.Content}`\nChannel: `{oldMsg.Channel}`\n" +
                                  $"Date Originally Created: `{oldMsg.CreatedAt}`\n",
                    ThumbnailUrl = "https://i.imgur.com/uYkjSxM.png"
                };

                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUpdatedMessages).SendEmbedAsync(embed);
            }
        }

        private static async Task _client_UserJoined(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);

            if (server.LogUserJoins == 0)
                return;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "User Joined",
                Description =
                    $"User: `[Name: {arg} | ID: {arg.Id}]`\nAccount Created: `{arg.CreatedAt}`\nStatus: `{Regex.Replace(arg.Status.ToString(), "([a-z])([A-Z])", "$1 $2")}`",
                ThumbnailUrl = "https://i.imgur.com/3PsE0Ey.png"
            };

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendEmbedAsync(embed);
        }

        private static async Task _client_UserLeft(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);

            if (server.LogUserLeaves == 0)
                return;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "User Left",
                Description =
                    $"User: `[Name: {arg} | ID: {arg.Id}]`\nAccount Created: `{arg.CreatedAt}`\nStatus: `{Regex.Replace(arg.Status.ToString(), "([a-z])([A-Z])", "$1 $2")}`",
                ThumbnailUrl = "https://i.imgur.com/1I0ayRE.png"
            };

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendEmbedAsync(embed);
        }

        private static async Task OnAntiRaid(AntiRaidEventArgs e)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(e.SocketGuild.Id);

            if (server.LogAntiraids == 0)
                return;

            string actionedUsers = "";
            foreach (SocketGuildUser user in e.GuildUsers)
            {
                actionedUsers +=
                    $"Name: `{user}` | ID: `{user.Id}`\n";
            }

            if (actionedUsers.Length > 1750)
                actionedUsers = e.GuildUsers.Count.ToString("N0");

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Anti-Raid Triggered",
                Description = $"Punishment: `{e.Punishment}`\n" +
                              $"Users Actioned:\n\n{actionedUsers}",
                ThumbnailUrl = "https://i.imgur.com/QFY9CdE.png"
            };

            await _client.GetGuild(e.SocketGuild.Id).GetTextChannel(server.LogAntiraids).SendEmbedAsync(embed);
        }

        private static async Task _client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg2.Id);

            if (server.LogBans == 0)
                return;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "User Banned",
                Description = $"User: `[Name: {arg1} | ID: {arg1.Id}`]\n" +
                              $"Reason: `{(await arg2.GetBanAsync(arg1.Id)).Reason}`",
                ThumbnailUrl = "https://i.imgur.com/6Xk2HCG.png"
            };

            await arg2.GetTextChannel(server.LogBans).SendEmbedAsync(embed);
        }

        private static async Task _client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg2.Id);

            if (server.LogUnbans == 0)
                return;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "User Un-Banned",
                Description = $"User: `[Name: {arg1} | ID: {arg1.Id}`]\n",
                ThumbnailUrl = "https://i.imgur.com/uYOa4VD.png"
            };

            await arg2.GetTextChannel(server.LogBans).SendEmbedAsync(embed);
        }

        private static async Task _client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            Server server;

            if (arg2.VoiceChannel is null)
                server = await DatabaseQueries.GetOrCreateServerAsync(arg3.VoiceChannel.Guild.Id);
            else
                server = await DatabaseQueries.GetOrCreateServerAsync(arg2.VoiceChannel.Guild.Id);

            string oldVoice = string.Empty, newVoice = string.Empty, embedUrl = string.Empty;

            if (server.LogVoiceChannelConnections != 0)
            {
                if (arg2.VoiceChannel is null)
                {
                    oldVoice = "No prior channel.";
                    newVoice = $"New connection to {arg3.VoiceChannel.Name}";
                    embedUrl = "https://i.imgur.com/WPtsNwD.png";
                }

                if (arg2.VoiceChannel != null && arg3.VoiceChannel != null)
                {
                    oldVoice = $"{arg2.VoiceChannel.Name}";
                    newVoice = $"{arg3.VoiceChannel.Name}";
                    embedUrl = "https://i.imgur.com/Z4JTUBq.png";
                }

                if (arg3.VoiceChannel is null)
                {
                    oldVoice = $"{arg2.VoiceChannel.Name}";
                    newVoice = "No new channel, user has disconnected.";
                    embedUrl = "https://i.imgur.com/pAifz2P.png";
                }

                _embed = new KaguyaEmbedBuilder
                {
                    Title = "User Voice State Updated",
                    Description = $"User: `[Name: {arg1} | ID: {arg1.Id}]`\nOld Voice Channel: `{oldVoice}`\nNew Voice Channel: `{newVoice}`",
                    ThumbnailUrl = embedUrl
                };
            }

            if (server.LogVoiceChannelConnections != 0)
            {
                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogVoiceChannelConnections)
                             .SendEmbedAsync(_embed);
            }
        }
    }
}