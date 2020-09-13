using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;
using System;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "premiumkeys")]
    public class PremiumKey : IKey, IKaguyaQueryable<PremiumKey>,
        IKaguyaUnique<PremiumKey>,
        IServerSearchable<PremiumKey>,
        IUserSearchable<PremiumKey>
    {
        [PrimaryKey]
        [Column(Name = "key"), NotNull]
        public string Key { get; set; }
        [Column(Name = "length_in_seconds"), NotNull]
        public long LengthInSeconds { get; set; }
        [Column(Name = "key_creator_id"), NotNull]
        public ulong KeyCreatorId { get; set; }
        [Column(Name = "user_id"), Nullable]
        public ulong UserId { get; set; }
        [Column(Name = "server_id"), Nullable]
        public ulong ServerId { get; set; }
        [Column(Name = "has_expired")]
        public bool HasExpired { get; set; }
    }
}
