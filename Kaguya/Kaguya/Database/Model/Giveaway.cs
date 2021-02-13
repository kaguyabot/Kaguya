using System;
using System.ComponentModel.DataAnnotations;

namespace Kaguya.Database.Model
{
    public class Giveaway
    {
        /// <summary>
        /// ID of the message that this giveaway is attached to. There can only be
        /// 1 giveaway attached to a message id.
        /// </summary>
        [Key]
        public ulong MessageId { get; set; }
        /// <summary>
        /// ID of the server that this giveaway is held in.
        /// </summary>
        public ulong ServerId { get; set; }
        /// <summary>
        /// ID of the channel that this giveaway is held in.
        /// </summary>
        public ulong ChannelId { get; set; }
        /// <summary>
        /// The amount of exp to award a user who participates in this giveaway.
        /// </summary>
        public int? Exp { get; set; }
        /// <summary>
        /// The amount of coins to award a user who participates in this giveaway.
        /// </summary>
        public int? Coins { get; set; }
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