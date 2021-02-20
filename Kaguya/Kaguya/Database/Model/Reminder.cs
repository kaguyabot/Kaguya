using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class Reminder
    {
        /// <summary>
        /// The unique identifier of this reminder.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The userId this reminder belongs to.
        /// </summary>
        public ulong UserId { get; set; }
        /// <summary>
        /// The text that the user set and the text that we notifiy the user with when
        /// the expiration period has arrived.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// When (in the future) this reminder needs to be sent to the user.
        /// </summary>
        public DateTimeOffset Expiration { get; set; }
        /// <summary>
        /// Whether the reminder has been sent to the user.
        /// </summary>
        public bool HasTriggered { get; set; }
    }
}