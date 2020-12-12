using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Microsoft.Extensions.Logging;

namespace Kaguya.Services
{
    /// <summary>
    /// This class is responsible for automatically ratelimiting users. A ratelimit is a temporary blacklist
    /// from the bot that grows on a linear scale until a certain amount of rate limits. A user is unratelimited
    /// if they go a long time without an infraction.
    /// </summary>
    ///
    // TODO: Implement.
    public class RatelimitService
    {
        public const int Commands = 6;
        public const int Milliseconds = 4750;
        
        private readonly ILogger<RatelimitService> _logger;
        private readonly KaguyaUserRepository _userRepository;
        private static readonly BlockingCollection<KaguyaUser> _ratelimitQueue = new BlockingCollection<KaguyaUser>();
        private Task _runner;

        private readonly List<KaguyaUser> _usersToReset = new List<KaguyaUser>();

        private readonly Dictionary<int, TimeSpan> _ratelimitBlacklistDurations = new Dictionary<int, TimeSpan>
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

        public RatelimitService(ILogger<RatelimitService> logger, KaguyaUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _runner = Task.Run(async () => await Run());

            InitReset().GetAwaiter().GetResult();
        }

        // TODO: Implement
        private async Task Run()
        {
            foreach (KaguyaUser user in _ratelimitQueue.GetConsumingEnumerable())
            {
                try
                {
                    user.ActiveRateLimit -= 1;
                    if (user.RateLimitWarnings > 0 && user.LastRatelimited < DateTime.Now.AddDays(31))
                    {
                        _logger.LogInformation($"User {user} has had their ratelimit warnings reset.");
                        user.RateLimitWarnings = 0;
                    }

                    if (user.ActiveRateLimit == 0)
                    {
                        await InvokeRatelimitAsync(user);
                    }
                    
                    _usersToReset.Add(user);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Exception encountered when processing user(s) inside of _ratelimitQueue.");
                }
            }
        }

        private Task InitReset()
        {
            var timer = new Timer(Milliseconds);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (_, _) =>
            {
                if (_usersToReset.Count == 0)
                    return;
                
                foreach (KaguyaUser user in _usersToReset)
                {
                    user.ActiveRateLimit = 0;
                }

                await _userRepository.UpdateRange(_usersToReset);
                _usersToReset.Clear();
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles all blacklists and punishments associated with a user who breaches the ratelimit.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task InvokeRatelimitAsync(KaguyaUser user)
        {
        }

        public static void Enqueue(KaguyaUser user) => _ratelimitQueue.Add(user);

        /// <summary>
        /// Based on the number of <see cref="ratelimits"/>, returns a duration in minutes for how long a user should be ratelimited.
        /// </summary>
        /// <param name="ratelimits"></param>
        /// <returns></returns>
        private static double RatelimitsToMinutes(int ratelimits)
        {
            if (ratelimits > 10)
            {
                return RatelimitsToMinutes(10);
            }
            
            var sq = Math.Sqrt(ratelimits / 1.5);
            const double exp = 10.5;

            // Read as : SquareRoot(x / 1.5)^10.5 + 1.
            return Math.Pow(sq, exp) + 1; // +1 guarantees a 1 minute ratelimit.
        }
    }
}