using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IGambleHistoryRepository : IRepository<GambleHistory>
    {
        public Task<IList<GambleHistory>> GetAllAsync(ulong userId);
        public Task<int> GetCountForUserAsync(ulong userId);
        public Task<int> GetCountForServerAsync(ulong serverId);
        public Task<GambleHistory> GetMostRecentForUserAsync(ulong userId);
        public Task<GambleHistory> GetBiggestLossAsync(ulong userId);
        public Task<GambleHistory> GetBiggestWinAsync(ulong userId);
        public Task DeleteAsync(GambleHistory value);
        /// <summary>
        /// Returns the all time count of gamble winnings in coins
        /// </summary>
        /// <returns></returns>
        public Task<int> TotalGambleWins();
        /// <summary>
        /// Returns the all time count of gamble losses
        /// </summary>
        /// <returns></returns>
        public Task<int> TotalGambleLosses();
        /// <summary>
        /// The all-time sum of coins won through gambling
        /// </summary>
        /// <returns></returns>
        public Task<long> TotalGambleWinsCoins();
        /// <summary>
        /// The all-time sum of coins lost through gambling
        /// </summary>
        /// <returns></returns>
        public Task<long> TotalGambleLossesCoins();
    }
}