using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IWordFilterRepository
    {
        public Task<FilteredWord> GetAsync(ulong key, string word);
        public Task UpdateAsync(FilteredWord value);
        public Task DeleteAsync(FilteredWord value);
        public Task<bool> DeleteIfExistsAsync(FilteredWord value);
        public Task InsertAsync(FilteredWord value);
        public Task<bool> InsertIfNotExistsAsync(FilteredWord value);
        public Task<FilteredWord[]> GetAllForServerAsync(ulong serverId, bool includeWildcards);
        public Task DeleteAllForServerAsync(ulong serverId);
    }
}