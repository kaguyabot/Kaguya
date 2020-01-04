using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "blacklistedchannels")]
    public class BlackListedChannel : IKaguyaQueryable<BlackListedChannel>, IServerSearchable<BlackListedChannel>
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "ChannelId"), NotNull]
        public ulong ChannelId { get; set; }
        [Column(Name = "Expiration"), NotNull]
        public double Expiration { get; set; }
        /// <summary>
        /// FK_KaguyaServer_BlackListedChannels
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}