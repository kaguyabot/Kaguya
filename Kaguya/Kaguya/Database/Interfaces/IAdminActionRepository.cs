using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface IAdminActionRepository : IRepository<ulong, AdminAction>
	{
		public Task ForceExpireAsync(AdminAction value);
		public Task ForceExpireRangeAsync(IEnumerable<AdminAction> collection);
		public Task<IList<AdminAction>> GetAllForServerAsync(ulong serverId);
		public Task<int> GetCountForServerAsync(ulong serverId);
		public Task<IList<AdminAction>> GetAllUnexpiredForUserInServerAsync(ulong userId, ulong serverId);
		public Task<IList<AdminAction>> GetAllUnexpiredForUserInServerAsync(ulong userId, ulong serverId, string action);
		public Task UpdateRangeAsync(IEnumerable<AdminAction> collection);
	}
}