using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "upvotes")]
    public class Upvote : IKaguyaQueryable<Upvote>, IUserSearchable<Upvote>
    {
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Time"), NotNull]
        public double Time { get; set; }
        [Column(Name = "PointsAwarded"), NotNull]
        public int PointsAwarded { get; set; }
        [Column(Name = "ExpAwarded"), NotNull]
        public int ExpAwarded { get; set; }
        [Column(Name = "ReminderSent"), NotNull]
        public bool ReminderSent { get; set; }
    }
}
