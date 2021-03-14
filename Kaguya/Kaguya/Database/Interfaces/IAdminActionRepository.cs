using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IAdminActionRepository : IRepository<AdminAction>
	{
		public Task ForceExpireAsync(AdminAction value);
		public Task ForceExpireRangeAsync(IEnumerable<AdminAction> collection);
		public Task<IList<AdminAction>> GetAllAsync(ulong serverId, bool showHidden = false);
		public Task<IList<AdminAction>> GetAllAsync(ulong serverId, string action, bool showHidden = false);
		public Task<int> GetCountAsync(ulong serverId);
		public Task<IList<AdminAction>> GetAllUnexpiredAsync(ulong userId, ulong serverId, bool showHidden = false);

		public Task<IList<AdminAction>> GetAllUnexpiredAsync(ulong userId, ulong serverId, string action,
			bool showHidden = false);

		/// <summary>
		///  Gets a collection of <see cref="AdminAction" /> that need to be undone.
		///  If a user was muted for 30 minutes, and it's time for them to be unmuted by the
		///  <see cref="Kaguya.Internal.Services.Recurring.RevertAdminActionService" />, they will
		///  be returned as part of this collection.
		/// </summary>
		/// <returns></returns>
		public Task<IList<AdminAction>> GetAllToUndoAsync();

		public Task HideAsync(AdminAction value);
	}
}