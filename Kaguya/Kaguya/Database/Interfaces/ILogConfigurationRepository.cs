using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface ILogConfigurationRepository : IRepository<ulong, LogConfiguration>
    {
        public Task<LogConfiguration> GetOrCreateAsync(ulong key);
    }
}