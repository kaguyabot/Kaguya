using System.Threading.Tasks;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
	public class TemporaryBanRepository : ITemporaryBanRepository
	{
		public Task<TemporaryBan> GetAsync(ulong key) => throw new System.NotImplementedException();
		public Task DeleteAsync(ulong key) => throw new System.NotImplementedException();
		public Task<TemporaryBan> UpdateAsync(ulong key, TemporaryBan value) => throw new System.NotImplementedException();
	}
}