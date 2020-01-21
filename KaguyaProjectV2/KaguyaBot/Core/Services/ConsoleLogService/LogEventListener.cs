using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService
{
    public class LogEventListener
    {
        private static DiscordShardedClient _client = ConfigProperties.Client;

        public static void Listener()
        {
            _client.ShardConnected += async (DiscordSocketClient client) => await ConsoleLogger.LogAsync($"Shard {client.ShardId} connected.", LogLvl.TRACE);
            _client.ShardDisconnected += async (Exception ex, DiscordSocketClient client) => await ConsoleLogger.LogAsync($"Shard {client.ShardId} disconnected. Exception: {ex.Message}", LogLvl.ERROR);
            _client.ShardReady += async (DiscordSocketClient client) => await ConsoleLogger.LogAsync($"Shard {client.ShardId} ready. Guilds: {client.Guilds.Count:N0}", LogLvl.INFO);
            _client.ShardLatencyUpdated += async (int oldLatency, int newLatency, DiscordSocketClient client) =>
                await ConsoleLogger.LogAsync($"Shard {client.ShardId} latency has updated. [Old: {oldLatency}ms | New: {newLatency}ms]", LogLvl.TRACE);

            _client.ChannelCreated += async (SocketChannel channel) =>
                await ConsoleLogger.LogAsync($"Channel Created [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLvl.DEBUG);
            _client.ChannelDestroyed += async (SocketChannel channel) =>
                await ConsoleLogger.LogAsync($"Channel Deleted [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLvl.DEBUG);
            _client.ChannelUpdated += async (SocketChannel channel, SocketChannel channel2) =>
                await ConsoleLogger.LogAsync($"Channel Updated [Name: #{channel} | New Name: #{channel2} | ID: {channel.Id} |" +
                $" Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLvl.TRACE);

            _client.JoinedGuild += async (SocketGuild guild) => await ConsoleLogger.LogAsync($"Joined Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLvl.INFO);
            _client.LeftGuild += async (SocketGuild guild) => await ConsoleLogger.LogAsync($"Left Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLvl.INFO);

            _client.MessageDeleted += async (Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) =>
            {
                if (cache.Value is null) return;
                await ConsoleLogger.LogAsync($"Message Deleted [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLvl.TRACE);
            };

            _client.MessageUpdated += async (Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel) =>
            {
                if (cache.Value is null) return;
                if (cache.Value.Embeds != null && cache.Value.Content == msg.Content) return;
                await ConsoleLogger.LogAsync($"Message Updated [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLvl.TRACE);
            };

            _client.MessageReceived += async (SocketMessage msg) => await ConsoleLogger.LogAsync($"Message Received [Author: {msg.Author} | ID: {msg.Id} | Bot: {msg.Author.IsBot}]", LogLvl.TRACE);

            _client.RoleCreated += async (SocketRole role) => await ConsoleLogger.LogAsync($"Role Created [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLvl.DEBUG);
            _client.RoleDeleted += async (SocketRole role) => await ConsoleLogger.LogAsync($"Role Deleted [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLvl.DEBUG);
            _client.RoleUpdated += async (SocketRole role, SocketRole role2) =>
                await ConsoleLogger.LogAsync($"Role Updated [Name: {role.Name} | New Name: {role2.Name} | ID: {role.Id} | Guild: {role.Guild}]", LogLvl.DEBUG);

            _client.UserBanned += async (SocketUser user, SocketGuild guild) => await ConsoleLogger.LogAsync($"User Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLvl.DEBUG);
            _client.UserUnbanned += async (SocketUser user, SocketGuild guild) => await ConsoleLogger.LogAsync($"User Un-Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLvl.DEBUG);
            _client.UserJoined += async (SocketGuildUser user) => await ConsoleLogger.LogAsync($"User Joined Guild [User: {user} | User ID: {user.Id} | Guild: {user.Guild}]", LogLvl.DEBUG);

            _client.UserVoiceStateUpdated += async (SocketUser user, SocketVoiceState vs1, SocketVoiceState vs2) => await ConsoleLogger.LogAsync($"User Voice State Updated: [User: {user}]", LogLvl.TRACE);

            //Twitch stuff
        }
    }
}