using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class KaguyaStatistics
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }
        public int Users { get; init; }
        /// <summary>
        /// How many servers the bot has ever connected to and has data on.
        /// </summary>
        public int Servers { get; init; }
        /// <summary>
        /// How many servers the bot is currently connected to.
        /// </summary>
        public int ConnectedServers { get; init; }
        /// <summary>
        /// The total amount of shards the bot is connected to.
        /// </summary>
        public int Shards { get; init; }
        /// <summary>
        /// How many commands have been executed (all-time)
        /// </summary>
        public int CommandsExecuted { get; init; }
        /// <summary>
        /// How many commands have been executed in the last 24 hours
        /// </summary>
        public int CommandsExecutedTwentyFourHours { get; init; }
        /// <summary>
        /// Total fish caught (non-trash)
        /// </summary>
        public int Fish { get; init; }
        /// <summary>
        /// Total coins in circulation
        /// </summary>
        public long Coins { get; init; }
        /// <summary>
        /// Total gambles
        /// </summary>
        public int Gambles { get; init; }
        /// <summary>
        /// RAM usage, in megabytes.
        /// </summary>
        public double RamUsageMegabytes { get; init; }
        /// <summary>
        /// The bot's latency to the Discord gateway, in milliseconds.
        /// </summary>
        public int LatencyMilliseconds { get; init; }
        /// <summary>
        /// The version of the bot.
        /// </summary>
        public string Version { get; init; }
        /// <summary>
        /// When the stats were taken
        /// </summary>
        public DateTimeOffset? Timestamp { get; init; }
    }
}