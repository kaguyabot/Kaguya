using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "warnsettings")]
    public class WarnSetting
    {
        [PrimaryKey]
        public ulong ServerId { get; set; }
        [Column(Name = "Mute"), NotNull]
        public int Mute { get; set; }
        [Column(Name = "Kick"), NotNull]
        public int Kick { get; set; }
        [Column(Name = "Shadowban"), NotNull]
        public int Shadowban { get; set; }
        [Column(Name = "Ban"), NotNull]
        public int Ban { get; set; }
    }
}
