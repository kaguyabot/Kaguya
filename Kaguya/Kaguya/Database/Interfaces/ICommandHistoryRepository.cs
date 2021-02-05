using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface ICommandHistoryRepository : IRepository<CommandHistory>
	{
		public Task<int> GetSuccessfulCountAsync();
	}
}