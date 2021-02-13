using System.Collections;
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
		/// <summary>
		/// Returns the top <see cref="count"/> coin holders out of all users. Does not
		/// display the bot owner's coins.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task<IList<KaguyaUser>> GetTopCoinHoldersAsync(int count = 10);
		/// <summary>
		/// Returns the top <see cref="count"/> exp holders out of all users.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task<IList<KaguyaUser>> GetTopExpHoldersAsync(int count = 10);
	}
}