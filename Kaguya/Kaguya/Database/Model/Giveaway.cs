﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class Giveaway
    {
        /// <summary>
        /// Unique ID, generated and auto-incremented.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// ID of the server that this giveaway is held in.
        /// </summary>
        public ulong ServerId { get; set; }
        /// <summary>
        /// ID of the channel that this giveaway is held in.
        /// </summary>
        public ulong ChannelId { get; set; }
        /// <summary>
        /// ID of the message that this giveaway is attached to.
        /// </summary>
        public ulong MessageId { get; set; }
        /// <summary>
        /// The amount of exp to award a user who participates in this giveaway.
        /// </summary>
        public int? Exp { get; set; }
        /// <summary>
        /// The amount of points to award a user who participates in this giveaway.
        /// </summary>
        public int? Points { get; set; }
        /// <summary>
        /// The item to give away
        /// </summary>
        public string Item { get; set; }
        /// <summary>
        /// The amount of items to give away.
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// When the giveaway expires.
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}