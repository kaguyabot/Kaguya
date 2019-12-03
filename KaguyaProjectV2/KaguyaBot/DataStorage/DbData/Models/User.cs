using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "kaguyauser")]
    public class User
    {
        [PrimaryKey]
        public ulong Id { get; set; }
        [Column(Name = "Experience"), NotNull]
        public int Experience { get; set; }
        [Column(Name = "Points"), NotNull]
        public int Points { get; set; }
        [Column(Name = "PremiumTokens"), NotNull]
        public int PremiumTokens { get; set; }
        [Column(Name = "Rep"), NotNull]
        public int Rep { get; set; }
        [Column(Name = "OsuId"), Nullable]
        public int OsuId { get; set; }
        [Column(Name = "CommandUses"), Nullable]
        public int TotalCommandUses { get; set; }
        [Column(Name = "NSFWImages"), Nullable]
        public int TotalNSFWImages { get; set; }
        [Column(Name = "ActiveRateLimit"), Nullable]
        public int ActiveRateLimit { get; set; }
        [Column(Name = "RateLimitWarnings"), Nullable]
        public int RateLimitWarnings { get; set; }
        [Column(Name = "GamblingWins"), Nullable]
        public int TotalGamblingWins { get; set; }
        [Column(Name = "GamblingLosses"), Nullable]
        public int TotalGamblingLosses { get; set; }
        [Column(Name = "CurrencyAwarded"), Nullable]
        public int TotalCurrencyAwarded { get; set; }
        [Column(Name = "CurrencyLost"), Nullable]
        public int TotalCurrencyLost { get; set; }
        [Column(Name = "RollWins"), Nullable]
        public int TotalRollWins { get; set; }
        [Column(Name = "QuickdrawWins"), Nullable]
        public int TotalQuickdrawWins { get; set; }
        [Column(Name = "QuickdrawLosses"), Nullable]
        public int TotalQuickdrawLosses { get; set; }
        [Column(Name = "BlacklistExpiration"), Nullable]
        public double BlacklistExpiration { get; set; }
        [Column(Name = "LatestEXP"), Nullable]
        public double LatestExp { get; set; }
        [Column(Name = "LatestTimelyBonus"), Nullable]
        public double LatestTimelyBonus { get; set; }
        [Column(Name = "LatestWeeklyBonus"), Nullable]
        public double LatestWeeklyBonus { get; set; }
        [Column(Name = "LastGivenRep"), Nullable]
        public double LastGivenRep { get; set; }
        [Column(Name = "UpvoteBonusExpiration"), Nullable]
        public double UpvoteBonusExpiration { get; set; }
        [Column(Name = "IsBlacklisted"), NotNull]
        public bool IsBlacklisted { get; set; }

        public double SupporterExpirationDate
        {
            get
            {
                var now = DateTime.Now.ToOADate();
                var allUserKeys = UtilityQueries.GetAllKeys().Where(x => x.UserId == Id);
                return now + allUserKeys.Sum(key => key.Expiration - now);
            }
        }
        /// <summary>
        /// FK_KaguyaUser_GambleHistory_BackReference
        /// </summary>
        [Association(ThisKey = "Id", OtherKey = "UserId")]
        public IEnumerable<GambleHistory> GambleHistory { get; set; }

        public bool IsSupporter => SupporterExpirationDate - DateTime.Now.ToOADate() > 0;
    }
}
