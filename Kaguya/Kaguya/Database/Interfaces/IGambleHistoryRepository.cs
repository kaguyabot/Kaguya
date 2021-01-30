using System.Collections.Generic;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IGambleHistoryRepository : IRepository<ulong, GambleHistory>
    {
        public IList<GambleHistory> GetAllForUserAsync(ulong userId);
        public int GetCountForUserAsync(ulong userId);
        public int GetCountForServerAsync(ulong serverId);
        public int GetCountAsync();
    }
}