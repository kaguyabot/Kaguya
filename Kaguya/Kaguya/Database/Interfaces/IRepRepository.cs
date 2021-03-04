using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IRepRepository : IRepository<Rep>
    {
        /// <summary>
        /// Returns all rep belonging to the given userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<IList<Rep>> GetAllAsync(ulong userId);
        /// <summary>
        /// Returns the most recent rep object that belongs to the given userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<Rep> GetMostRecentAsync(ulong userId);
        /// <summary>
        /// Returns the total count of rep belonging to the given userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<int> GetCountRepReceivedAsync(ulong userId);
        /// <summary>
        /// Returns the total count of rep given to others by the given userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<int> GetCountRepGivenAsync(ulong userId);
    }
}