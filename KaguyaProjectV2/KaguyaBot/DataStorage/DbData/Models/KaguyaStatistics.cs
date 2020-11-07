using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using System;
using LinqToDB.Mapping;
using Newtonsoft.Json;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "stats")]
    public class KaguyaStatistics : IKaguyaQueryable<KaguyaStatistics>
    {
        [JsonProperty(PropertyName = "kaguya_users")]
        [Column(Name = "kaguya_users"), NotNull]
        public int KaguyaUsers { get; set; }

        [JsonProperty(PropertyName = "guilds")]
        [Column(Name = "guilds"), NotNull]
        public int Guilds { get; set; }

        [JsonProperty(PropertyName = "guild_users")]

        [Column(Name = "guild_users"), NotNull]
        public int GuildUsers { get; set; }

        [JsonProperty(PropertyName = "shards")]

        [Column(Name = "shards"), NotNull]
        public int Shards { get; set; }

        [JsonProperty(PropertyName = "commands")]

        [Column(Name = "commands"), NotNull]
        public int Commands { get; set; }
        [JsonProperty(PropertyName = "commands_last24hours")]

        [Column(Name = "commands_last24hours"), NotNull]
        public int CommandsLast24Hours { get; set; }
        [JsonProperty(PropertyName = "fish")]

        [Column(Name = "fish"), NotNull]
        public int Fish { get; set; }
        [JsonProperty(PropertyName = "points")]

        [Column(Name = "points"), NotNull]
        public long Points { get; set; }
        [JsonProperty(PropertyName = "gambles")]
        
        [Column(Name = "gambles"), NotNull]
        public int Gambles { get; set; }
        [JsonProperty(PropertyName = "time_stamp")]
        
        [Column(Name = "time_stamp"), NotNull]
        public DateTime TimeStamp { get; set; }
        [JsonProperty(PropertyName = "text_channels")]
        
        [Column(Name = "text_channels"), NotNull]
        public int TextChannels { get; set; }
        [JsonProperty(PropertyName = "voice_channels")]
        
        [Column(Name = "voice_channels"), NotNull]
        public int VoiceChannels { get; set; }
        [JsonProperty(PropertyName = "ram_usage")]
        
        [Column(Name = "ram_usage"), NotNull]
        public double RamUsageMegabytes { get; set; }
        [JsonProperty(PropertyName = "latency_ms")]
        
        [Column(Name = "latency_ms"), NotNull]
        public int LatencyMilliseconds { get; set; }
        [JsonProperty(PropertyName = "uptime_seconds")]

        [Column(Name = "uptime_seconds"), NotNull]
        public long UptimeSeconds { get; set; }
        [JsonProperty(PropertyName = "version")]
        
        [Column(Name = "version"), NotNull]
        public string Version { get; set; }
    }
}