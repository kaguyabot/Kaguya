using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "antiraid")]
    public class AntiRaid
    {
        [PrimaryKey]
        public ulong ServerId { get; set; }
        [Column(Name = "Users")]
        public int Users { get; set; }
        [Column(Name = "Seconds")]
        public int Seconds { get; set; }
        [Column(Name = "Action")]
        public string Action { get; set; }

        [Association(ThisKey = "ServerId", OtherKey = "Id")]
        public Server Server { get; set; }
    }
}
