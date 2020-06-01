using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "user_items_tools")]
    public class UserTool: IKaguyaQueryable<UserTool>, IUserSearchable<UserTool>
    {
        /// <summary>
        /// The ID of the user that the item belongs to.
        /// </summary>
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        /// <summary>
        /// The <see cref="int"/> value that the item's Enum corresponds to.
        /// </summary>
        [Column(Name = "Tool"), NotNull]
        public Tool Tool { get; set; }
        /// <summary>
        /// The name of the item's Enum printed with the all-caps format.
        /// </summary>
        [Column(Name = "ToolName"), NotNull]
        public string ToolName { get; set; }

        /// <summary>
        /// Determines the strength of the item. Default value is 1.
        /// </summary>
        [Column(Name = "Rank"), NotNull]
        public int Rank { get; set; }
        /// <summary>
        /// The item's durability: when this value is zero, the item is broken and can no longer be used.
        /// </summary>
        [Column(Name = "CurrentDurability"), NotNull]
        public int CurrentDurability { get; set; }
        /// <summary>
        /// The maximum possible durability for the item.
        /// </summary>
        [Column(Name = "MaxDurability"), NotNull]
        public int MaxDurability { get; set; }
    }

    public enum Tool
    {
        /// <summary>
        /// A fishing rod that grants a higher chance to catch rarer fish.
        /// </summary>
        DEEP_SEA_FISHING_ROD,
        /// <summary>
        /// Reduced fishing cooldown.
        /// </summary>
        BASSMASTER_HAT
    }
}