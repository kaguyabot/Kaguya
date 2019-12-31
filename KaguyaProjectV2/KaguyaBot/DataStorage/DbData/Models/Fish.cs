using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "fish")]
    public class Fish
    {
        [PrimaryKey]
        public long FishId { get; set; }
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "TimeCaught"), NotNull]
        public double TimeCaught { get; set; }
        [Column(Name = "Fish"), NotNull]
        public FishType FishType { get; set; }
        [Column(Name = "FishString")] 
        public string FishString { get; set; }
        [Column(Name = "Value"), NotNull]
        public int Value { get; set; }
        [Column(Name = "Sold"), NotNull]
        public bool Sold { get; set; }
    }

    public enum FishType
    {
        BIG_KAHUNA, // 0.05%
        GIANT_SQUID, // 0.15%
        ORANTE_SLEEPER_RAY, // 0.30%
        DEVILS_HOLE_PUPFISH, // 0.5%
        SMALLTOOTH_SAWFISH, // 1%
        GIANT_SEA_BASS, // 3%
        TRIGGERFISH, // 5%
        RED_DRUM, // 5%
        LARGE_SALMON, // 7%
        LARGE_BASS, // 7%
        CATFISH, // 9%
        SMALL_SALMON, // 10%
        SMALL_BASS, // 10%
        PINFISH, // 12%
        SEAWEED, // 15%
        BAIT_STOLEN // 15%
    }
}
