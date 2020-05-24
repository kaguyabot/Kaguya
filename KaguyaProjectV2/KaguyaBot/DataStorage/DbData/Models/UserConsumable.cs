using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "user_items_consumable")]
    public class UserConsumable : IKaguyaQueryable<UserConsumable>, IUserSearchable<UserConsumable>
    {
        /// <summary>
        /// The ID of the user in which the item belongs to.
        /// </summary>
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        /// <summary>
        /// The type of <see cref="ConsumableItem"/> that this object represents. This value is stored as an integer in the database.
        /// </summary>
        [Column(Name = "Item"), NotNull]
        public ConsumableItem ConsumableItem { get; set; }
        /// <summary>
        /// The name of the item Enum, formatted in all caps. <code>Item.ToString()</code>
        /// </summary>
        [Column(Name = "ItemName"), NotNull]
        public string ItemName { get; set; }
        /// <summary>
        /// An integer determining the strength of the consumable. Default value is 1.
        /// </summary>
        [Column(Name = "Rank"), NotNull]
        public int Rank { get; set; }
        /// <summary>
        /// The length, in seconds, of how long this consumable's effect will last.
        /// </summary>
        [Column(Name = "Duration"), NotNull]
        public long Duration { get; set; }
        /// <summary>
        /// The value of the DateTime, in OLE Automation (OA) Date form.
        /// </summary>
        [Column(Name = "Expiration"), NotNull]
        public double Expiration { get; set; }
        /// <summary>
        /// Whether the item has been consumed by the user. This should be update whenever the user consumes the item.
        /// </summary>
        [Column(Name = "HasConsumed"), NotNull]
        public bool HasConsumed { get; set; }
    }

    public enum ConsumableItem
    {
        /// <summary>
        /// Potion that grants a greatly reduced fishing cooldown.
        /// </summary>
        POTION_OF_INSTANT_GRATIFICATION
    }
}
