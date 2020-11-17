using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "antiraid")]
    public class AntiRaidConfig : IKaguyaQueryable<AntiRaidConfig>,
        IKaguyaUnique<AntiRaidConfig>,
        IServerSearchable<AntiRaidConfig>
    {
        [PrimaryKey]
        [Column(Name = "server_id")]
        public ulong ServerId { get; set; }

        [Column(Name = "users")]
        public int Users { get; set; }

        [Column(Name = "seconds")]
        public int Seconds { get; set; }

        [Column(Name = "action")]
        public string Action { get; set; }

        [Association(ThisKey = "server_id", OtherKey = "server_id")]
        public Server Server { get; set; }
    }
}