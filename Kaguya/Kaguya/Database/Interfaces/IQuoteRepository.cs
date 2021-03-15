using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IQuoteRepository : IRepository<Quote>
	{
		public Task<IList<Quote>> GetAllAsync(ulong serverId);
		public Task<Quote> GetRandomQuoteAsync(ulong serverId);
	}
}