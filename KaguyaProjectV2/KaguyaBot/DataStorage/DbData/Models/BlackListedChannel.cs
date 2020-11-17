using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "blacklistedchannels")]
    public class BlackListedChannel : IKaguyaQueryable<BlackListedChannel>, IServerSearchable<BlackListedChannel>,
        IKaguyaUnique<BlackListedChannel>
    {
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "channel_id")]
        [NotNull]
        public ulong ChannelId { get; set; }

        [Column(Name = "expiration")]
        [NotNull]
        public double Expiration { get; set; }

        /// <summary>
        /// FK_KaguyaServer_BlackListedChannels
        /// </summary>
        [Association(ThisKey = "server_id", OtherKey = "id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}