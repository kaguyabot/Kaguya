using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "twitch")]
    public class TwitchChannel
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
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