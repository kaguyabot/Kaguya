using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "reminders")]
    public class Reminder : IKaguyaQueryable<Reminder>, IUserSearchable<Reminder>
    {
        [Column(Name = "UserId")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "Expiration")]
        [NotNull]
        public double Expiration { get; set; }

        [Column(Name = "Text")]
        [NotNull]
        public string Text { get; set; }

        [Column(Name = "HasTriggered")]
        [NotNull]
        public bool HasTriggered { get; set; }

        /// <summary>
        /// FK_KaguyaUser_Reminders
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }
}