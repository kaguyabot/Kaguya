using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "praise")]
    public class Praise
    {
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "GivenBy"), NotNull]
        public ulong GivenBy { get; set; }
        [Column(Name = "TimeGiven"), NotNull]
        public double TimeGiven { get; set; }
        [Column(Name = "Reason"), NotNull]
        public string Reason { get; set; }

        [Association(ThisKey = "ServerId", OtherKey = "Id")]
        public Server Server { get; set; }
    }
}