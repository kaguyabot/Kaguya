using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Application
{
    /// <summary>
    /// A static class containing various values that need to be stored in Memory and refreshed often.
    /// </summary>
    public static class MemoryCache
    {
        /// <summary>
        /// The <see cref="string"/> Key of this <see cref="Dictionary{TKey,TValue}"/>
        /// is equal to the name of this command. The <see cref="int"/>
        /// value is equal to how many times this command has been used successfully.
        /// </summary>
        public static Dictionary<string, int>? MostPopularCommandCache { get; private set; }

        /// <summary>
        /// A cache of all <see cref="OwnerGiveaway"/> objects in the database.
        /// </summary>
        public static List<OwnerGiveaway> OwnerGiveawaysCache { get; private set; }

        /// <summary>
        /// A cache of all <see cref="OwnerGiveawayReaction"/> items in the database.
        /// </summary>
        public static List<OwnerGiveawayReaction> OwnerGiveawayReactions { get; private set; }

        /// <summary>
        /// A cache of all currently active poker sessions, based on user ID.
        /// todo: Assign value.
        /// </summary>
        public static HashSet<ulong> ActivePokerSessions { get; private set; }

        /// <summary>
        /// A cache of the count of all commands used.
        /// </summary>
        public static int AllTimeCommandCount { get; private set; }

        /// <summary>
        /// A cache of the count of all caught fish. Does not include <see cref="FishType.BAIT_STOLEN"/> events.
        /// </summary>
        public static int AllTimeFishCount { get; private set; }

        /// <summary>
        /// A cache of the count of all points in circulation.
        /// </summary>
        public static int AllPointsCount { get; private set; }

        /// <summary>
        /// Initializes the memory cache for various values that need to be refreshed often.
        /// This is just the initial population of the class's static variables.
        /// </summary>
        public static async Task Initialize()
        {
            MostPopularCommandCache = DatabaseQueries.GetMostPopularCommandAsync();
            await ConsoleLogger.LogAsync("Memory Cache: Most popular command cache populated.", LogLvl.DEBUG);
            OwnerGiveawaysCache = await DatabaseQueries.GetAllAsync<OwnerGiveaway>();
            await ConsoleLogger.LogAsync("Memory Cache: Owner giveaways cache populated.", LogLvl.DEBUG);
            OwnerGiveawayReactions = await DatabaseQueries.GetAllAsync<OwnerGiveawayReaction>();
            await ConsoleLogger.LogAsync("Memory Cache: Owner giveaway previous reactions cache populated.", LogLvl.DEBUG);
            AllTimeCommandCount = await DatabaseQueries.GetCountAsync<CommandHistory>();
            await ConsoleLogger.LogAsync("Memory Cache: All time command count populated.", LogLvl.DEBUG);
            AllTimeFishCount = await DatabaseQueries.GetCountAsync<Fish>(x => x.FishType != FishType.BAIT_STOLEN);

            StartMostPopCommandCacheTimer();
            StartOwnerGiveawayCacheTimer();
            StartDbStatisticsTimer();
        }

        /// <summary>
        /// Refreshes the <see cref="MostPopularCommandCache"/> object.
        /// </summary>
        private static void StartMostPopCommandCacheTimer(long milliseconds = 600000)
        {
            var timer = new Timer(milliseconds);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) => { MostPopularCommandCache = DatabaseQueries.GetMostPopularCommandAsync(); };
        }

        private static void StartOwnerGiveawayCacheTimer(long milliseconds = 600000)
        {
            var timer = new Timer(milliseconds);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) => { OwnerGiveawaysCache = (await DatabaseQueries.GetAllAsync<OwnerGiveaway>()).ToList(); };
        }

        private static void StartDbStatisticsTimer()
        {
            var timer = new Timer(300000); // 5 minutes
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) =>
            {
                AllTimeCommandCount = await DatabaseQueries.GetCountAsync<CommandHistory>();
                AllTimeFishCount = await DatabaseQueries.GetCountAsync<Fish>(x => x.FishType != FishType.BAIT_STOLEN);
                AllPointsCount = DatabaseQueries.GetTotalCurrency();
            };
        }
    }
}