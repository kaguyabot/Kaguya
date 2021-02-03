using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IAntiraidRepository : IRepository<AntiRaidConfig>
    {
        public Task InsertOrUpdateAsync(AntiRaidConfig config);
    }
}