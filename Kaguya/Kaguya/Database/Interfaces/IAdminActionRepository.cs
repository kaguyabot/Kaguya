using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface IAdminActionRepository : IRepository<ulong, AdminAction>
	{
		public Task<IList<AdminAction>> GetAllForServerAsync(ulong serverId);
		public Task<int> GetCountForServerAsync(ulong serverId);
	}
}