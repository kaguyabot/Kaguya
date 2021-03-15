using Discord;
using Discord.WebSocket;
using Kaguya.Discord.Commands.Administration;
using Kaguya.Internal.Services.Recurring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Internal
{
	/// <summary>
	///  Used for silent operations (mute, shadowban) during automated "user punishment" processes
	///  such as the <see cref="AntiraidWorker" />.
	/// </summary>
	public class SilentSysActions
	{
		private const string SHADOWBAN_ROLE_NAME = "kaguya-shadowban";
		private const string MUTE_ROLE_NAME = "kaguya-mute";
		private readonly ILogger<SilentSysActions> _logger;

		public SilentSysActions(IServiceProvider serviceProvider)
		{
			_logger = serviceProvider.GetRequiredService<ILogger<SilentSysActions>>();
		}

		public async Task<bool> SilentMuteUserAsync(SocketGuildUser user, ulong? muteRoleId)
		{
			return await SilentApplyRoleAsync(user, muteRoleId, true);
		}

		public async Task<bool> SilentShadowbanUserAsync(SocketGuildUser user, ulong? muteRoleId)
		{
			return await SilentApplyRoleAsync(user, muteRoleId, false);
		}

		/// <summary>
		///  Applys either a mute role or shadowban role to the user. If not mute, it's a shadowban.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="roleId"></param>
		/// <param name="mute">Whether this is a mute role. If false, applys shadowban role.</param>
		/// <returns>
		///  Whether the mute role was successfully applied to the user and whether
		///  Kaguya could update all guild text channels with <see cref="OverwritePermissions" /> for the role.
		/// </returns>
		private async Task<bool> SilentApplyRoleAsync(SocketGuildUser user, ulong? roleId, bool mute)
		{
			var guild = user.Guild;
			IRole role = roleId.HasValue ? guild.GetRole(roleId.Value) : null;

			if (role == null)
			{
				string roleName = mute ? MUTE_ROLE_NAME : SHADOWBAN_ROLE_NAME;

				if (guild.Roles.Any(x => x.Name.Equals(roleName)))
				{
					role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
				}

				if (role == null)
				{
					var newRole = await CreateRoleAsync(guild, roleName, GuildPermissions.None);
					if (newRole == null)
					{
						return false;
					}

					role = newRole;
				}
			}

			try
			{
				await user.AddRoleAsync(role);
			}
			catch (Exception e)
			{
				_logger.LogError(e,
					"Exception encountered during processing of sys-action automated mute for " + $"user {user.Id} in guild {guild.Id}.");

				return false;
			}

			if (guild.Channels.Any(x => !x.GetPermissionOverwrite(role).HasValue))
			{
				var owPerms = Mute.GetMuteOverwritePermissions();

				try
				{
					foreach (var channel in guild.Channels.Where(x => !x.GetPermissionOverwrite(role).HasValue))
					{
						if (!mute)
						{
							owPerms = OverwritePermissions.DenyAll(channel);
						}

						await channel.AddPermissionOverwriteAsync(role, owPerms);
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e,
						"Exception encountered during the updating of the text channel " +
						$"permissions in guild {guild.Id} for role {role.Name}.");

					return false;
				}
			}

			return true;
		}

		private static async Task<IRole> CreateRoleAsync(SocketGuild guild, string name, GuildPermissions permissions)
		{
			try
			{
				var role = await guild.CreateRoleAsync(name, permissions, null, false, null);

				return role;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public async Task SilentRemoveRoleByIdAsync(SocketGuildUser user, SocketGuild guild, ulong roleId)
		{
			var role = guild.GetRole(roleId);

			if (role == null)
			{
				_logger.LogWarning($"Failed to silently remove role {roleId} from user {user.Id} in " +
				                   $"guild {guild.Id} - role was null");
			}

			if (!guild.Roles.Any(x => x.Id == roleId))
			{
				_logger.LogWarning($"Failed to silently remove role {roleId} from user {user.Id} in " +
				                   $"guild {guild.Id} - role not found in guild");

				return;
			}

			if (!user.Roles.Contains(role))
			{
				_logger.LogWarning($"Failed to silently remove role {roleId} from user {user.Id} in " +
				                   $"guild {guild.Id} - user did not have the role.");

				return;
			}

			try
			{
				await user.RemoveRoleAsync(role);
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, $"Failed to silently remove role {role!.Name} from user {user.Id} " + $"in guild {guild.Id}");
			}
		}
	}
}