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
        public static List<OwnerGiveaway> OwnerGiveawaysCache { get; private set; }
        public static List<OwnerGiveawayReaction> OwnerGiveawayReactions { get; private set; }
        public static HashSet<ulong> ActivePokerSessions { get; set; } = new HashSet<ulong>();

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
            
            StartMostPopCommandCacheTimer();
            StartOwnerGiveawayCacheTimer();
        }

        /// <summary>
        /// Refreshes the <see cref="MostPopularCommandCache"/> object.
        /// </summary>
        private static void StartMostPopCommandCacheTimer(long milliseconds = 600000)
        {
            Timer timer = new Timer(milliseconds);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) => 
                { MostPopularCommandCache = DatabaseQueries.GetMostPopularCommandAsync(); };
        }

        private static void StartOwnerGiveawayCacheTimer(long milliseconds = 600000)
        {
            Timer timer = new Timer(milliseconds);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) => 
                { OwnerGiveawaysCache = (await DatabaseQueries.GetAllAsync<OwnerGiveaway>()).ToList(); };
        }

        /// <summary>
        /// Sets the first property in the <see cref="MemoryCache"/> class whose type
        /// matches that of <see cref="T"/>
        /// </summary>
        /// <param name="arg">The value at which to set </param>
        /// <param name="propertyName">If there are multiple cached properties that type
        /// equals <see cref="T"/>, this value must be specified, else an exception will be thrown. </param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentException">Thrown if there are multiple static variables in the
        /// <see cref="MemoryCache"/> class whose type matches that of <see cref="T"/>.
        /// </exception>
        private static void SetCache<T>(T arg, string propertyName = null) where T : IMemoryCacheable<T>
        {
            PropertyInfo[] pInfo = typeof(MemoryCache).GetProperties();
            PropertyInfo pMatch = null;

            int matchCount = pInfo.Count(x => x.GetType() == typeof(T));
            
            if(matchCount > 1 && propertyName == null)
            {
                throw new ArgumentException("There is more than one static varible in the MemoryCache class " +
                                            "that matches the type of the argument passed into this method. The " +
                                            "name of the property must be specified.");
            }
            
            if(matchCount == 1)
            {
                pMatch = pInfo.FirstOrDefault(x => x.GetType() == typeof(T));
            }
            
            if (propertyName != null)
            {
                pMatch = pInfo.FirstOrDefault(x => x.Name == propertyName);
            }
            
            if(pMatch == null)
                throw new ArgumentException("The argument passed into this method does not match any " +
                                            "of the static variables in the MemoryCache class.");
            
            pMatch.SetValue(pMatch, arg);
        }
    }
}