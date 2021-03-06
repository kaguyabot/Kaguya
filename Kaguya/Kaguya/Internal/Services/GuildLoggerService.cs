﻿using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Kaguya.Internal.Events.ArgModels;
using Kaguya.Internal.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services
{
	public class GuildLoggerService
	{
		private readonly DiscordShardedClient _client;
		private readonly CommonEmotes _commonEmotes;
		private readonly ILogger<GuildLoggerService> _logger;
		private readonly IServiceProvider _serviceProvider;

		public GuildLoggerService(IServiceProvider serviceProvider, ILogger<GuildLoggerService> logger, DiscordShardedClient client,
			CommonEmotes commonEmotes)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
			_client = client;
			_commonEmotes = commonEmotes;
		}

		public async Task LogMessageDeletedAsync(Cacheable<IMessage, ulong> cache, ISocketMessageChannel textChannel)
		{
			var message = cache.Value;

			if (message is null || message.Author.IsBot)
			{
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(((SocketGuildChannel) textChannel).Guild.Id);
				var config = await logConfigurationRepository.GetOrCreateAsync(((SocketGuildChannel) textChannel).Guild.Id);

				if (!config.MessageDeleted.HasValue)
				{
					return;
				}

				string content = string.IsNullOrEmpty(message.Content) ? "<Message contained no text>" : $"{message.Content}";

				var sb = new StringBuilder($"🗑️ `[{GetFormattedTimestamp()}]` ");
				sb.Append($"Message deleted.\nAuthor: {message.Author.Mention}. Channel: {((SocketTextChannel) message.Channel).Mention}.");

				// Premium servers get more content in the log.
				if (server.IsPremium)
				{
					sb.Append($"\nContent: \"**{content}**\"");

					if (message.Attachments.Count > 0)
					{
						sb.Append($" Attachments: **{message.Attachments.Count}**.");
						foreach (var a in message.Attachments)
						{
							sb.Append($" URL: **<{a.ProxyUrl}>**");
						}
					}
				}

				var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta).WithDescription(sb.ToString());

				try
				{
					await _client.GetGuild(server.ServerId)
					             .GetTextChannel(config.MessageDeleted.Value)
					             .SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send message deleted log to channel {config.MessageDeleted.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.MessageDeleted = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogMessageUpdatedAsync(Cacheable<IMessage, ulong> cache, SocketMessage message, ISocketMessageChannel textChannel)
		{
			if (!cache.HasValue || !(textChannel is SocketGuildChannel channel))
			{
				return;
			}

			var oldMsg = cache.Value;

			if (oldMsg.Author.IsBot)
			{
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(channel.Guild.Id);
				var config = await logConfigurationRepository.GetOrCreateAsync(channel.Guild.Id);

				if (!config.MessageUpdated.HasValue)
				{
					return;
				}

				string content = oldMsg.Content;

				if (content == message.Content)
				{
					return;
				}

				if (string.IsNullOrWhiteSpace(content))
				{
					content = "<No previous text>";
				}

				var sb = new StringBuilder($"📝 `[{GetFormattedTimestamp()}]` ");
				sb.Append($"Message updated.\nAuthor: {oldMsg.Author.Mention}. Channel: {((SocketTextChannel) oldMsg.Channel).Mention}.");

				if (server.IsPremium)
				{
					string arg2Content = string.IsNullOrWhiteSpace(message.Content) ? "<No content>" : message.Content;
					sb.AppendLine($"\nOld Content:\n\"**{content}**\"");
					sb.Append($"New Content:\n\"**{arg2Content}**\"");
				}

				var embed = new KaguyaEmbedBuilder(KaguyaColors.Orange).WithDescription(sb.ToString());

				try
				{
					await _client.GetGuild(server.ServerId)
					             .GetTextChannel(config.MessageUpdated.Value)
					             .SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send message updated log to channel {config.MessageUpdated.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.MessageUpdated = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogUserJoinedAsync(SocketGuildUser user)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(user.Guild.Id);
				var config = await logConfigurationRepository.GetOrCreateAsync(user.Guild.Id);

				if (!config.UserJoins.HasValue)
				{
					return;
				}

				string msg =
					$"✅ `[{GetFormattedTimestamp()}]` {user.Mention} joined the server. Member Count: **{user.Guild.MemberCount:N0}**";

				var embed = new KaguyaEmbedBuilder(KaguyaColors.Green).WithDescription(msg);

				try
				{
					await _client.GetGuild(server.ServerId).GetTextChannel(config.UserJoins.Value).SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send user join log to channel {config.UserJoins.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.UserJoins = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogUserLeftAsync(SocketGuildUser user)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(user.Guild.Id);
				var config = await logConfigurationRepository.GetOrCreateAsync(user.Guild.Id);

				if (!config.UserLeaves.HasValue)
				{
					return;
				}

				string msg = $"{_commonEmotes.RedCrossEmote} `[{GetFormattedTimestamp()}]` {user.Mention} left the server. " +
				             $"Member Count: **{user.Guild.MemberCount:N0}**";

				var embed = new KaguyaEmbedBuilder(KaguyaColors.Red).WithDescription(msg);

				try
				{
					await _client.GetGuild(server.ServerId).GetTextChannel(config.UserLeaves.Value).SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send user leave log to channel {config.UserLeaves.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.UserLeaves = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogAntiRaidAsync(AdminAction action, SocketUser socketUser)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();
				var antiraidConfigRepository = scope.ServiceProvider.GetRequiredService<AntiraidConfigRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(action.ServerId);
				var config = await logConfigurationRepository.GetOrCreateAsync(action.ServerId);
				var arConfig = await antiraidConfigRepository.GetAsync(action.ServerId);

				if (arConfig == null || !config.Antiraids.HasValue)
				{
					return;
				}

				string punishString = AntiraidData.FormattedAntiraidPunishment(action.Action);
				string msg = $"🛡️ `[Anti-Raid]` `[{GetFormattedTimestamp()}]` {socketUser.Mention} was {punishString}.";
				var embed = new KaguyaEmbedBuilder(KaguyaColors.Blue).WithDescription(msg);

				try
				{
					await _client.GetGuild(action.ServerId).GetTextChannel(config.Antiraids.Value).SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send antiraid log to channel {config.Antiraids.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.Antiraids = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogUserBannedAsync(SocketUser bannedUser, SocketGuild guild)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(guild.Id);
				var config = await logConfigurationRepository.GetOrCreateAsync(guild.Id);

				if (!config.Bans.HasValue)
				{
					return;
				}

				string msg = $"⛔ `[{GetFormattedTimestamp()}]` {bannedUser.Mention} was banned from the server. " +
				             $"Member Count: **{guild.MemberCount - 1:N0}**"; // - 1 on count b/c guild is a cached object.

				var embed = new KaguyaEmbedBuilder(KaguyaColors.DarkRed).WithDescription(msg);

				try
				{
					await guild.GetTextChannel(config.Bans.Value).SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send ban log to channel {config.Bans.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.Bans = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogUserUnbannedAsync(SocketUser unbannedUser, SocketGuild guild)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(guild.Id);
				var config = await logConfigurationRepository.GetOrCreateAsync(guild.Id);

				if (!config.UnBans.HasValue)
				{
					return;
				}

				string msg = $"♻ `[{GetFormattedTimestamp()}]` {unbannedUser.Mention} has been unbanned.";
				var embed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow).WithDescription(msg);

				try
				{
					await guild.GetTextChannel(config.UnBans.Value).SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send unban log to channel {config.UnBans.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.UnBans = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState curVoiceState, SocketVoiceState nextVoiceState)
		{
			// We only log voice channel changes.
			if (user == null || (curVoiceState.VoiceChannel != null && curVoiceState.VoiceChannel.Equals(nextVoiceState.VoiceChannel)))
			{
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				KaguyaServer server;
				LogConfiguration config;
				if (curVoiceState.VoiceChannel is null)
				{
					server = await kaguyaServerRepository.GetOrCreateAsync(nextVoiceState.VoiceChannel.Guild.Id);
					config = await logConfigurationRepository.GetOrCreateAsync(nextVoiceState.VoiceChannel.Guild.Id);
				}
				else
				{
					server = await kaguyaServerRepository.GetOrCreateAsync(curVoiceState.VoiceChannel.Guild.Id);
					config = await logConfigurationRepository.GetOrCreateAsync(curVoiceState.VoiceChannel.Guild.Id);
				}

				if (!config.VoiceUpdates.HasValue)
				{
					return;
				}

				string changeString = ""; // User has...
				string emoji = string.Empty;

				if (curVoiceState.VoiceChannel is null)
				{
					emoji = "🎙️🟢"; // Green circle
					changeString = $"joined **{nextVoiceState.VoiceChannel.Name}**";
				}

				if (curVoiceState.VoiceChannel != null && nextVoiceState.VoiceChannel != null)
				{
					emoji = "🎙️🟡"; // Yellow circle.
					changeString = $"moved from **{curVoiceState.VoiceChannel.Name}** to **{nextVoiceState.VoiceChannel.Name}**";
				}

				if (nextVoiceState.VoiceChannel is null)
				{
					emoji = "🎙️🔴"; // Red circle.
					changeString = $"disconnected from **{curVoiceState.VoiceChannel.Name}**";
				}

				var sb = new StringBuilder($"{emoji} `[{GetFormattedTimestamp()}]` {user.Mention} ");
				sb.Append($"has {changeString}.");

				var embed = new KaguyaEmbedBuilder(KaguyaColors.IceBlue).WithDescription(sb.ToString());

				try
				{
					await _client.GetGuild(server.ServerId)
					             .GetTextChannel(config.VoiceUpdates.Value)
					             .SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send voice state log to channel {config.VoiceUpdates.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.VoiceUpdates = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		public async Task LogFilteredWordAsync(FilteredWordEventData fwData)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var kaguyaServerRepository = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();
				var logConfigurationRepository = scope.ServiceProvider.GetRequiredService<LogConfigurationRepository>();

				var server = await kaguyaServerRepository.GetOrCreateAsync(fwData.ServerId);
				var config = await logConfigurationRepository.GetOrCreateAsync(fwData.ServerId);

				if (!config.FilteredWord.HasValue)
				{
					return;
				}

				IUser author = _client.GetUser(fwData.UserId);

				var sb = new StringBuilder($"🛂 `[{GetFormattedTimestamp()}]` Filtered word detected from {author.Mention}. ");
				sb.Append($"Phrase: **{fwData.Phrase}**");

				if (server.IsPremium)
				{
					sb.Append($"\nMessage Contents: \"**{fwData.Message.Content}**\"");
				}

				var embed = new KaguyaEmbedBuilder(KaguyaColors.DarkBlue).WithDescription(sb.ToString());

				try
				{
					await _client.GetGuild(fwData.ServerId)
					             .GetTextChannel(config.FilteredWord.Value)
					             .SendMessageAsync(embed: embed.Build());
				}
				catch (Exception)
				{
					_logger.LogWarning($"Failed to send filtered phrase log to channel {config.FilteredWord.Value} " +
					                   $"in guild {server.ServerId}. Resetting this log channel to null so it " +
					                   "doesn't happen again!");

					config.FilteredWord = null;
					await logConfigurationRepository.UpdateAsync(config);
				}
			}
		}

		private static string GetFormattedTimestamp()
		{
			var d = DateTimeOffset.Now;
			var sb = new StringBuilder();

			sb.Append(d.Month.ToString("00") + "-");
			sb.Append(d.Day.ToString("00") + "-");
			sb.Append(d.Year.ToString("0000") + " ");
			sb.Append(d.Hour.ToString("00") + ":");
			sb.Append(d.Minute.ToString("00") + ":");
			sb.Append(d.Second.ToString("00"));

			return sb.ToString();
		}
	}
}