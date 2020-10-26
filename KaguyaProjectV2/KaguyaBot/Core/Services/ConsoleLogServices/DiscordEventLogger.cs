using System;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices
{
    public class DiscordEventLogger
    {
        private static DiscordShardedClient _client = ConfigProperties.Client;

        public static void InitLogger()
        {
            _client.ShardConnected += async client =>
                await ConsoleLogger.LogAsync($"Shard {client.ShardId} connected.", LogLvl.TRACE);

            _client.ShardDisconnected += async (ex, client) =>
                await ConsoleLogger.LogAsync($"Shard {client.ShardId} disconnected. Exception: {ex.Message}\nException Type: {ex.GetType()}", LogLvl.ERROR);

            _client.ShardReady += async client =>
                await ConsoleLogger.LogAsync($"Shard {client.ShardId} ready. Guilds: {client.Guilds.Count:N0}", LogLvl.INFO);

            _client.ShardLatencyUpdated += async (oldLatency, newLatency, client) =>
                await ConsoleLogger.LogAsync($"Shard {client.ShardId} latency has updated. [Old: {oldLatency}ms | New: {newLatency}ms]",
                    LogLvl.TRACE);

            _client.ChannelCreated += async channel =>
                await ConsoleLogger.LogAsync(
                    $"Channel Created [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]",
                    LogLvl.DEBUG);

            _client.ChannelDestroyed += async channel =>
                await ConsoleLogger.LogAsync(
                    $"Channel Deleted [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]",
                    LogLvl.DEBUG);

            _client.ChannelUpdated += async (channel, channel2) =>
                await ConsoleLogger.LogAsync($"Channel Updated [Name: #{channel} | New Name: #{channel2} | ID: {channel.Id} |" +
                                             $" Guild: {(channel as SocketGuildChannel)?.Guild}]", LogLvl.TRACE);

            _client.JoinedGuild += async guild =>
                await ConsoleLogger.LogAsync($"Joined Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLvl.INFO);

            _client.LeftGuild += async guild =>
                await ConsoleLogger.LogAsync($"Left Guild [Name: {guild.Name} | ID: {guild.Id}]", LogLvl.INFO);

            _client.MessageDeleted += async (cache, channel) =>
            {
                if (cache.Value is null) return;
                await ConsoleLogger.LogAsync($"Message Deleted [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLvl.TRACE);
            };

            _client.MessageUpdated += async (cache, msg, channel) =>
            {
                if (cache.Value is null) return;
                if (cache.Value.Embeds != null && cache.Value.Content == msg.Content) return;
                await ConsoleLogger.LogAsync($"Message Updated [Author: {cache.Value.Author} | ID: {cache.Id}]", LogLvl.TRACE);
            };

            _client.MessageReceived += async msg =>
                await ConsoleLogger.LogAsync($"Message Received [Author: {msg.Author} | ID: {msg.Id} | Bot: {msg.Author.IsBot}]", LogLvl.TRACE);

            _client.RoleCreated += async role =>
                await ConsoleLogger.LogAsync($"Role Created [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLvl.DEBUG);

            _client.RoleDeleted += async role =>
                await ConsoleLogger.LogAsync($"Role Deleted [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]", LogLvl.DEBUG);

            _client.RoleUpdated += async (role, role2) =>
                await ConsoleLogger.LogAsync($"Role Updated [Name: {role.Name} | New Name: {role2.Name} | ID: {role.Id} | Guild: {role.Guild}]",
                    LogLvl.TRACE);

            _client.UserBanned += async (user, guild) =>
                await ConsoleLogger.LogAsync($"User Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLvl.DEBUG);

            _client.UserUnbanned += async (user, guild) =>
                await ConsoleLogger.LogAsync($"User Un-Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]", LogLvl.DEBUG);

            _client.UserJoined += async user =>
                await ConsoleLogger.LogAsync($"User Joined Guild [User: {user} | User ID: {user.Id} | Guild: {user.Guild}]", LogLvl.DEBUG);

            _client.UserVoiceStateUpdated += async (user, vs1, vs2) =>
                await ConsoleLogger.LogAsync($"User Voice State Updated: [User: {user}]", LogLvl.TRACE);
        }
    }
}