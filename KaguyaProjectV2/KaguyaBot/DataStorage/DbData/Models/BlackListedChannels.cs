using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "blacklistedchannels")]
    public class BlackListedChannels
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "ChannelId"), NotNull]
        public ulong ChannelId { get; set; }
        /// <summary>
        /// FK_KaguyaServer_WarnActions
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}