using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "ServerMutedUsers")]
    public class MutedUsers
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Duration"), NotNull]
        public long Duration { get; set; }
        /// <summary>
        /// FK_KaguyaServer_MutedUsers
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
        /// <summary>
        /// FK_KaguyaUser_MutedUsers
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }

}
