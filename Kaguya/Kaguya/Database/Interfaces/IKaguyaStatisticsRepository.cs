using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IKaguyaStatisticsRepository : IRepository<KaguyaStatistics>
    {
        public Task PostNewAsync();
        public Task<KaguyaStatistics> GetMostRecentAsync();
    }
}