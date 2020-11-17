using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "reminders")]
    public class Reminder : IKaguyaQueryable<Reminder>, IUserSearchable<Reminder>
    {
        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "expiration")]
        [NotNull]
        public double Expiration { get; set; }

        [Column(Name = "text")]
        [NotNull]
        public string Text { get; set; }

        [Column(Name = "has_triggered")]
        [NotNull]
        public bool HasTriggered { get; set; }

        /// <summary>
        /// FK_KaguyaUser_Reminders
        /// </summary>
        [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
        public User User { get; set; }
    }
}