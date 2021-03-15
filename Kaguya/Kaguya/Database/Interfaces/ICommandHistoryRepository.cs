using Kaguya.Database.Model;
using System;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface ICommandHistoryRepository : IRepository<CommandHistory>
	{
		/// <summary>
		///  Returns the all-time successful count of all commands executed.
		/// </summary>
		/// <returns></returns>
		public Task<int> GetSuccessfulCountAsync();

		/// <summary>
		///  Returns the all-time successful command count for the given user.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public Task<int> GetSuccessfulCountAsync(ulong userId);

		/// <summary>
		///  Returns the count of successful command executions that occurred within the difference
		///  between now and the positive time threshold.
		/// </summary>
		/// <param name="threshold">A positive timespan representing the time constraint.</param>
		/// <returns></returns>
		public Task<int> GetRecentSuccessfulCountAsync(TimeSpan threshold);

		/// <summary>
		///  Returns the count of successful command executions for the given user that have occurred
		///  within the difference between now and the positive time threshold.
		/// </summary>
		/// <param name="userId">The id of the user we are finding commands for</param>
		/// <param name="threshold">A positive timespan representing the time constraint.</param>
		/// <returns></returns>
		public Task<int> GetRecentSuccessfulCountAsync(ulong userId, TimeSpan threshold);

		/// <summary>
		///  Returns the most commonly used command for the given user.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public Task<string> GetFavoriteCommandAsync(ulong userId);

		/// <summary>
		///  Returns the most popular command and how many times it has been executed.
		/// </summary>
		/// <returns></returns>
		public Task<(string cmdName, int count)> GetMostPopularCommandAsync();
	}
}