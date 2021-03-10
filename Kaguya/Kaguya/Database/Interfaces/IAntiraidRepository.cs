using Kaguya.Database.Model;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IAntiraidRepository : IRepository<AntiRaidConfig>
	{
		public Task InsertOrUpdateAsync(AntiRaidConfig config);
	}
}