using Kaguya.Database.Model;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IWordFilterRepository : IRepository<FilteredWord>
	{
		public Task<bool> DeleteIfExistsAsync(ulong key1, string key2);
		public Task<bool> InsertIfNotExistsAsync(FilteredWord value);
		public Task<FilteredWord[]> GetAllAsync(ulong serverId, bool includeWildcards);
		public Task DeleteAllAsync(ulong serverId);
	}
}