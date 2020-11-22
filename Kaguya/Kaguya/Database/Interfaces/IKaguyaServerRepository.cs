using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IKaguyaServerRepository : IRepository<ulong, KaguyaServer>
    {
        public Task<KaguyaServer> GetOrCreateAsync(ulong id);
    }
}