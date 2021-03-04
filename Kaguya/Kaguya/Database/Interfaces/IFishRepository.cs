using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Internal.Services;

namespace Kaguya.Database.Interfaces
{
    public interface IFishRepository : IRepository<Fish>
    {
        /// <summary>
        /// Gets all Fish for the user
        /// </summary>
        /// <param name="userId">The user to gather fish for</param>
        /// <returns>all of the user's fish</returns>
        public Task<IList<Fish>> GetAllForUserAsync(ulong userId);
        /// <summary>
        /// Gets all fish for the server
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns>all of the server's fish</returns>
        public Task<IList<Fish>> GetAllForServerAsync(ulong serverId);
        /// <summary>
        /// Gets a count of all non-trash rarity fish for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<int> CountAllNonTrashAsync(ulong userId);
        /// <summary>
        /// Gets all fish of the specified type for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fish"></param>
        /// <returns></returns>
        public Task<IList<Fish>> GetAllOfTypeAsync(ulong userId, FishType fish);
        /// <summary>
        /// Gets all fish of the given rarity for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rarity"></param>
        /// <returns></returns>
        public Task<IList<Fish>> GetAllOfRarityAsync(ulong userId, FishRarity rarity);
        /// <summary>
        /// Gets all non-trash rarity fish for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<IList<Fish>> GetAllNonTrashAsync(ulong userId);
        /// <summary>
        /// Returns a sum of all fish caught by all users for the given rarity.
        /// </summary>
        /// <param name="rarity"></param>
        /// <returns></returns>
        public Task<int> CountAllOfRarityAsync(FishRarity rarity);
        /// <summary>
        /// Returns a gross sum of coins earned by all users who have played the fishing game.
        /// </summary>
        /// <returns></returns>
        public Task<int> CountAllCoinsEarnedAsync();
        /// <summary>
        /// Returns a net sum of all coins earned by all users who have played the fishing game.
        /// </summary>
        /// <returns></returns>
        public Task<int> CountNetCoinsEarnedAsync();
    }
}