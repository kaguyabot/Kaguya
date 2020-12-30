using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public string Text { get; set; }
        public DateTime Expiration { get; set; }
        public bool HasTriggered { get; set; }
        public bool NeedsDelivery => Expiration < DateTime.Now && !HasTriggered;
    }
}