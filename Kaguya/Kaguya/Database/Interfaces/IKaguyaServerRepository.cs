using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IKaguyaServerRepository : IRepository<KaguyaServer>
    {
        public Task<KaguyaServer> GetOrCreateAsync(ulong id);
    }
}