using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IServerExperienceRepository : IRepository<ServerExperience>
	{
		/// <summary>
		///  Gets or creates a <see cref="ServerExperience" /> object for the provided <see cref="userId" /> and <see cref="serverId" />.
		/// </summary>
		/// <param name="serverId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public Task<ServerExperience> GetOrCreateAsync(ulong serverId, ulong userId);

		/// <summary>
		///  Returns all <see cref="ServerExperience" /> objects that exist for the <see cref="serverId" />.
		/// </summary>
		/// <param name="serverId"></param>
		/// <returns></returns>
		public Task<IList<ServerExperience>> GetAllExpAsync(ulong serverId);

		/// <summary>
		///  Returns the <see cref="count" /> of top experience point holders for the given <see cref="serverId" />.
		/// </summary>
		/// <param name="serverId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task<IList<ServerExperience>> GetTopAsync(ulong serverId, int count = 10);

		/// <summary>
		///  Inserts or updates the <see cref="value" /> to the database.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Task AddOrUpdateAsync(ServerExperience value);

		/// <summary>
		///  Subtracts the <see cref="amount" /> of exp to the <see cref="ServerExperience" /> object that
		///  belongs to the given <see cref="serverId" /> and <see cref="userId" />.
		/// </summary>
		/// <param name="serverId"></param>
		/// <param name="userId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public Task SubtractAsync(ulong serverId, ulong userId, int amount);

		/// <summary>
		///  Returns the user's rank out of members in the <see cref="serverId" />.
		/// </summary>
		/// <param name="serverId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public Task<int> FetchRankAsync(ulong serverId, ulong userId);

		/// <summary>
		///  Returns the count of server exp saved for the serverId provided.
		/// </summary>
		/// <param name="serverId"></param>
		/// <returns></returns>
		public Task<int> GetAllCountAsync(ulong serverId);
	}
}