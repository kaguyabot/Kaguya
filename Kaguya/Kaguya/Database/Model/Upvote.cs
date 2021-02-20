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
        /// <summary>
        /// ID of the user who made the upvote
        /// </summary>
        public ulong UserId { get; set; }
        /// <summary>
        /// ID of the bot that received a vote
        /// </summary>
        public ulong BotId { get; set; }
        /// <summary>
        /// Whether the vote was made on a weekend
        /// </summary>
        public bool IsWeekend { get; set; }
        /// <summary>
        /// When the vote was made
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
        /// <summary>
        /// Any extra arguments provided by the top.gg webhook - can be null.
        /// </summary>
        public string QueryParams { get; set; }
        /// <summary>
        /// Whether the user has been reminded that their upvote cooldown has expired.
        /// </summary>
        public bool ReminderSent { get; set; }
        /// <summary>
        /// The type of upvote. Can be "upvote" or "test"
        /// </summary>
        public string Type { get; set; }
    }
}