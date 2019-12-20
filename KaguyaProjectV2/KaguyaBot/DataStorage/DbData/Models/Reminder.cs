using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "reminders")]
    public class Reminder
    {
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Expiration"), NotNull]
        public double Expiration { get; set; }
        [Column(Name = "Text"), NotNull]
        public string Text { get; set; }
        [Column(Name = "HasTriggered"), NotNull]
        public bool HasTriggered { get; set; }

        /// <summary>
        /// FK_KaguyaUser_Reminders
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }
}
