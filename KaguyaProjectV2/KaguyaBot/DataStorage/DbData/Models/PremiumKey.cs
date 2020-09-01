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
        [Column(Name = "Key"), NotNull]
        public string Key { get; set; }
        [Column(Name = "LengthInSeconds"), NotNull]
        public long LengthInSeconds { get; set; }
        [Column(Name = "KeyCreatorId"), NotNull]
        public ulong KeyCreatorId { get; set; }
        [Column(Name = "UserId"), Nullable]
        public ulong UserId { get; set; }
        [Column(Name = "ServerId"), Nullable]
        public ulong ServerId { get; set; }
    }
}
