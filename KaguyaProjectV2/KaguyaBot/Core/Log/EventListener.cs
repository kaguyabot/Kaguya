using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Log
{
    public class EventListener
    {
        static DiscordShardedClient _client = GlobalProperties.Client;

        public static void Listener()
        {
            _client.ShardConnected += (DiscordSocketClient client) => Logger.Log($"Shard {client.ShardId} connected.", LogLevel.DEBUG);
            _client.ShardDisconnected += (Exception ex, DiscordSocketClient client) => Logger.Log($"Shard {client.ShardId} disconnected. Exception: {ex.Message}", LogLevel.ERROR);
            _client.ShardReady += (DiscordSocketClient client) => Logger.Log($"Shard {client.ShardId} ready. Guilds: {client.Guilds.Count.ToString("N0")}", LogLevel.INFO);
            _client.ShardLatencyUpdated += (int oldLatency, int newLatency, DiscordSocketClient client) =>
                Logger.Log($"Shard {client.ShardId} latency has updated. [Old: {oldLatency}ms | New: {newLatency}ms]", LogLevel.TRACE);

            _client.ChannelCreated += (SocketChannel channel) =>
                Logger.Log($"Channel Created [Name: #{(channel as SocketGuildChannel).Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel).Guild}]", LogLevel.DEBUG);
            _client.ChannelDestroyed += (SocketChannel channel) =>
                Logger.Log($"Channel Deleted [Name: #{(channel as SocketGuildChannel).Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel).Guild}]", LogLevel.DEBUG);
            _client.ChannelUpdated += (SocketChannel channel, SocketChannel channel2) =>
                Logger.Log($"Channel Updated [Name: #{(channel as SocketGuildChannel).Name} | New Name: #{(channel2 as SocketGuildChannel).Name} | ID: {channel.Id} |" +
                $" Guild: {(channel as SocketGuildChannel).Guild}]", LogLevel.TRACE);

            _client.JoinedGuild += (SocketGuild guild) => Logger.Log($"Joined Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLevel.INFO);
            _client.LeftGuild += (SocketGuild guild) => Logger.Log($"Left Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLevel.INFO);

            //_client.MessageDeleted += (Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) =>
            //    Logger.Log($"Message Deleted [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLevel.TRACE);
            _client.MessageReceived += (SocketMessage msg) => Logger.Log($"Message Received [Author: {msg.Author} | ID: {msg.Id} | Bot: {msg.Author.IsBot}]", LogLevel.TRACE);

            _client.RoleCreated += (SocketRole role) => Logger.Log($"Role Created [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLevel.DEBUG);
            _client.RoleDeleted += (SocketRole role) => Logger.Log($"Role Deleted [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLevel.DEBUG);
            _client.RoleUpdated += (SocketRole role, SocketRole role2) =>
                Logger.Log($"Role Updated [Name: {role.Name} | New Name: {role2.Name} | ID: {role.Id} | Guild: {role.Guild}]", LogLevel.DEBUG);

            _client.UserBanned += (SocketUser user, SocketGuild guild) => Logger.Log($"User Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLevel.DEBUG);
            _client.UserUnbanned += (SocketUser user, SocketGuild guild) => Logger.Log($"User Un-Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLevel.DEBUG);
            _client.UserJoined += (SocketGuildUser user) => Logger.Log($"User Joined Guild [User: {user} | User ID: {user.Id} | Guild: {user.Guild}]", LogLevel.DEBUG);
        }
    }
}