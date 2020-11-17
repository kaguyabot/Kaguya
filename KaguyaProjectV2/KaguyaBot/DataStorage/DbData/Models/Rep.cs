using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "rep")]
    public class Rep : IKaguyaQueryable<Rep>, IKaguyaUnique<Rep>, IUserSearchable<Rep>
    {
        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "given_by")]
        [NotNull]
        public ulong GivenBy { get; set; }

        [Column(Name = "time_given")]
        [NotNull]
        public double TimeGiven { get; set; }

        [Column(Name = "reason")]
        [NotNull]
        public string Reason { get; set; }
        
        [Association(ThisKey = "user_id", OtherKey = "user_id", CanBeNull = false)]
        public User User { get; set; }
    }
}