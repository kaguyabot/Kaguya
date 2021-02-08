using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

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
		public Task<IList<AdminAction>> GetAllUnexpiredAsync(ulong userId, ulong serverId, string action, bool showHidden = false);
		public Task HideAsync(AdminAction value);
	}
}