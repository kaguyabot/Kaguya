using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Interactivity;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Overrides.Extensions;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Exceptions;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Administration
{
	[Module(CommandModule.Administration)]
	[Group("shadowban")]
	[Alias("sb")]
	[RequireUserPermission(GuildPermission.Administrator)]
	[RequireBotPermission(GuildPermission.Administrator)]
	public class Shadowban : KaguyaBase<Shadowban>
	{
		private const string ROLE_NAME = "kaguya-shadowban";
		private readonly AdminActionRepository _adminActionRepository;
		private readonly InteractivityService _interactivityService;
		private readonly KaguyaServerRepository _kaguyaServerRepository;
		private readonly ILogger<Shadowban> _logger;

		public Shadowban(ILogger<Shadowban> logger,
			AdminActionRepository adminActionRepository,
			KaguyaServerRepository kaguyaServerRepository,
			InteractivityService interactivityService) : base(logger)
		{
			_logger = logger;
			_adminActionRepository = adminActionRepository;
			_kaguyaServerRepository = kaguyaServerRepository;
			_interactivityService = interactivityService;
		}

		[Command(RunMode = RunMode.Async)]
		[Summary("Shadowbans a user indefinitely with an optional reason. The shadowban persists until the " +
		         "user is unshadowbanned via the `shadowban -u` command. If the shadowban role is manually removed from the user " +
		         "by a moderator, the shadowban will not be automatically reapplied.")]
		[Remarks("<user> [reason]")]
		[Example("@User#0000 Being really spammy in chat.")]
		public async Task ShadowbanCommandAsync(SocketGuildUser user,
			[Remainder]
			string reason = null)
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			await ShadowbanUserAsync(user, null, reason, server);
		}

		[Command("-t", RunMode = RunMode.Async)]
		[Summary("Shadowbans a user for a specified duration, with an optional reason.")]
		[Remarks("<user> <duration> [reason]")]
		[Example("@User#0000 30m Being really spammy in chat.")]
		[Example("@User#0000 1d16h35m25s")]
		// ReSharper disable once MethodOverloadWithOptionalParameter
		public async Task ShadowbanCommandAsync(SocketGuildUser user,
			string duration,
			[Remainder]
			string reason = null)
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

			var timeParser = new TimeParser(duration);
			var parsedDuration = timeParser.ParseTime();

			if (parsedDuration == TimeSpan.Zero)
			{
				throw new TimeParseException(duration);
			}

			DateTimeOffset? shadowbanExpiration = DateTimeOffset.Now.Add(parsedDuration);

			await ShadowbanUserAsync(user, shadowbanExpiration, reason, server);
		}

		[Command("-u")]
		[Summary("Unshadowbands a user.")]
		[Remarks("<user>")]
		public async Task UnShadowbanUserCommandAsync(SocketGuildUser user)
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			var shadowbanRole = await GetShadowbanRoleAsync(server);
			bool isShadowbanned = await UserIsCurrentlyShadowbannedAsync(user, shadowbanRole);

			if (!isShadowbanned)
			{
				await SendBasicErrorEmbedAsync("This user is not shadowbanned.");

				return;
			}

			var allUserShadowbans = await GetUnexpiredShadowbansAsync(user.Id, server.ServerId);
			await _adminActionRepository.ForceExpireRangeAsync(allUserShadowbans);

			// Remove shadowban role from user, if applicable.
			if (user.Roles.Any(x => x.Id == shadowbanRole.Id))
			{
				try
				{
					await user.RemoveRoleAsync(shadowbanRole);
				}
				catch (Exception e)
				{
					await SendBasicErrorEmbedAsync(
						$"An error occurred when trying to remove {user.Mention}'s shadowban role:\n" +
						$"Error message: {e.Message.AsBold()}");

					return;
				}
			}

			await SendBasicSuccessEmbedAsync($"Unshadowbanned user {user.Mention}.");
		}

		[Command("-sync")]
		[Summary("Syncs all text, voice, and category channel permissions for this server's shadowban role. " +
		         "This should be used after adding new public channels so that shadowbanned users won't be able to type in them.")]
		public async Task ShadowbanSyncCommandAsync()
		{
			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			var shadowbanRole = await GetShadowbanRoleAsync(server);
			var embedFields = await SetShadowbanPermissionsAsync(shadowbanRole);

			if (!embedFields.Any())
			{
				await SendBasicErrorEmbedAsync("All shadowban permissions are already synced!");

				return;
			}

			var embed =
				GetBasicSuccessEmbedBuilder($"Synced permissions for the shadowban role {shadowbanRole.Mention}.")
					.WithFields(embedFields)
					.Build();

			await SendEmbedAsync(embed);
		}

		private async Task ShadowbanUserAsync(SocketGuildUser user,
			DateTimeOffset? expiration,
			string reason,
			KaguyaServer server)
		{
			var adminAction = new AdminAction
			{
				ServerId = Context.Guild.Id,
				ModeratorId = Context.User.Id,
				ActionedUserId = user.Id,
				Action = AdminAction.ShadowbanAction,
				Reason = reason,
				Expiration = expiration,
				Timestamp = DateTimeOffset.Now
			};

			bool shadowbanRoleExists = DetermineIfShadowbanRoleExists(server);
			bool updateServer = false;

			var shadowbanRole = await GetShadowbanRoleAsync(server);

			// We want to confirm with the user whether they want to overwrite the existing shadowban or leave the existing one.
			if (await UserIsCurrentlyShadowbannedAsync(user, shadowbanRole))
			{
				await SendConfirmationMessageAsync(user, expiration);
			}

			await _adminActionRepository.InsertAsync(adminAction); // Very earliest we can insert to DB.

			List<EmbedFieldBuilder> permissionFields = new();

			try
			{
				permissionFields = await SetShadowbanPermissionsAsync(shadowbanRole);
			}
			catch (Exception e)
			{
				await SendBasicErrorEmbedAsync(
					"Warning: Failed to complete permission overwrite execution process. This error occurs from a " +
					$"lack of permissions.\n\nError: {e.Message.AsBold()}\n\n" +
					"The shadowban operation will still continue. " +
					"Use the ".AsBold() +
					"shadowban -sync".AsCodeBlockSingleLine().AsBold() +
					" " +
					"command after updating my permissions to continue.".AsBold());
			}

			try
			{
				await user.AddRoleAsync(shadowbanRole);
			}
			catch (Exception e)
			{
				await SendBasicErrorEmbedAsync(
					$"Failed to add role {shadowbanRole.ToString().AsBold()} to user {user.ToString().AsBold()}.\nReason: {e.ToString().AsBold()}");

				return;
			}

			if (!shadowbanRoleExists)
			{
				updateServer = true;
				server.ShadowbanRoleId = shadowbanRole.Id;
			}

			if (updateServer)
			{
				await _kaguyaServerRepository.UpdateAsync(server);
			}

			var embed = GetFinalEmbed(user, expiration, reason, permissionFields);
			await SendEmbedAsync(embed);
		}

		private async Task SendConfirmationMessageAsync(SocketGuildUser user, DateTimeOffset? expiration)
		{
			var currentUserShadowbans = await GetUnexpiredShadowbansAsync(user.Id, Context.Guild.Id);

			if (!currentUserShadowbans.Any())
			{
				return;
			}

			bool permanentShadowbans = currentUserShadowbans.Any(x => !x.Expiration.HasValue);

			if (!permanentShadowbans)
			{
				currentUserShadowbans = currentUserShadowbans.OrderByDescending(x => x.Expiration ?? DateTime.MinValue)
				                                             .ToList();
			}

			var longestShadowban = permanentShadowbans
				? currentUserShadowbans.First(x => !x.Expiration.HasValue)
				: currentUserShadowbans[0];

			if (longestShadowban == null)
			{
				return;
			}

			string oldShadowbanDurationStr =
				(permanentShadowbans ? "never".AsBold() : longestShadowban.Expiration?.Humanize())
				.Humanize(LetterCasing.Sentence)
				.AsBold();

			string newShadowbanDurationStr = (!expiration.HasValue
				? "permanent"
				: (expiration.Value - DateTimeOffset.Now).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day) +
				  " from now").AsBold();

			string reasonStr = (longestShadowban.Reason ?? "<No reason provided>").AsItalics();

			var oldMod = Context.Guild.GetUser(longestShadowban.ModeratorId);

			var overwriteEmbed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
			{
				Description =
					"This user is already shadowbanned. Would you like to overwrite their current shadowban? Details of " +
					"the current shadowbans are described below:\n" +
					$"- Expiration: [current: {oldShadowbanDurationStr} | new: {newShadowbanDurationStr}]\n" +
					$"- Reason: {reasonStr}\n" +
					"- Moderator: " +
					(oldMod?.Mention ?? "Not found".AsItalics()) +
					"\n\n" +
					"Response will expire in 60 seconds, defaulting to ✅.".AsItalics() +
					"\n" +
					"Note: Overwriting does not erase shadowban history.".AsItalics() +
					"\n\n" +
					"✅ = Replace old duration with new. (default)\n" +
					"❌ = Don't replace old. User will be unshadowbanned at latest possible time."
			};

			var result =
				await _interactivityService.SendConfirmationAsync(overwriteEmbed, Context.Channel,
					TimeSpan.FromSeconds(60));

			// If the user wants to overwrite...
			if (result.Value)
			{
				// We force expire as we want to keep the shadowban reason history.
				// Forcing expiration ensures it won't be actioned on by any background services.
				await _adminActionRepository.ForceExpireRangeAsync(currentUserShadowbans);
				await SendBasicSuccessEmbedAsync(
					"Okay, I'll replace the old shadowban duration with the one you just provided.");
			}
			else
			{
				await SendBasicSuccessEmbedAsync(
					"Okay, I'll insert this and log it, but if the user currently has shadowbans that expire later " +
					"than what you provided, they will be unshadowbanned at that time.\n" +
					"Use the `shadowban -status` command to view this information.");
			}
		}

		private bool DetermineIfShadowbanRoleExists(KaguyaServer server)
		{
			if (!server.ShadowbanRoleId.HasValue)
			{
				return false;
			}

			var shadowbanRole = Context.Guild.GetRole(server.ShadowbanRoleId.Value);
			return shadowbanRole != null;
		}

		private async Task<IRole> GetShadowbanRoleAsync(KaguyaServer server)
		{
			var match = Context.Guild.GetRole(server.ShadowbanRoleId ?? 0) ?? await CreateShadowbanRoleAsync();
			return match;
		}

		private async Task<IRole> CreateShadowbanRoleAsync()
		{
			_logger.LogDebug(
				$"Shadowban role created in guild {Context.Guild.Id}. Guild roles: {Context.Guild.Roles.Humanize()}");

			return await Context.Guild.CreateRoleAsync(ROLE_NAME, GuildPermissions.None, KaguyaColors.Default, false,
				false);
		}

		private async Task<List<EmbedFieldBuilder>> SetShadowbanPermissionsAsync(IRole role)
		{
			List<EmbedFieldBuilder> fieldBuilders = new();
			List<SocketTextChannel> textChannelsToUpdate = new();
			List<SocketCategoryChannel> categoriesToUpdate = new();
			List<SocketVoiceChannel> voiceChannelsToUpdate = new();

			foreach (var textChannel in Context.Guild.TextChannels)
			{
				if (!textChannel.GetPermissionOverwrite(role).HasValue)
				{
					textChannelsToUpdate.Add(textChannel);
				}
			}

			foreach (var category in Context.Guild.CategoryChannels)
			{
				if (!category.GetPermissionOverwrite(role).HasValue)
				{
					categoriesToUpdate.Add(category);
				}
			}

			foreach (var vc in Context.Guild.VoiceChannels)
			{
				if (!vc.GetPermissionOverwrite(role).HasValue)
				{
					voiceChannelsToUpdate.Add(vc);
				}
			}

			if (textChannelsToUpdate.Any())
			{
				string s = textChannelsToUpdate.Count == 1 ? default : "s";
				fieldBuilders.Add(new EmbedFieldBuilder
				{
					Name = "Channel Permissions",
					Value =
						$"Denied all permissions for role {role.ToString().AsBold()} in {textChannelsToUpdate.Count.ToString("N0").AsBold()} text channel{s}."
				});
			}

			if (categoriesToUpdate.Any())
			{
				string s = textChannelsToUpdate.Count == 1 ? default : "s";
				fieldBuilders.Add(new EmbedFieldBuilder
				{
					Name = "Category Permissions",
					Value =
						$"Denied all permissions for role {role.ToString().AsBold()} in {categoriesToUpdate.Count.ToString("N0").AsBold()} categorie{s}."
				});
			}

			if (voiceChannelsToUpdate.Any())
			{
				string s = textChannelsToUpdate.Count == 1 ? default : "s";
				fieldBuilders.Add(new EmbedFieldBuilder
				{
					Name = "Voice Channel Permissions",
					Value =
						$"Denied all permissions for role {role.ToString().AsBold()} in {voiceChannelsToUpdate.Count.ToString("N0").AsBold()} voice channel{s}."
				});
			}

			List<IGuildChannel> finalCollection = new();

			finalCollection.AddRange(textChannelsToUpdate);
			finalCollection.AddRange(voiceChannelsToUpdate);
			finalCollection.AddRange(categoriesToUpdate);

			if (finalCollection.Any())
			{
				await ReplyAsync($"{Context.User.Mention} Processing channel permissions. Please wait...");
				await Task.Run(async () =>
				{
					foreach (var channel in finalCollection)
					{
						await channel.AddPermissionOverwriteAsync(role, GetShadowbanOverwritePermissions(channel));
					}
				});
			}

			return fieldBuilders;
		}

		/// <summary>
		///  A <see cref="OverwritePermissions" /> for any shadowban role created by Kaguya.
		/// </summary>
		/// <returns></returns>
		public static OverwritePermissions GetShadowbanOverwritePermissions(IGuildChannel channel)
		{
			return OverwritePermissions.DenyAll(channel);
		}

		private async Task<bool> UserIsCurrentlyShadowbannedAsync(SocketGuildUser user, IRole shadowbanRole)
		{
			if (user.Roles.Any(x => x.Id == shadowbanRole.Id))
			{
				return true;
			}

			// unexpired = null expiration or expiration that has not already expired.
			var unexpiredUserShadowbans = await GetUnexpiredShadowbansAsync(user.Id, Context.Guild.Id);

			return unexpiredUserShadowbans.Any();
		}

		private async Task<IList<AdminAction>> GetUnexpiredShadowbansAsync(ulong userId, ulong serverId)
		{
			return await _adminActionRepository.GetAllUnexpiredAsync(userId, serverId, AdminAction.ShadowbanAction);
		}

		private Embed GetFinalEmbed(SocketGuildUser target,
			DateTimeOffset? expiration,
			string reason,
			List<EmbedFieldBuilder> fields)
		{
			string durationStr = expiration.HasValue
				? $" for {(expiration.Value - DateTimeOffset.Now).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day).AsBold()}"
				: string.Empty;

			string reasonStr = reason == null ? "<No reason provided>".AsBold() : reason.AsBold();

			return new KaguyaEmbedBuilder(KaguyaColors.Purple)
			       .WithDescription($"{Context.User.Mention} Shadowbanned user {target.Mention}{durationStr}." +
			                        $"\nReason: {reasonStr}")
			       .WithFooter("To unshadowban this user, use the shadowban -u command.")
			       .WithFields(fields)
			       .Build();
		}
	}
}