using Discord;
using Discord.Commands;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Configuration
{
	[Module(CommandModule.Configuration)]
	[Group("autorole")]
	[Alias("atr")]
	[RequireUserPermission(ChannelPermission.ManageRoles)]
	[RequireBotPermission(ChannelPermission.ManageRoles)]
	public class AutoRoleCommand : KaguyaBase<AutoRoleCommand>
	{
		private const int MAX_ROLES_REG = 1;
		private const int MAX_ROLES_PREM = 5;
		private readonly AutoRoleRepository _autoRoleRepository;
		private readonly CommonEmotes _commonEmotes;
		private readonly KaguyaServerRepository _kaguyaServerRepository;
		private readonly ILogger<AutoRoleCommand> _logger;

		public AutoRoleCommand(ILogger<AutoRoleCommand> logger, KaguyaServerRepository kaguyaServerRepository,
			AutoRoleRepository autoRoleRepository, CommonEmotes commonEmotes) : base(logger)
		{
			_logger = logger;
			_kaguyaServerRepository = kaguyaServerRepository;
			_autoRoleRepository = autoRoleRepository;
			_commonEmotes = commonEmotes;
		}

		[Command]
		[Summary("Adds the specified role(s) to the auto-role service. \"Auto Roles\" are roles that are automatically assigned " +
		         "as soon as the user joins the server. Non-premium servers are limited to one role. Premium servers " +
		         "may assign up to five roles.")]
		[Remarks("<role> {...}")]
		[Example("\"My special role\"")]
		[Example("@My Special Role")]
		[Example("Restricted \"Needs verification\"")]
		public async Task AutoRoleCommandAsync(params IRole[] roles)
		{
			if (roles.Length < 1)
			{
				await SendBasicErrorEmbedAsync("You must specify at least one role.");
				return;
			}

			var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);
			int curAutoRoles = await _autoRoleRepository.CountAsync(Context.Guild.Id);

			if ((server.IsPremium && (curAutoRoles + roles.Length) > MAX_ROLES_PREM) ||
			    (!server.IsPremium && (curAutoRoles + roles.Length) > MAX_ROLES_REG))
			{
				string s = curAutoRoles == 1 ? "" : "s";
				await SendBasicErrorEmbedAsync("You are trying to add too many auto-roles. This server already has " +
				                               $"{curAutoRoles} active auto role{s}. Please delete one and try again.");

				return;
			}

			var successSb = new StringBuilder();
			var errorSb = new StringBuilder();
			foreach (var role in roles)
			{
				if (role.IsManaged)
				{
					errorSb.AppendLine($"{_commonEmotes.RedCrossEmote} {role.Mention} is managed by Discord or a Bot " +
					                   "and cannot be given to users.");

					continue;
				}

				var existingMatch = await _autoRoleRepository.GetAsync(role.Id);
				if (existingMatch != null)
				{
					errorSb.AppendLine($"{_commonEmotes.RedCrossEmote} {role.Mention} is already configured as an auto role.");
					continue;
				}

				var autoRole = new AutoRole
				{
					ServerId = Context.Guild.Id,
					RoleId = role.Id
				};

				await _autoRoleRepository.InsertAsync(autoRole);
				successSb.AppendLine($"{_commonEmotes.CheckMarkEmoji} {role.Mention}");
			}

			DetermineStatus(successSb, errorSb, out bool success, out bool error);

			var finalSb = new StringBuilder();
			if (success)
			{
				finalSb.AppendLine("Configured Auto Roles:".AsBoldUnderlined()).AppendLine(successSb.ToString());
			}

			if (error)
			{
				finalSb.AppendLine("Errors:".AsBoldUnderlined()).Append(errorSb);
			}

			var color = GetEmbedColor(success, error);
			var embed = new KaguyaEmbedBuilder(color).WithDescription(finalSb.ToString())
			                                         .WithFooter("Ensure the 'Kaguya' role is at the top of the Role " +
			                                                     "hierarchy for the auto-role service to work!!");

			await SendEmbedAsync(embed);
		}

		[Command("-r")]
		[Summary("Removes the specified role(s) from the auto-role service.")]
		[Remarks("<role> {...}")]
		[Example("\"My special role\"")]
		[Example("@My Special Role @MyRole2")]
		[Example("Restricted \"Needs verification\" @Penguins")]
		public async Task RemoveAutoRoleCommandAsync(params IRole[] roles)
		{
			if (roles.Length < 1)
			{
				await SendBasicErrorEmbedAsync("You must specify at least one role.");
				return;
			}

			var curRoles = await _autoRoleRepository.GetAllAsync(Context.Guild.Id);

			var successSb = new StringBuilder();
			var errorSb = new StringBuilder();

			foreach (var role in roles)
			{
				var match = curRoles.FirstOrDefault(x => x.RoleId == role.Id);
				if (match == null)
				{
					errorSb.AppendLine($"{_commonEmotes.RedCrossEmote} {role.Mention} is not listed as an auto-role.");
				}
				else
				{
					try
					{
						await _autoRoleRepository.DeleteByRoleIdAsync(role.Id);
						successSb.AppendLine($"{_commonEmotes.CheckMarkEmoji} {role.Mention}");
					}
					catch (Exception e)
					{
						_logger.LogWarning(e, $"Failed to delete auto role {role.Id} from guild {Context.Guild.Id}");
						errorSb.AppendLine($"{_commonEmotes.RedCrossEmote} Failed to remove {role.Mention}. Error: {e.Message}");
					}
				}
			}

			var finalSb = new StringBuilder();
			DetermineStatus(successSb, errorSb, out bool success, out bool error);

			if (success)
			{
				finalSb.AppendLine("Removed Auto Roles:".AsBoldUnderlined());
				finalSb.AppendLine(successSb.ToString());
			}

			if (error)
			{
				finalSb.AppendLine("Errors:".AsBoldUnderlined());
				finalSb.AppendLine(errorSb.ToString());
			}

			var color = GetEmbedColor(success, error);
			var embed = new KaguyaEmbedBuilder(color).WithDescription(finalSb.ToString()).Build();

			await SendEmbedAsync(embed);
		}

		[Command("-v")]
		[Summary("Displays the currently configured auto role(s).")]
		public async Task ViewAutoRolesCommandAsync()
		{
			var allRoles = await _autoRoleRepository.GetAllAsync(Context.Guild.Id);
			if (allRoles.Count == 0)
			{
				await SendBasicEmbedAsync("The server does not have any auto-roles configured.", KaguyaColors.ConfigurationColor);
				return;
			}

			var successSb = new StringBuilder();
			var errorSb = new StringBuilder();

			foreach (var autoRole in allRoles)
			{
				var guildRole = Context.Guild.GetRole(autoRole.RoleId);
				if (guildRole == null)
				{
					await _autoRoleRepository.DeleteAsync(autoRole.Id);
					errorSb.AppendLine($"{_commonEmotes.RedCrossEmote} Role with ID {autoRole.RoleId} is missing from the server! " +
					                   "I've gone ahead and automatically removed this auto-role for you.");
				}
				else
				{
					successSb.AppendLine($"{_commonEmotes.CheckMarkEmoji} {guildRole.Mention} is active.");
				}
			}

			var finalSb = new StringBuilder();
			DetermineStatus(successSb, errorSb, out bool success, out bool error);

			if (success)
			{
				finalSb.AppendLine("Active Auto Roles".AsBoldUnderlined());
				finalSb.AppendLine(successSb.ToString());
			}

			if (error)
			{
				finalSb.AppendLine("Errors".AsBoldUnderlined());
				finalSb.AppendLine(errorSb.ToString());
			}

			var color = GetEmbedColor(success, error);
			var embed = new KaguyaEmbedBuilder(color).WithDescription(finalSb.ToString());

			await SendEmbedAsync(embed);
		}

		private Color GetEmbedColor(bool success, bool error)
		{
			return success && error ? KaguyaColors.Magenta : success ? KaguyaColors.ConfigurationColor : KaguyaColors.Red;
		}

		private void DetermineStatus(StringBuilder successBuilder, StringBuilder errorBuilder, out bool success, out bool error)
		{
			success = false;
			error = false;

			if (!string.IsNullOrWhiteSpace(successBuilder.ToString()))
			{
				success = true;
			}

			if (!string.IsNullOrWhiteSpace(errorBuilder.ToString()))
			{
				error = true;
			}
		}
	}
}