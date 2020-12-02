using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Kaguya.Database.Context;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.options;
using Kaguya.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaguya.Discord
{
	public class DiscordWorker : IHostedService
	{
		private readonly IOptions<AdminConfigurations> _adminConfigs;
		private readonly IOptions<DiscordConfigurations> _discordConfigs;
		private readonly ILogger<DiscordWorker> _logger;
		private readonly CommandService _commandService;
		private readonly KaguyaDbContext _dbContext;
		private readonly IServiceProvider _serviceProvider;
		private readonly DiscordShardedClient _client;

		public DiscordWorker(DiscordShardedClient client, IOptions<AdminConfigurations> adminConfigs, IOptions<DiscordConfigurations> discordConfigs,
			ILogger<DiscordWorker> logger, CommandService commandService, DbContextOptions<KaguyaDbContext> dbContextOptions, 
			IServiceProvider serviceProvider)
		{
			_client = client;
			_adminConfigs = adminConfigs;
			_discordConfigs = discordConfigs;
			_logger = logger;
			_commandService = commandService;
			_dbContext = new KaguyaDbContext(dbContextOptions);
			_serviceProvider = serviceProvider;
			// TODO: add emote type handler 
			// TODO: add socket guild user list type handler
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), scope.ServiceProvider);
			}
			
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
					_logger.Log(LogLevel.Error, cmdEx, $"Exception encountered when executing command. Message: {logMessage.Message}");
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
				
				await _dbContext.SaveChangesAsync();
			}

			var user = await _dbContext.Users.AsQueryable().FirstOrDefaultAsync(u => u.UserId == message.Author.Id);
			if (user == null)
			{
				user = (await _dbContext.Users.AddAsync(new KaguyaUser
				{
					UserId = message.Author.Id
				})).Entity;

				await _dbContext.SaveChangesAsync();
			}

			if (user.UserId != _adminConfigs.Value.OwnerId)
			{
				if (await _dbContext.BlacklistedEntities.AsQueryable().AnyAsync(b =>
					new []
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

			// TODO: Implement experience handlers.
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

			using (var commandScope = _serviceProvider.CreateScope()) {
				await _commandService.ExecuteAsync(commandCtx, argPos, commandScope.ServiceProvider);
			}
		}

		private async Task<bool> CheckFilteredPhrase(ICommandContext commandCtx, KaguyaServer server, IMessage message)
		{
			var userPerms = (await commandCtx.Guild.GetUserAsync(commandCtx.User.Id)).GuildPermissions;

			if (userPerms.Administrator)
				return false;

			var filters = await _dbContext.FilteredWords.AsQueryable().Where(w => w.ServerId == server.ServerId)
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
			var (start, end) = (pattern.StartsWith("*"), pattern.EndsWith("*"));
			var wordlet = Regex.Escape(pattern.Substring(start ? 1 : 0,
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

		private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext ctx, IResult result)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var provider = scope.ServiceProvider;
				
				var ksRepo = provider.GetRequiredService<KaguyaServerRepository>();
				// var userRepo = provider.GetService<KaguyaUserRepository>();
				// var chRepo = provider.GetService<CommandHistoryRepository>();

				if (!command.IsSpecified)
				{
					return;
				}

				var server = await ksRepo.GetOrCreateAsync(ctx.Guild.Id);
				int guildShard = _client.GetShardIdFor(ctx.Guild);

				CommandHistory ch = null;
				if (command.GetValueOrDefault() != null)
				{
					ch = new CommandHistory 
					     {
						     UserId = ctx.User.Id, 
						     ServerId = ctx.Guild.Id, 
						     CommandName = command.Value.GetFullCommandName(), 
						     Message = ctx.Message.Content, 
						     ExecutedSuccessfully = true, 
						     ExecutionTime = DateTime.Now
					     };
				}
					
				if (result.IsSuccess)
				{
					//KaguyaUser user = await userRepo.GetOrCreateAsync(ctx.User.Id);

					//user.ActiveRateLimit++;
					server.TotalCommandCount++;

					var logCtxSb = new StringBuilder();

					logCtxSb.AppendLine($"Command Executed [Name: {command.Value.Name} | Message: {ctx.Message}]");
					logCtxSb.AppendLine($"User [Name: {ctx.User} | ID: {ctx.User.Id}]");
					logCtxSb.AppendLine($"Guild [Name: {ctx.Guild} | ID: {ctx.Guild.Id} | Shard: {guildShard:N0}]");
					logCtxSb.AppendLine($"Channel [Name: {ctx.Channel} | ID: {ctx.Channel.Id}]");

					_logger.LogInformation(logCtxSb.ToString());
				}
				else
				{
					var logErrorSb = new StringBuilder();
				
					logErrorSb.AppendLine($"Command Failed [Message: {ctx.Message}]");
					logErrorSb.AppendLine($"User [Name: {ctx.User} | ID: {ctx.User.Id}]");
					logErrorSb.AppendLine($"Guild [Name: {ctx.Guild} | ID: {ctx.Guild.Id} | Shard: {guildShard:N0}]");
					logErrorSb.AppendLine($"Channel [Name: {ctx.Channel} | ID: {ctx.Channel.Id}]");
				
					_logger.LogDebug(logErrorSb.ToString());
				
					// We don't want to spam users with "Unknown command" if they are invoking a 
					// command from another bot with the same prefix.
					if (result.Error != CommandError.UnknownCommand)
					{
						if (ch != null)
						{
							ch.ExecutedSuccessfully = false;
						}
						
						try
						{
							await ctx.Channel.SendMessageAsync($"{ctx.User.Mention} There was an error executing the command {command.Value.Module.Name}.\n" +
							                                   $"Please use `{server.CommandPrefix}help {command.Value.Module.Name}` for " +
							                                   $"instructions on how to use this command.");
						}
						catch (HttpException e)
						{
							// TODO: Implement auto-eject.
							// We auto-eject from guilds that don't give us permission to respond to command errors.
						}
						catch (Exception)
						{
							_logger.LogError($"Failed to send message in guild {ctx.Guild.Id} due to an exception.");
						}
					}
				}

				if (ch != null)
				{
					_dbContext.CommandHistories.Add(ch);
				}
				
				await _dbContext.SaveChangesAsync();
			}
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