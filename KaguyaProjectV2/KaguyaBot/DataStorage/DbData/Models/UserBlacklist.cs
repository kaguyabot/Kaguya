using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "user_blacklists")]
    public class UserBlacklist : IKaguyaQueryable<UserBlacklist>, IKaguyaUnique<UserBlacklist>, IUserSearchable<UserBlacklist>
    {
        [PrimaryKey]
        [Column(Name = "user_id")]
        public ulong UserId { get; set; }

        [Column(Name = "expiration")]
        [NotNull]
        public double Expiration { get; set; }

        [Column(Name = "reason")]
        [NotNull]
        public string Reason { get; set; }

        [Association(ThisKey = "user_id", OtherKey = "user_id")]
        public User User { get; set; }
    }
}