using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "owner_giveaway_reactions")]
    public class OwnerGiveawayReaction : 
        IKaguyaQueryable<OwnerGiveawayReaction>, 
        IUserSearchable<OwnerGiveawayReaction>
    {
        [Column(Name = "owner_giveaway_id"), NotNull]
        public int OwnerGiveawayId { get; set; }
        [Column(Name = "user_id"), NotNull]
        public ulong UserId { get; set; }
    }
}