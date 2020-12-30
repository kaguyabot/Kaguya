using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface ICommandHistoryRepository
	{
		public Task<CommandHistory> GetAsync(ulong userId, ulong serverId, string command);
		public Task UpdateAsync(CommandHistory value);
		public Task DeleteAsync(CommandHistory value);
		public Task InsertAsync(CommandHistory value);
	}
}