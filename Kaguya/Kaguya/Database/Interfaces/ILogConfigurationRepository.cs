using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface ILogConfigurationRepository : IRepository<LogConfiguration>
    {
        public Task<LogConfiguration> GetOrCreateAsync(ulong key);
    }
}