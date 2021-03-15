using Kaguya.Database.Model;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IAutoRoleRepository : IRepository<AutoRole>
	{
		/// <summary>
		/// Returns a <see cref="AutoRole"/> with the given <see cref="roleId"/>. Returns null if not found.
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public Task<AutoRole> GetAsync(ulong roleId);
		/// <summary>
		/// Returns the total count of <see cref="AutoRole"/>s this server has active.
		/// </summary>
		/// <param name="serverId"></param>
		/// <returns></returns>
		public Task<int> CountAsync(ulong serverId);
		/// <summary>
		/// Returns all <see cref="AutoRole"/>s this server has.
		/// </summary>
		/// <param name="serverId"></param>
		/// <returns></returns>
		public Task<IList<AutoRole>> GetAllAsync(ulong serverId);
		/// <summary>
		/// Removes all <see cref="AutoRole"/>s for the server.
		/// </summary>
		/// <param name="serverId"></param>
		/// <returns></returns>
		public Task ClearAllAsync(ulong serverId);
		/// <summary>
		/// Deletes the first <see cref="AutoRole"/> with the given role ID.
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public Task DeleteByRoleIdAsync(ulong roleId);
	}
}