using Kaguya.Database.Model;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IKaguyaStatisticsRepository : IRepository<KaguyaStatistics>
	{
		public Task PostNewAsync();
		public Task<KaguyaStatistics> GetMostRecentAsync();
	}
}