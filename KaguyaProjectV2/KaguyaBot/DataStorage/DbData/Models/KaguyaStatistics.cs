using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using System;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "stats")]
    public class KaguyaStatistics : IKaguyaQueryable<KaguyaStatistics>
    {
        [Column(Name = "KaguyaUserCount")]
        public int KaguyaUserCount { get; set; }

        [Column(Name = "GuildCount")]
        public int GuildCount { get; set; }

        [Column(Name = "GuildUserCount")]
        public int GuildUserCount { get; set; }

        [Column(Name = "ShardCount")]
        public int ShardCount { get; set; }

        [Column(Name = "CommandCount")]
        public int CommandCount { get; set; }

        [Column(Name = "FishCaught")]
        public int FishCaught { get; set; }

        [Column(Name = "TotalPoints")]
        public long TotalPoints { get; set; }

        [Column(Name = "TimeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}