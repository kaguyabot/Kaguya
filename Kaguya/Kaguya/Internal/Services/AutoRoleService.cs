using Discord;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services
{
	public class AutoRoleService
	{
		private readonly DiscordShardedClient _client;
		private readonly ILogger<AutoRoleService> _logger;
		private readonly IServiceProvider _serviceProvider;

		public AutoRoleService(ILogger<AutoRoleService> logger, IServiceProvider serviceProvider, DiscordShardedClient client)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_client = client;
		}

		/// <summary>
		///  Processes the given userId and adds all <see cref="AutoRole" />s to them that belong to the server.
		/// </summary>
		/// <param name="serverId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task TriggerAsync(ulong serverId, ulong userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var autoRoleRepository = scope.ServiceProvider.GetRequiredService<AutoRoleRepository>();

				var guild = _client.GetGuild(serverId);

				if (guild == null)
				{
					await autoRoleRepository.ClearAllAsync(serverId);

					_logger.LogWarning($"Guild with ID {serverId} could not be found. Deleted all auto-roles from database");
					return;
				}

				var user = guild.GetUser(userId);
				if (user == null)
				{
					_logger.LogWarning($"User with ID {userId} could not be found. Auto-roles will not be given");
					return;
				}

				var rolesToAdd = new List<IRole>();
				var allRoles = await autoRoleRepository.GetAllAsync(serverId);
				
				// Check to see whether bot has permission to add roles.
				var botPerms = guild.GetUser(_client.CurrentUser.Id).GuildPermissions;
				if (allRoles.Any() && !botPerms.ManageRoles && !botPerms.Administrator)
				{
					_logger.LogWarning($"Permissions invalid: ManageRoles permission returned false in guild {guild.Id}");

					await autoRoleRepository.ClearAllAsync(serverId);
					return;
				}

				
				foreach (ulong roleId in allRoles.Select(x => x.RoleId))
				{
					var role = guild.GetRole(roleId);
					if (role == null)
					{
						await autoRoleRepository.DeleteByRoleIdAsync(roleId);

						_logger.LogDebug($"Role with ID {roleId} could not be found in guild {serverId}. Deleted from database.");
					}
					else
					{
						rolesToAdd.Add(role);
					}
				}

				try
				{
					await user.AddRolesAsync(rolesToAdd);
					_logger.LogDebug($"User successfully given roles: {rolesToAdd.Humanize(x => $"[Name: {x.Name} | ID: {x.Id}]")}");
				}
				catch (Exception e)
				{
					_logger.LogWarning(e, $"Failed to add {allRoles.Count} roles to user {userId} in guild {serverId}.");

					// Iterate through all roles and eliminate problematic ones from the auto-role system.
					// Typically, these are roles that are too high in the role heirarchy for Kaguya to add.
					foreach (var role in rolesToAdd)
					{
						try
						{
							await user.AddRoleAsync(role);
						}
						catch (Exception e2)
						{
							await autoRoleRepository.DeleteByRoleIdAsync(role.Id);
							_logger.LogWarning(
								$"Identified problematic role: {role.Id}. Message: {e2.Message}." + " Deleted from database.");
						}
					}
				}
			}
		}
	}
}