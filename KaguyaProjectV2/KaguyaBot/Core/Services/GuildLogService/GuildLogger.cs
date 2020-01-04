using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System.Text.RegularExpressions;
using System;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.GuildLogService
{
    public class GuildLogger
    {
        private static readonly DiscordShardedClient _client = ConfigProperties.Client;
        private static KaguyaEmbedBuilder _embed;

        public static void GuildLogListener()
        {
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
            //Twitch, youtube, reddit, twitter notifications
        }

        private static async Task _client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            Server server;

            if (arg2.VoiceChannel is null)
                server = await DatabaseQueries.GetOrCreateServerAsync(arg3.VoiceChannel.Guild.Id);
            else
                server = await DatabaseQueries.GetOrCreateServerAsync(arg2.VoiceChannel.Guild.Id);

            string oldVoice = string.Empty, newVoice = string.Empty, embedURL = string.Empty;

            if (server.LogVoiceChannelConnections != 0)
            {
                if (arg2.VoiceChannel is null)
                {
                    oldVoice = "No prior channel.";
                    newVoice = $"New connection to {arg3.VoiceChannel.Name}";
                    embedURL = "https://i.imgur.com/WPtsNwD.png";
                }
                if (arg2.VoiceChannel != null && arg3.VoiceChannel != null)
                {
                    oldVoice = $"{arg2.VoiceChannel.Name}";
                    newVoice = $"{arg3.VoiceChannel.Name}";
                    embedURL = "https://i.imgur.com/Z4JTUBq.png";
                }
                if (arg3.VoiceChannel is null)
                {
                    oldVoice = $"{arg2.VoiceChannel.Name}";
                    newVoice = "No new channel, user has disconnected.";
                    embedURL = "https://i.imgur.com/pAifz2P.png";
                }

                _embed = new KaguyaEmbedBuilder
                {
                    Title = "User Voice State Updated",
                    Description = $"User: `[Name: {arg1} | ID: {arg1.Id}]`\nOld Voice Channel: `{oldVoice}`\nNew Voice Channel: `{newVoice}`",
                    ThumbnailUrl = embedURL
                };
            }

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogVoiceChannelConnections).SendMessageAsync(embed: _embed.Build());
        }

        private static async Task _client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg2.Id);
            if (server.LogUnbans == 0)
                return;
        }

        private static async Task _client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg2.Id);
            if (server.LogBans == 0)
                return;
        }

        private static async Task _client_UserLeft(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);
            if (server.LogUserLeaves == 0)
                return;

            KaguyaEmbedBuilder builder = new KaguyaEmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = arg.GetAvatarUrl()
                },
                Title = "User Left",
                Description = $"User: `[Name: {arg} | ID: {arg.Id}]`\nAccount Created: `{arg.CreatedAt}`\nStatus: `{Regex.Replace(arg.Status.ToString(), "([a-z])([A-Z])", "$1 $2")}`",
                ThumbnailUrl = "https://i.imgur.com/1I0ayRE.png",
            };

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendMessageAsync(embed: builder.Build());
        }

        private static async Task _client_UserJoined(SocketGuildUser arg)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(arg.Guild.Id);
            if (server.LogUserJoins == 0)
                return;

            KaguyaEmbedBuilder builder = new KaguyaEmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = arg.GetAvatarUrl()
                },
                Title = "User Joined",
                Description = $"User: `[Name: {arg} | ID: {arg.Id}]`\nAccount Created: `{arg.CreatedAt}`\nStatus: `{Regex.Replace(arg.Status.ToString(), "([a-z])([A-Z])", "$1 $2")}`",
                ThumbnailUrl = "https://i.imgur.com/3PsE0Ey.png",
            };

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUserJoins).SendMessageAsync(embed: builder.Build());
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

                KaguyaEmbedBuilder builder = new KaguyaEmbedBuilder
                {
                    Title = "Message Updated",
                    Description = $"User: `[Name: {oldMsg.Author} | ID: {oldMsg.Author.Id}]`\n" +
                                  $"Old Message: `{content}`\nNew Message: `{arg2.Content}`\nChannel: `{oldMsg.Channel}`\nDate Originally Created: `{oldMsg.CreatedAt}`\n",
                    ThumbnailUrl = "https://i.imgur.com/uYkjSxM.png"
                };

                await _client.GetGuild(server.ServerId).GetTextChannel(server.LogUpdatedMessages).SendMessageAsync(embed: builder.Build());
            }
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


            KaguyaEmbedBuilder builder;
            string content = string.IsNullOrEmpty(message.Content)
                ? "<Message contained no text>" : $"{message.Content}";

            if (message.Attachments.Count == 0)
            {
                builder = new KaguyaEmbedBuilder
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
                    builder = new KaguyaEmbedBuilder
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
                    builder = new KaguyaEmbedBuilder
                    {
                        Title = "Message Deleted",
                        Description = $"User: `[Name: {message.Author} | ID: {message.Author.Id}]`\n" +
                                      $"Content: `{content}`\nChannel: `{message.Channel}`\nDate Created: `{message.CreatedAt}`\n" +
                                      $"Number of Attachments: `{message.Attachments.Count}`",
                        ThumbnailUrl = "https://i.imgur.com/hooIc7u.png",
                    };
                }
            }

            await _client.GetGuild(server.ServerId).GetTextChannel(server.LogDeletedMessages)
                .SendMessageAsync(embed: builder.Build());
        }
    }
}