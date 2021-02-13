using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface IKaguyaUserRepository : IRepository<KaguyaUser>
	{
		public Task<KaguyaUser> GetOrCreateAsync(ulong id);
		public Task<IEnumerable<KaguyaUser>> GetActiveRatelimitedUsersAsync(bool ignoreOwner);
		public Task<int> FetchExperienceRankAsync(ulong id);
		/// <summary>
		/// Counts the total amount of coins owned by all users.
		/// </summary>
		/// <returns></returns>
		public Task<long> CountCoinsAsync();
	}
}