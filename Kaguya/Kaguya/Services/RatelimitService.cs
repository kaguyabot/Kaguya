using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;

namespace Kaguya.Services
{
    /// <summary>
    /// This class is responsible for automatically ratelimiting users. A ratelimit is a temporary blacklist
    /// from the bot that grows on a linear scale until a certain amount of rate limits. A user is unratelimited
    /// if they go a long time without an infraction.
    /// </summary>
    public class RatelimitService
    {
        private static readonly BlockingCollection<KaguyaUser> _ratelimitQueue = new BlockingCollection<KaguyaUser>();
        private Task _runner;

        private readonly Dictionary<int, TimeSpan> _ratelimitBlacklistDurations = new Dictionary<int, TimeSpan>()
        {
            { 1, TimeSpan.FromMinutes(RatelimitsToMinutes(1)) },
            { 2, TimeSpan.FromMinutes(RatelimitsToMinutes(2)) },
            { 3, TimeSpan.FromMinutes(RatelimitsToMinutes(3)) },
            { 4, TimeSpan.FromMinutes(RatelimitsToMinutes(4)) },
            { 5, TimeSpan.FromMinutes(RatelimitsToMinutes(5)) },
            { 6, TimeSpan.FromMinutes(RatelimitsToMinutes(6)) },
            { 7, TimeSpan.FromMinutes(RatelimitsToMinutes(7)) },
            { 8, TimeSpan.FromMinutes(RatelimitsToMinutes(8)) },
            { 9, TimeSpan.FromMinutes(RatelimitsToMinutes(9)) },
            { 10, TimeSpan.FromMinutes(RatelimitsToMinutes(10)) }
        };

        public RatelimitService()
        {
            _runner = Task.Run(async () => await Run());
        }

        private async Task Run()
        {
            foreach (KaguyaUser user in _ratelimitQueue.GetConsumingEnumerable())
            {
                // TODO: Implement.
            }
        }

        public void Enqueue(KaguyaUser user) => _ratelimitQueue.Add(user);

        private static double RatelimitsToMinutes(int ratelimits)
        {
            var sq = Math.Sqrt(ratelimits / 1.5);
            const double exp = 10.5;

            // Read as : SquareRoot(x / 1.5)^10.5 + 1.
            return Math.Pow(sq, exp) + 1; // +1 guarantees a 1 minute ratelimit.
        }
    }
}