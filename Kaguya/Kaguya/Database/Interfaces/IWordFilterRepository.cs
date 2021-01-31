using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;

namespace Kaguya.Database.Interfaces
{
    public interface IWordFilterRepository : IRepository<ulong, string, FilteredWord>
    {
        public Task<bool> DeleteIfExistsAsync(ulong key1, string key2);
        public Task<bool> InsertIfNotExistsAsync(FilteredWord value);
        public Task<FilteredWord[]> GetAllForServerAsync(ulong serverId, bool includeWildcards);
        public Task DeleteAllForServerAsync(ulong serverId);
    }
}