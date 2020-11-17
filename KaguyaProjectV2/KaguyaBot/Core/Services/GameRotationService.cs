using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public static class GameRotationService
    {
        private static byte _index;
        private static bool _enabled = true;
        private static readonly DiscordShardedClient _client = ConfigProperties.Client;

        private static readonly List<Tuple<string, ActivityType>> _games = new List<Tuple<string, ActivityType>>
        {
            new Tuple<string, ActivityType>($"Version {ConfigProperties.Version}", ActivityType.Streaming),
            new Tuple<string, ActivityType>($"{_client.TotalUsers():N0} users", ActivityType.Watching),
            new Tuple<string, ActivityType>($"{_client.Guilds.Count:N0} servers", ActivityType.Watching),
            new Tuple<string, ActivityType>($"$help | @Kaguya help", ActivityType.Listening),
            new Tuple<string, ActivityType>("$vote for bonuses", ActivityType.Watching),
            new Tuple<string, ActivityType>("$premium", ActivityType.Watching)
        };

        public static async Task Initialize()
        {
            var timer = new Timer(600000); // 10 Minutes
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (_, _) =>
            {
                if (!_enabled)
                    return;

                if (_index >= _games.Count)
                    _index = 0;

                Tuple<string, ActivityType> curGame = _games[_index];
                await Set(curGame);
                await ConsoleLogger.LogAsync($"Switched game to: {curGame.Item2} {curGame.Item1}", LogLvl.INFO);

                _index++;
            };
        }

        /// <summary>
        /// Pauses the game rotation service for a set duration.
        /// </summary>
        /// <param name="duration"></param>
        public static void Pause(TimeSpan duration)
        {
            _enabled = false;
            var timer = new Timer(duration.TotalMilliseconds);
            timer.Enabled = true;
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => _enabled = true;
        }

        /// <summary>
        /// Resumes the game rotation service.
        /// </summary>
        public static void Resume() => _enabled = true;

        public static async Task Set(Tuple<string, ActivityType> game) => await _client.SetGameAsync(game.Item1 + ".", null, game.Item2);
        public static async Task Set(Tuple<string, ActivityType> game, string streamUrl) => 
            await _client.SetGameAsync(game.Item1 + ".", streamUrl, game.Item2);
        public static async Task SetToDefault() => await Set(_games[0]);

        public static async Task Set(string text, ActivityType type)
        {
            var toSet = new Tuple<string, ActivityType>(text, type);
            await Set(toSet);
        }
    }
}