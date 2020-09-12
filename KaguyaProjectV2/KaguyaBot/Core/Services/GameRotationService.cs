using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public static class GameRotationService
    {
        private static byte index;
        private static bool Enabled = true;
        public static async Task Initialize()
        {
            Timer timer = new Timer(600000); // 10 Minutes
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (sender, e) =>
            {
                if (!Enabled)
                    return;
                
                var client = ConfigProperties.Client;
                var games = new List<Tuple<string, ActivityType>>
                {
                    new Tuple<string, ActivityType>($"Version {ConfigProperties.Version}", ActivityType.Streaming),
                    new Tuple<string, ActivityType>($"{client.TotalUsers():N0} users", ActivityType.Watching),
                    new Tuple<string, ActivityType>($"{client.Guilds.Count:N0} servers", ActivityType.Watching),
                    new Tuple<string, ActivityType>($"$help | @Kaguya help", ActivityType.Listening),
                    new Tuple<string, ActivityType>("$vote for bonuses", ActivityType.Watching),
                    new Tuple<string, ActivityType>("$premium", ActivityType.Watching)
                };

                if (index >= games.Count)
                    index = 0;

                var curGame = games[index];
                await client.SetGameAsync(curGame.Item1 + ".", null, curGame.Item2);
                await ConsoleLogger.LogAsync($"Switched game to: {curGame.Item2} {curGame.Item1}", LogLvl.INFO);

                index++;
            };
        }

        /// <summary>
        /// Pauses the game rotation service for a set duration.
        /// </summary>
        /// <param name="duration"></param>
        public static void Pause(TimeSpan duration)
        {
            Enabled = false;
            Timer timer = new Timer(duration.TotalMilliseconds);
            timer.Enabled = true;
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => Enabled = true;
        }
    }
}
