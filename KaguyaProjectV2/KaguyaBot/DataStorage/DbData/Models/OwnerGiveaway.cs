using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "owner_giveaway")]
    public class OwnerGiveaway : IKaguyaQueryable<OwnerGiveaway>,
        IKaguyaUnique<OwnerGiveaway>,
        IMemoryCacheable<OwnerGiveaway>
    {
        /// <summary>
        /// The id of the owner giveaway. This is solely managed by the database.
        /// </summary>
        [Column(Name = "id")]
        [PrimaryKey]
        [NotNull]
        [Identity]
        public int Id { get; set; }

        /// <summary>
        /// The id of the message that this giveaway is attached to.
        /// </summary>
        [Column(Name = "message_id")]
        [NotNull]
        public ulong MessageId { get; set; }

        /// <summary>
        /// The id of the channel that the giveaway's message lives in.
        /// </summary>
        [Column(Name = "channel_id")]
        [NotNull]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The amount of Exp each user is awarded by participating in this giveaway.
        /// </summary>
        [Column(Name = "exp")]
        [NotNull]
        public int Exp { get; set; }

        /// <summary>
        /// The amount of Points each user is awarded by participating in this giveaway.
        /// </summary>
        [Column(Name = "points")]
        [NotNull]
        public int Points { get; set; }

        /// <summary>
        /// When this owner giveaway expires, in OADate form.
        /// </summary>
        [Column(Name = "expiration")]
        [NotNull]
        public double Expiration { get; set; }

        /// <summary>
        /// Whether or not the embedded message has been marked as expired.
        /// </summary>
        [Column(Name = "has_expired")]
        [NotNull]
        public bool HasExpired { get; set; }
    }
}