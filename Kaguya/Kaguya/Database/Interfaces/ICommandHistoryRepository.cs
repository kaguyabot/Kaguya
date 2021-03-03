using System;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface ICommandHistoryRepository : IRepository<CommandHistory>
	{
		/// <summary>
		/// Returns the all-time successful count of all commands executed.
		/// </summary>
		/// <returns></returns>
		public Task<int> GetSuccessfulCountAsync();
		/// <summary>
		/// Returns the count of successful command executions that occurred within the difference
		/// between now and the positive offset.
		/// </summary>
		/// <param name="threshold">A positive timespan representing the time constraint.</param>
		/// <returns></returns>
		public Task<int> GetRecentSuccessfulCountAsync(TimeSpan threshold);
		/// <summary>
		/// Returns the all-time successful command count for the given user.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public Task<int> GetSuccessfulCountAsync(ulong userId);
		/// <summary>
		/// Returns the all tie
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		public Task<int> GetRecentSuccessfulCountAsync(ulong userId, TimeSpan threshold);
	}
}