using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IPollRepository : IRepository<Poll>
	{
		/// <summary>
		/// A collection of polls that have expired but have not yet been
		/// edited to reflect this back to the user.
		/// </summary>
		/// <returns></returns>
		public Task<IList<Poll>> GetAllToNotifyAsync();
		/// <summary>
		/// A collection of all polls that are currently ongoing.
		/// </summary>
		/// <returns></returns>
		public Task<IList<Poll>> GetAllOngoingAsync();
		/// <summary>
		/// Returns a poll with the given message ID. Message IDs are unique.
		/// </summary>
		/// <param name="messageId"></param>
		/// <returns></returns>
		public Task<Poll> GetAsync(ulong messageId);
	}
}