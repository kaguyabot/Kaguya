using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IRepository<T> where T : class
	{
		public Task<T> GetAsync(params object[] key);
		public Task DeleteAsync(params object[] key);
		public Task UpdateAsync(T value);
		public Task UpdateRangeAsync(IEnumerable<T> values);
		public Task InsertAsync(T value);
		public Task BulkInsertAsync(IEnumerable<T> values);
		public Task<IList<T>> GetAllAsync();
		public Task<int> GetCountAsync();
	}
}