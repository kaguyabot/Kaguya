using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "rep")]
    public class Rep : IKaguyaQueryable<Rep>, IKaguyaUnique<Rep>, IUserSearchable<Rep>
    {
        [Column(Name = "UserId")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "GivenBy")]
        [NotNull]
        public ulong GivenBy { get; set; }

        [Column(Name = "TimeGiven")]
        [NotNull]
        public double TimeGiven { get; set; }

        [Column(Name = "Reason")]
        [NotNull]
        public string Reason { get; set; }
    }
}