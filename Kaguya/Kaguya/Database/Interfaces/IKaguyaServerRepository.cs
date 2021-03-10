using Kaguya.Database.Model;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IKaguyaServerRepository : IRepository<KaguyaServer>
	{
		public Task<KaguyaServer> GetOrCreateAsync(ulong id);
	}
}