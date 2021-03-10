using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IGiveawayRepository : IRepository<Giveaway>
	{
		public Task<IList<Giveaway>> GetActiveGiveawaysAsync();
		public Task<IList<Giveaway>> GetActiveGiveawaysAsync(ulong serverId);
	}
}