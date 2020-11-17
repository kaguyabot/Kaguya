using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "muted_users")]
    public class MutedUser : IKaguyaQueryable<MutedUser>,
        IServerSearchable<MutedUser>,
        IUserSearchable<MutedUser>,
        IKaguyaUnique<MutedUser>
    {
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        /// <summary>
        /// When the mute will expire, in OADate form.
        /// </summary>
        [Column(Name = "expires_at")]
        [NotNull]
        public double ExpiresAt { get; set; }

        /// <summary>
        /// FK_KaguyaServer_MutedUsers
        /// </summary>
        [Association(ThisKey = "server_id", OtherKey = "id", CanBeNull = false)]
        public Server Server { get; set; }

        /// <summary>
        /// FK_KaguyaUser_MutedUsers
        /// </summary>
        [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
        public User User { get; set; }
    }
}