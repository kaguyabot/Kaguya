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
        public int Shards { get; init; }
        public int CommandsExecuted { get; init; }
        public int Fish { get; init; }
        public long Coins { get; init; }
        public int Gambles { get; init; }
        public double RamUsageMegabytes { get; init; }
        public int LatencyMilliseconds { get; init; }
        public string Version { get; init; }
    }
}