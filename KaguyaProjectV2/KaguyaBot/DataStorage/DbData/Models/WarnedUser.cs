using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "warned_users")]
    public class WarnedUser : IKaguyaQueryable<WarnedUser>, IUserSearchable<WarnedUser>, IServerSearchable<WarnedUser>
    {
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "moderator_name")]
        [NotNull]
        public string ModeratorName { get; set; }

        [Column(Name = "reason")]
        [NotNull]
        public string Reason { get; set; }

        [Column(Name = "date")]
        [NotNull]
        public double Date { get; set; }

        /// <summary>
        /// FK_KaguyaServer_WarnedUsers
        /// </summary>
        [Association(ThisKey = "server_id", OtherKey = "server_id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}