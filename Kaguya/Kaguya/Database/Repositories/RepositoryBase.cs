using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class RepositoryBase<T> : IRepository<T> where T : class
	{
		protected readonly KaguyaDbContext DbContext;
		protected readonly DbSet<T> Table;

		public RepositoryBase(KaguyaDbContext dbContext)
		{
			DbContext = dbContext;
			Table = dbContext.Set<T>();
		}

		/// <summary>
		///  Finds and returns a detached entity via the <see cref="Table" />.FindAsync() method. If the
		///  entity could not be found, null is returned.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>The entity with the given primary key values or null if it was not found.</returns>
		public async Task<T> GetAsync(params object[] key)
		{
			var entity = await Table.FindAsync(key);

			if (entity == null)
			{
				return null;
			}

			DbContext.Entry(entity).State = EntityState.Detached;

			return entity;
		}

		/// <summary>
		///  Safely deletes an object identified by the given primary key(s) if it exists and saves the change to the database.
		/// </summary>
		/// <param name="key">A collection of primary key values used to uniquely identify the given <see cref="T" /> object.</param>
		/// <returns></returns>
		public async Task DeleteAsync(params object[] key)
		{
			var match = await GetAsync(key);
			if (match != null)
			{
				Table.Remove(match);
				await DbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		///  Updates the given <see cref="value" /> and saves the changes to the database.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public async Task UpdateAsync(T value)
		{
			Table.Update(value);
			await DbContext.SaveChangesAsync();
		}

		/// <summary>
		///  Updates all of the provided <see cref="values" /> and saves the changes to the database.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public async Task UpdateRangeAsync(IEnumerable<T> values)
		{
			Table.UpdateRange(values);
			await DbContext.SaveChangesAsync();
		}

		/// <summary>
		///  Inserts the <see cref="value" /> and saves the change to the database.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public async Task InsertAsync(T value)
		{
			Table.Add(value);
			await DbContext.SaveChangesAsync();
		}

		/// <summary>
		///  Bulk inserts all <see cref="values" /> by calling <see cref="Table" />.AddRange() on the collection and
		///  saves the changes to the database.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public async Task BulkInsertAsync(IEnumerable<T> values)
		{
			Table.AddRange(values);
			await DbContext.SaveChangesAsync();
		}

		/// <summary>
		///  Returns the entire <see cref="Table" /> as a <see cref="List" />.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<T>> GetAllAsync() { return await Table.AsNoTracking().ToListAsync(); }

		/// <summary>
		///  Returns the count of all elements in the current <see cref="Table" />.
		/// </summary>
		/// <returns></returns>
		public async Task<int> GetCountAsync() { return await Table.AsNoTracking().CountAsync(); }
	}
}