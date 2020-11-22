﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Kaguya.Database.Context;
using Kaguya.Database.Model;
using Kaguya.Discord.options;
using Kaguya.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaguya.Discord
{
	public class DiscordWorker : IHostedService
	{
		private readonly IOptions<AdminConfigurations> _adminConfigs;
		private readonly IOptions<DiscordConfigurations> _discordConfigs;
		private readonly ILogger<DiscordWorker> _logger;
		private readonly CommandService _commandService;
		private readonly KaguyaDbContext _dbContext;
		private readonly ServiceProvider _serviceProvider;
		private DiscordShardedClient _client;

		public DiscordWorker(IOptions<AdminConfigurations> adminConfigs, IOptions<DiscordConfigurations> discordConfigs,
			ILogger<DiscordWorker> logger,
			CommandService commandService, KaguyaDbContext dbContext, ServiceProvider serviceProvider)
		{
			_adminConfigs = adminConfigs;
			_discordConfigs = discordConfigs;
			_logger = logger;
			_commandService = commandService;
			_dbContext = dbContext;
			_serviceProvider = serviceProvider;
			// TODO: add emote type handler 
			// TODO: add socket guild user list type handler
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var restClient = new DiscordRestClient();
			await restClient.LoginAsync(TokenType.Bot, _discordConfigs.Value.BotToken);
			var shards = await restClient.GetRecommendedShardCountAsync();

			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = _discordConfigs.Value.AlwaysDownloadUsers ?? true,
				MessageCacheSize = _discordConfigs.Value.MessageCacheSize ?? 50,
				TotalShards = shards,
				LogLevel = LogSeverity.Debug
			});

			_client.Log += logMessage =>
			{
				_logger.Log(logMessage.Severity.ToLogLevel(), logMessage.Exception, logMessage.Message);
				return Task.CompletedTask;
			};

			InitLogging();

			InitCommands();

			await _client.LoginAsync(TokenType.Bot, _discordConfigs.Value.BotToken);

			await _client.StartAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _client.StopAsync();
			try
			{
				_client.Dispose();
			}
			catch (Exception ex)
			{
				_logger.Log(LogLevel.Error, ex, "Failed to dispose discord");
			}
		}

		#region Commands

		private void InitCommands()
		{
			_commandService.CommandExecuted += CommandExecutedAsync;
			_commandService.Log += logMessage =>
			{
				if (logMessage.Exception is CommandException cmdEx)
				{
					_logger.Log(LogLevel.Error, cmdEx, "Command exception encountered :(");
				}

				return Task.CompletedTask;
			};

			_client.MessageReceived += HandleCommandAsync;
		}

		private async Task HandleCommandAsync(SocketMessage msg)
		{
			if (!(msg is SocketUserMessage message) || message.Author.IsBot)
			{
				return;
			}

			if (message.Channel.GetType() != typeof(SocketTextChannel))
			{
				return;
			}

			if (!(message.Channel is SocketGuildChannel guildChannel))
			{
				return;
			}

			var server = await _dbContext.Servers.AsQueryable()
				.FirstOrDefaultAsync(s => s.ServerId == guildChannel.Guild.Id);
			if (server == null)
			{
				server = (await _dbContext.Servers.AddAsync(new KaguyaServer
				{
					ServerId = guildChannel.Guild.Id
				})).Entity;
			}

			var user = await _dbContext.Users.AsQueryable().FirstOrDefaultAsync(u => u.UserId == message.Author.Id);
			if (user == null)
			{
				user = (await _dbContext.Users.AddAsync(new KaguyaUser
				{
					UserId = message.Author.Id
				})).Entity;
			}

			if (user.UserId != _adminConfigs.Value.OwnerId)
			{
				if (await _dbContext.BlacklistedEntities.AsQueryable().AnyAsync(b =>
					new[]
					{
						user.UserId, server.ServerId
					}.Contains(b.EntityId)))
				{
					return;
				}
			}

			var commandCtx = new ShardedCommandContext(_client, message);

			if (await CheckFilteredPhrase(commandCtx, server, message))
			{
				return; // If filtered phrase (and user isn't admin), return.
			}

			// await ExperienceHandler.TryAddExp(user, server, commandCtx);
			// await ServerSpecificExperienceHandler.TryAddExp(user, server, commandCtx);

			// If the channel is blacklisted and the user isn't an Admin, return.
			if (!commandCtx.Guild.GetUser(commandCtx.User.Id).GuildPermissions.Administrator &&
			    await _dbContext.BlacklistedEntities.AsQueryable().AnyAsync(x =>
				    x.EntityId == commandCtx.Channel.Id && x.EntityType == BlacklistedEntityType.Channel))
			{
				return;
			}

			// Parsing of osu! beatmaps.
			if (server.OsuLinkParsingEnabled)
			{
				if (Regex.IsMatch(msg.Content,
					    @"https?://osu\.ppy\.sh/beatmapsets/[0-9]+#(?:osu|taiko|mania|fruits)/[0-9]+",
					    RegexOptions.IgnoreCase) ||
				    Regex.IsMatch(msg.Content, @"https?://osu\.ppy\.sh/b/[0-9]+"))
				{
					// TODO: implement
					// await AutomaticBeatmapLinkParserService.LinkParserMethod(msg, commandCtx);
				}
			}

			var argPos = 0;

			if (message.Author.IsBot ||
			    !(message.HasStringPrefix(server.CommandPrefix, ref argPos) ||
			      message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
			{
				return;
			}

			await _commandService.ExecuteAsync(commandCtx, argPos, _serviceProvider);
		}

		private async Task<bool> CheckFilteredPhrase(ICommandContext commandCtx, KaguyaServer server, IMessage message)
		{
			var userPerms = (await commandCtx.Guild.GetUserAsync(commandCtx.User.Id)).GuildPermissions;

			if (userPerms.Administrator)
				return false;

			var filters = await _dbContext.WordFilters.AsQueryable().Where(w => w.ServerId == server.ServerId)
				.ToListAsync();

			if (filters.Count == 0) return false;

			foreach (var filter in filters.Where(filter => FilterMatch(message.Content, filter.Word)))
			{
				await commandCtx.Channel.DeleteMessageAsync(message);
				_logger.Log(LogLevel.Information,
					$"Filtered phrase detected: [Guild: {server.ServerId} | Phrase: {filter.Word}]");

				// TODO: implement
				// var fpArgs = new FilteredPhraseEventArgs(server, filter.Word, message);
				// KaguyaEvents.TriggerFilteredPhrase(fpArgs);

				return true;
			}

			return false;
		}

		public static bool FilterMatch(string message, string pattern)
		{
			var (start, end) = (start: pattern.StartsWith("*"), end: pattern.EndsWith("*"));
			var wordlet = Regex.Escape(pattern.Substring((start ? 1 : 0),
				end
					? pattern.Length - (start ? 2 : 1)
					: pattern.Length - (start ? 1 : 0)));
			if (start)
			{
				wordlet = "[^ ]*" + wordlet;
			}

			if (end)
			{
				wordlet += "[^ ]*";
			}

			return Regex.IsMatch(message, $"(?:^|[ ]){wordlet}(?:$|[ ])", RegexOptions.IgnoreCase);
		}

		private async Task CommandExecutedAsync(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
		{
			// TODO: implement
			await Task.CompletedTask;
		}

		#endregion

		#region Logging

		private void InitLogging()
		{
			_client.ShardConnected += client =>
			{
				_logger.Log(LogLevel.Trace, $"Shard {client.ShardId} connected.");
				return Task.CompletedTask;
			};

			_client.ShardDisconnected += (ex, client) =>
			{
				_logger.Log(LogLevel.Error, ex,
					$"Shard {client.ShardId} disconnected");
				return Task.CompletedTask;
			};

			_client.ShardReady += client =>
			{
				_logger.Log(LogLevel.Information, $"Shard {client.ShardId} ready. Guilds: {client.Guilds.Count:N0}");
				return Task.CompletedTask;
			};

			_client.ShardLatencyUpdated += (oldLatency, newLatency, client) =>
			{
				_logger.Log(LogLevel.Trace,
					$"Shard {client.ShardId} latency has updated. [Old: {oldLatency}ms | New: {newLatency}ms]");
				return Task.CompletedTask;
			};

			_client.ChannelCreated += channel =>
			{
				_logger.Log(LogLevel.Debug,
					$"Channel Created [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]");
				return Task.CompletedTask;
			};

			_client.ChannelDestroyed += channel =>
			{
				_logger.Log(LogLevel.Debug,
					$"Channel Deleted [Name: #{(channel as SocketGuildChannel)?.Name} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]");
				return Task.CompletedTask;
			};

			_client.ChannelUpdated += (channel, channel2) =>
			{
				_logger.Log(LogLevel.Trace,
					$"Channel Updated [Name: #{channel} | New Name: #{channel2} | ID: {channel.Id} | Guild: {(channel as SocketGuildChannel)?.Guild}]");
				return Task.CompletedTask;
			};

			_client.JoinedGuild += guild =>
			{
				_logger.Log(LogLevel.Information, $"Joined Guild [Name: {guild.Name} | ID: {guild.Id}]");
				return Task.CompletedTask;
			};

			_client.LeftGuild += guild =>
			{
				_logger.Log(LogLevel.Information, $"Left Guild [Name: {guild.Name} | ID: {guild.Id}]");
				return Task.CompletedTask;
			};

			_client.MessageDeleted += (cache, _) =>
			{
				if (cache.Value is null) return Task.CompletedTask;
				_logger.Log(LogLevel.Trace, $"Message Deleted [Author: {cache.Value.Author} | ID: {cache.Id}]");
				return Task.CompletedTask;
			};

			_client.MessageUpdated += (cache, msg, _) =>
			{
				if (cache.Value is null) return Task.CompletedTask;
				if (cache.Value.Embeds != null && cache.Value.Content == msg.Content) return Task.CompletedTask;
				_logger.Log(LogLevel.Trace, $"Message Updated [Author: {cache.Value.Author} | ID: {cache.Id}]");
				return Task.CompletedTask;
			};

			_client.MessageReceived += msg =>
			{
				_logger.Log(LogLevel.Trace,
					$"Message Received [Author: {msg.Author} | ID: {msg.Id} | Bot: {msg.Author.IsBot}]");
				return Task.CompletedTask;
			};

			_client.RoleCreated += role =>
			{
				_logger.Log(LogLevel.Debug,
					$"Role Created [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]");
				return Task.CompletedTask;
			};

			_client.RoleDeleted += role =>
			{
				_logger.Log(LogLevel.Debug,
					$"Role Deleted [Name: {role.Name} | Role ID: {role.Id} | Guild: {role.Guild}]");
				return Task.CompletedTask;
			};

			_client.RoleUpdated += (role, role2) =>
			{
				_logger.Log(LogLevel.Trace,
					$"Role Updated [Name: {role.Name} | New Name: {role2.Name} | ID: {role.Id} | Guild: {role.Guild}]");
				return Task.CompletedTask;
			};

			_client.UserBanned += (user, guild) =>
			{
				_logger.Log(LogLevel.Debug,
					$"User Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]");
				return Task.CompletedTask;
			};

			_client.UserUnbanned += (user, guild) =>
			{
				_logger.Log(LogLevel.Debug,
					$"User Un-Banned [User: {user} | User ID: {user.Id} | Guild: {guild.Name}]");
				return Task.CompletedTask;
			};

			_client.UserJoined += user =>
			{
				_logger.Log(LogLevel.Debug,
					$"User Joined Guild [User: {user} | User ID: {user.Id} | Guild: {user.Guild}]");
				return Task.CompletedTask;
			};

			_client.UserVoiceStateUpdated += (user, _, _) =>
			{
				_logger.Log(LogLevel.Trace, $"User Voice State Updated: [User: {user}]");
				return Task.CompletedTask;
			};
		}

		#endregion
	}
}