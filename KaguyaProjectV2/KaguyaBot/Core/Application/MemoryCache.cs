using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Application
{
    /// <summary>
    ///     A static class containing various values that need to be stored in Memory and refreshed often.
    /// </summary>
    public static class MemoryCache
    {
        /// <summary>
        ///     The <see cref="string" /> Key of this <see cref="Dictionary{TKey,TValue}" />
        ///     is equal to the name of this command. The <see cref="int" />
        ///     value is equal to how many times this command has been used successfully.
        /// </summary>
        public static Dictionary<string, int>? MostPopularCommandCache { get; private set; }

        /// <summary>
        ///     A cache of all <see cref="OwnerGiveaway" /> objects in the database.
        /// </summary>
        public static List<OwnerGiveaway> OwnerGiveawaysCache { get; private set; }

        /// <summary>
        ///     A cache of all <see cref="OwnerGiveawayReaction" /> items in the database.
        /// </summary>
        public static List<OwnerGiveawayReaction> OwnerGiveawayReactions { get; private set; }

        /// <summary>
        ///     A cache of all currently active poker sessions, based on user ID.
        /// </summary>
        public static HashSet<ulong> ActivePokerSessions { get; private set; }

        public static KaguyaStatistics MostRecentStats { get; private set; }

        /// <summary>
        ///     Initializes the memory cache for various values that need to be refreshed often.
        ///     This is just the initial population of the class's static variables.
        /// </summary>
        public static async Task Initialize()
        {
            MostPopularCommandCache = DatabaseQueries.GetMostPopularCommandAsync();
            await ConsoleLogger.LogAsync("Memory Cache: Most popular command cache populated.", LogLvl.DEBUG);

            OwnerGiveawaysCache = await DatabaseQueries.GetAllAsync<OwnerGiveaway>();
            await ConsoleLogger.LogAsync("Memory Cache: Owner giveaways cache populated.", LogLvl.DEBUG);

            OwnerGiveawayReactions = await DatabaseQueries.GetAllAsync<OwnerGiveawayReaction>();
            await ConsoleLogger.LogAsync("Memory Cache: Owner giveaway previous reactions cache populated.", LogLvl.DEBUG);

            StartMostPopCommandCacheTimer();
            StartOwnerGiveawayCacheTimer();
        }

        /// <summary>
        ///     Refreshes the <see cref="MostPopularCommandCache" /> object.
        /// </summary>
        private static void StartMostPopCommandCacheTimer()
        {
            var timer = new Timer(600000); // 10 Minutes
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) => { MostPopularCommandCache = DatabaseQueries.GetMostPopularCommandAsync(); };
        }

        private static void StartOwnerGiveawayCacheTimer()
        {
            var timer = new Timer(600000); // 10 Minutes
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) => { OwnerGiveawaysCache = (await DatabaseQueries.GetAllAsync<OwnerGiveaway>()).ToList(); };
        }

        // Should only ever be used by KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaStatsFactory
        // Is run on program startup by said class.
        public static void SetStats(KaguyaStatistics stats) => MostRecentStats = stats;
    }
}