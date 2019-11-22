using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using TwitchLib.Client;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService
{
    public class LogEventListener
    {
        private static DiscordShardedClient _client = GlobalProperties.client;

        public static void Listener()
        {
            _client.ShardConnected += (DiscordSocketClient client) => ConsoleLogger.Log($"Shard {client.ShardId} connected.", LogLevel.TRACE);
            _client.ShardDisconnected += (Exception ex, DiscordSocketClient client) => ConsoleLogger.Log($"Shard {client.ShardId} disconnected. Exception: {ex.Message}", LogLevel.ERROR);
            _client.ShardReady += (DiscordSocketClient client) => ConsoleLogger.Log($"Shard {client.ShardId} ready. Guilds: {client.Guilds.Count.ToString("N0")}", LogLevel.INFO);
            _client.ShardLatencyUpdated += (int oldLatency, int newLatency, DiscordSocketClient client) =>
                ConsoleLogger.Log($"Shard {client.ShardId} latency has updated. [Old: {oldLatency}ms | New: {newLatency}ms]", LogLevel.TRACE);

            _client.ChannelCreated += (SocketChannel channel) =>
                ConsoleLogger.Log($"Channel Created [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLevel.DEBUG);
            _client.ChannelDestroyed += (SocketChannel channel) =>
                ConsoleLogger.Log($"Channel Deleted [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLevel.DEBUG);
            _client.ChannelUpdated += (SocketChannel channel, SocketChannel channel2) =>
                ConsoleLogger.Log($"Channel Updated [Name: #{channel} | New Name: #{channel2} | ID: {channel.Id} |" +
                $" Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLevel.TRACE);

            _client.JoinedGuild += (SocketGuild guild) => ConsoleLogger.Log($"Joined Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLevel.INFO);
            _client.LeftGuild += (SocketGuild guild) => ConsoleLogger.Log($"Left Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLevel.INFO);

            _client.MessageDeleted += (Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) =>
            {
                if (cache.Value is null) return Task.CompletedTask;
                ConsoleLogger.Log($"Message Deleted [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLevel.TRACE);
                return Task.CompletedTask;
            };

            _client.MessageUpdated += (Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel) =>
            {
                if (cache.Value is null) return Task.CompletedTask;
                ConsoleLogger.Log($"Message Updated [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLevel.TRACE);
                return Task.CompletedTask;
            };

            _client.MessageReceived += (SocketMessage msg) => ConsoleLogger.Log($"Message Received [Author: {msg.Author} | ID: {msg.Id} | Bot: {msg.Author.IsBot}]", LogLevel.TRACE);

            _client.RoleCreated += (SocketRole role) => ConsoleLogger.Log($"Role Created [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLevel.DEBUG);
            _client.RoleDeleted += (SocketRole role) => ConsoleLogger.Log($"Role Deleted [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLevel.DEBUG);
            _client.RoleUpdated += (SocketRole role, SocketRole role2) =>
                ConsoleLogger.Log($"Role Updated [Name: {role.Name} | New Name: {role2.Name} | ID: {role.Id} | Guild: {role.Guild}]", LogLevel.DEBUG);

            _client.UserBanned += (SocketUser user, SocketGuild guild) => ConsoleLogger.Log($"User Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLevel.DEBUG);
            _client.UserUnbanned += (SocketUser user, SocketGuild guild) => ConsoleLogger.Log($"User Un-Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLevel.DEBUG);
            _client.UserJoined += (SocketGuildUser user) => ConsoleLogger.Log($"User Joined Guild [User: {user} | User ID: {user.Id} | Guild: {user.Guild}]", LogLevel.DEBUG);

            _client.UserVoiceStateUpdated += (SocketUser user, SocketVoiceState vs1, SocketVoiceState vs2) => ConsoleLogger.Log($"User Voice State Updated: [User: {user}]", LogLevel.TRACE);

            //Twitch stuff



        }
    }
}