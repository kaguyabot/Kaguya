using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "twitch")]
    public class TwitchChannel : IKaguyaQueryable<TwitchChannel>, IServerSearchable<TwitchChannel>
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "TextChannelId"), NotNull]
        public ulong TextChannelId { get; set; }
        [Column(Name = "ChannelName"), NotNull]
        public string ChannelName { get; set; }
        [Column(Name = "MentionEveryone"), NotNull]
        public bool MentionEveryone { get; set; }
        /// <summary>
        /// FK_KaguyaServer_Twitch
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}