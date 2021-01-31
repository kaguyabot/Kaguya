using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IGambleHistoryRepository : IRepository<ulong, GambleHistory>
    {
        public Task<IList<GambleHistory>> GetAllForUserAsync(ulong userId);
        public Task<int> GetCountForUserAsync(ulong userId);
        public Task<int> GetCountForServerAsync(ulong serverId);
        public Task<int> GetCountAsync();
        public Task<GambleHistory> GetMostRecentForUserAsync(ulong userId);
        public Task<GambleHistory> GetBiggestLossAsync(ulong userId);
        public Task<GambleHistory> GetBiggestWinAsync(ulong userId);
        public Task DeleteAsync(GambleHistory value);
    }
}