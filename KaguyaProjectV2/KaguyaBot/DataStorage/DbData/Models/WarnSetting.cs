using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "warn_settings")]
    public class WarnSetting : IKaguyaQueryable<WarnSetting>, IKaguyaUnique<WarnSetting>, IServerSearchable<WarnSetting>
    {
        [PrimaryKey]
        [Column(Name = "server_id")]
        public ulong ServerId { get; set; }

        [Column(Name = "mute")]
        [NotNull]
        public int Mute { get; set; }

        [Column(Name = "kick")]
        [NotNull]
        public int Kick { get; set; }

        [Column(Name = "shadowban")]
        [NotNull]
        public int Shadowban { get; set; }

        [Column(Name = "ban")]
        [NotNull]
        public int Ban { get; set; }
    }
}