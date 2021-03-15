using Kaguya.Database.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IKaguyaUserRepository : IRepository<KaguyaUser>
	{
		public Task<KaguyaUser> GetOrCreateAsync(ulong id);
		public Task<IEnumerable<KaguyaUser>> GetActiveRatelimitedUsersAsync(bool ignoreOwner);
		public Task<int> FetchExperienceRankAsync(ulong id);

		/// <summary>
		///  Counts the total amount of coins owned by all users.
		/// </summary>
		/// <returns></returns>
		public Task<long> CountCoinsAsync();

		/// <summary>
		///  Returns the top <see cref="count" /> coin holders out of all users. Does not
		///  display the bot owner's coins.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task<IList<KaguyaUser>> GetTopCoinHoldersAsync(int count = 10);

		/// <summary>
		///  Returns the top <see cref="count" /> exp holders out of all users.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task<IList<KaguyaUser>> GetTopExpHoldersAsync(int count = 10);

		/// <summary>
		///  Returns the top <see cref="count" /> fish holders out of all users.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task<IList<KaguyaUser>> GetTopFishHoldersAsync(int count = 10);

		/// <summary>
		///  Returns a collection of <see cref="ulong" />s containing all
		///  currently active premium subscribers, unique by id.
		/// </summary>
		/// <returns></returns>
		public Task<IList<ulong>> GetAllActivePremiumAsync();

		/// <summary>
		///  Returns a list of <see cref="ulong" />s containing all
		///  expired premium key holders, unique by id.
		/// </summary>
		/// <param name="cutoffDays">
		///  A positive <see cref="int" /> value, in days,
		///  representing the maximum farthest point in time behind the current <see cref="DateTime" />
		///  for which we should count a user in this collection.
		///  A duration of 30 days will return all expired key holder IDs who
		///  have redeemed a key within the last 30 days.
		/// </param>
		/// <returns></returns>
		public Task<IList<ulong>> GetAllExpiredPremiumAsync(int cutoffDays);
	}
}