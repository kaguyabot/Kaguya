using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "warnedusers")]
    public class WarnedUser : IKaguyaQueryable<WarnedUser>, IUserSearchable<WarnedUser>, IServerSearchable<WarnedUser>
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "ModeratorName"), NotNull]
        public string ModeratorName { get; set; }
        [Column(Name = "Reason"), NotNull]
        public string Reason { get; set; }
        [Column(Name = "Date"), NotNull]
        public double Date { get; set; }
        /// <summary>
        /// FK_KaguyaServer_WarnedUsers
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}
