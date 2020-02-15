using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "userblacklists")]
    public class UserBlacklist : IKaguyaQueryable<UserBlacklist>, IKaguyaUnique<UserBlacklist>, IUserSearchable<UserBlacklist>
    {
        [PrimaryKey]
        public ulong UserId { get; set; }
        [Column(Name = "Expiration"), NotNull]
        public double Expiration { get; set; }
        [Column(Name = "Reason"), NotNull]
        public string Reason { get; set; }

        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public User User { get; set; }
    }
}
