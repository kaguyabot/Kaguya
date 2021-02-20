using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class Quote
    {
        /// <summary>
        /// Unique, generated + auto-incremented ID.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// The server ID this quote was recorded in.
        /// </summary>
        public ulong ServerId { get; set; }
        /// <summary>
        /// The ID of the user who recorded this quote.
        /// </summary>
        public ulong UserId { get; set; }
        /// <summary>
        /// The quote's text.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// When the quote was recorded.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}