using Kaguya.Database.Model;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface ILogConfigurationRepository : IRepository<LogConfiguration>
	{
		public Task<LogConfiguration> GetOrCreateAsync(ulong key);
	}
}