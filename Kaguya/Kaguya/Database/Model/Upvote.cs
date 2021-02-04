using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class Upvote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public bool IsWeekend { get; set; }
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Any extra arguments provided by the top.gg webhook - can be null.
        /// </summary>
        public string ExtraArgs { get; set; }
    }
}