using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
	public interface IKaguyaUserRepository : IRepository<ulong, KaguyaUser>
	{
		public Task<KaguyaUser> GetOrCreateAsync(ulong id);
	}
}