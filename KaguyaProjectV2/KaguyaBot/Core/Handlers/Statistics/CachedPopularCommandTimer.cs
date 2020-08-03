using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Statistics
{
    public class CachedPopularCommandTimer
    {
        /// <summary>
        /// The <see cref="string"/> Key of this <see cref="Dictionary{TKey,TValue}"/>
        /// is equal to the name of this command. The <see cref="int"/>
        /// value is equal to how many times this command has been used successfully.
        /// </summary>
        public static Dictionary<string, int>? MostPopularCommand { get; private set; }

        public static void Initialize()
        {
            Timer timer = new Timer(600000); // 10 mins = 600,000 milliseconds.
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, e) =>
            {
                MostPopularCommand = DatabaseQueries.GetMostPopularCommandAsync();
            };
        }
    }
}