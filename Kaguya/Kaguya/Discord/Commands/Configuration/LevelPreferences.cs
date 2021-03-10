using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Interactivity;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Overrides.Extensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Configuration
{
	[Module(CommandModule.Configuration)]
	[Group("notifications")]
	[Alias("notifs")]
	[RequireUserPermission(GuildPermission.Administrator)]
	[Summary("Displays the currently configured level notification preferences for the server.")]
	public class LevelPreferences : KaguyaBase<LevelPreferences>
	{
		private static readonly List<string> _notifications = new()
		{
			"Levels: Server Only",
			"Levels: Global Only",
			"Levels: Server and Global",
			"Levels: Disabled"
		};
		private readonly CommonEmotes _commonEmotes;
		private readonly InteractivityService _interactivityService;
		private readonly KaguyaServerRepository _kaguyaServerRepository;
		private readonly ILogger<LevelPreferences> _logger;

		public LevelPreferences(ILogger<LevelPreferences> logger,
			KaguyaServerRepository kaguyaServerRepository,
			InteractivityService interactivityService,
			CommonEmotes commonEmotes) : base(logger)
		{
			_logger = logger;
			_kaguyaServerRepository = kaguyaServerRepository;
			_interactivityService = interactivityService;
			_commonEmotes = commonEmotes;
		}

		[Command]
		[InheritMetadata(CommandMetadata.Summary)]
		public async Task ToggleLevelsCommandAsync()
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			var sb = new StringBuilder();

			string enabledStr = "- Level announcements are ";

			var channel = Context.Guild.GetTextChannel(server.LevelAnnouncementsChannelId.GetValueOrDefault());

			if (channel == null)
			{
				enabledStr += "disabled".AsBold();
				if (server.LevelAnnouncementsChannelId.HasValue)
				{
					server.LevelAnnouncementsChannelId = null;
					await _kaguyaServerRepository.UpdateAsync(server);

					_logger.LogDebug($"Set server level notifications channel to null for guild {server.ServerId} - " +
					                 "currently set channel no longer exists.");
				}
			}
			else
			{
				enabledStr += $"set to {channel.Mention}";
			}

			sb.AppendLine(enabledStr);

			sb.AppendLine($"- Level preferences: {server.LevelNotifications.Humanize(LetterCasing.Title).AsBold()}");

			var embed = new KaguyaEmbedBuilder(KaguyaColors.ConfigurationColor)
			{
				Title = "Level Notification Preferences",
				Description = sb.ToString()
			}.WithFooter($"Change these preferences with {server.CommandPrefix}notifs -set");

			await SendEmbedAsync(embed);
		}

		[Command("-set", RunMode = RunMode.Async)]
		public async Task SetNotificationsCommandAsync()
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			var embed = new KaguyaEmbedBuilder(KaguyaColors.ConfigurationColor)
			{
				Title = "Notifications Setup",
				Description = "Type a response in chat below to set " +
				              "the notification preferences for this server.\n\n"
			}.WithFooter(
				$"Respond with only a number. Example: 1 | Current config: {server.LevelNotifications.Humanize(LetterCasing.Title)}");

			int c = 0;
			foreach (string n in _notifications)
			{
				c++;

				embed.Description += $"{c.ToString().AsBold()}. {n}\n";
			}

			var initEmbed = await SendEmbedAsync(embed);

			// Res needs to be casted into a LevelNotifications.

			int res = 0;
			int errCount = 0;

			InteractivityResult<SocketMessage> msg;
			while (true)
			{
				if (errCount == 2)
				{
					await SendBasicEmbedAsync("Too many failed attempts. Aborting...", Color.DarkMagenta);

					return;
				}

				msg = await _interactivityService.NextMessageAsync(
					x => int.TryParse(x.Content, out res) ||
					     x.Content.Contains("cancel", StringComparison.OrdinalIgnoreCase),
					timeout: TimeSpan.FromSeconds(60));

				if (msg.IsSuccess)
				{
					try
					{
						await msg.Value.DeleteAsync();
					}
					catch (Exception)
					{
						//
					}
				}

				if (res < 0 || res > _notifications.Count)
				{
					_interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel,
						deleteDelay: TimeSpan.FromSeconds(3),
						embed: GetBasicErrorEmbedBuilder("Invalid entry, please try again.").Build());

					errCount++;
				}
				else
				{
					break;
				}
			}

			if (msg.Value.Content.Contains("cancel", StringComparison.OrdinalIgnoreCase))
			{
				await initEmbed.ModifyAsync(x => x.Embed = new KaguyaEmbedBuilder(Color.Red)
				{
					Description = $"{_commonEmotes.RedCrossEmote} Operation cancelled."
				}.Build());
			}
			else
			{
				string successString = _notifications[res - 1].AsBold();

				await initEmbed.ModifyAsync(x => x.Embed = new KaguyaEmbedBuilder(KaguyaColors.Green)
				{
					Title = initEmbed.Embeds.First().Title,
					Description = $"Successfully set the notification preferences to {successString}"
				}.Build());
			}

			server.LevelNotifications = (LevelNotifications) res - 1;
			await _kaguyaServerRepository.UpdateAsync(server);

			if (server.LevelNotifications == LevelNotifications.Disabled)
			{
				return;
			}

			string modifyString;
			if (HasStaleNotifications(server))
			{
				modifyString = "There doesn't appear to be a " +
				               "channel for notifications to be sent to right now. " +
				               "Would you like to set one?".AsBold();
			}
			else
			{
				modifyString =
					$"Notifications are currently being sent to {Context.Guild.GetTextChannel(server.LevelAnnouncementsChannelId!.Value).Mention}.\n" +
					"Would you like to change where the level notifications are sent?".AsBold();
			}

			var resetEmbed = GetBasicEmbedBuilder(modifyString, KaguyaColors.ConfigurationColor);

			var confirmation =
				await _interactivityService.SendConfirmationAsync(resetEmbed, Context.Channel,
					TimeSpan.FromSeconds(60));

			if (confirmation.IsSuccess)
			{
				await SendBasicEmbedAsync("Where do you want to send the level notifications?\nExample: `#my-channel`",
					KaguyaColors.Purple);

				var response = await _interactivityService.NextMessageAsync(x => x.MentionedChannels.Any(),
					timeout: TimeSpan.FromSeconds(60));

				if (response.IsSuccess)
				{
					var channel = response.Value.MentionedChannels.FirstOrDefault();
					if (channel is not SocketTextChannel textChannel)
					{
						await SendBasicErrorEmbedAsync("Uh oh, I couldn't find a linked text channel. Aborting!");
						return;
					}

					await SendBasicSuccessEmbedAsync(
						$"Okay! I'll send all level notifications into {textChannel.Mention}.");

					server.LevelAnnouncementsChannelId = channel.Id;
					await _kaguyaServerRepository.UpdateAsync(server);
				}
			}
		}

		private bool HasStaleNotifications(KaguyaServer server)
		{
			var channel = Context.Guild.GetChannel(server.LevelAnnouncementsChannelId.GetValueOrDefault());
			return channel == null;
		}
	}
}