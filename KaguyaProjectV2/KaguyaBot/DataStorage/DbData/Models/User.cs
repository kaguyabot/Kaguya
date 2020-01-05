using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "kaguyauser")]
    public class User : IKaguyaQueryable<User>, IKaguyaUnique<User>, IUserSearchable<User>
    {
        [PrimaryKey]
        public ulong UserId { get; set; }
        [Column(Name = "Experience"), NotNull]
        public int Experience { get; set; }
        [Column(Name = "Points"), NotNull]
        public int Points { get; set; }
        [Column(Name = "OsuId"), NotNull]
        public int OsuId { get; set; }
        [Column(Name = "CommandUses"), NotNull]
        public int TotalCommandUses { get; set; }
        [Column(Name = "TotalDaysSupported"), NotNull]
        public int TotalDaysSupported { get; set; }
        [Column(Name = "NSFWImages"), NotNull]
        public int TotalNSFWImages { get; set; }
        /// <summary>
        /// Whenever a user uses a command, increase this by one.
        /// The ratelimit service will check for whether the user
        /// has too many commands allowed by the ratelimit (x cmds in y seconds).
        /// </summary>
        [Column(Name = "ActiveRateLimit"), NotNull]
        public int ActiveRateLimit { get; set; }
        [Column(Name = "RateLimitWarnings"), NotNull]
        public int RateLimitWarnings { get; set; }
        [Column(Name = "GamblingWins"), NotNull]
        public int TotalGamblingWins { get; set; }
        [Column(Name = "GamblingLosses"), NotNull]
        public int TotalGamblingLosses { get; set; }
        [Column(Name = "CurrencyAwarded"), NotNull]
        public int TotalCurrencyAwarded { get; set; }
        [Column(Name = "CurrencyLost"), NotNull]
        public int TotalCurrencyLost { get; set; }
        [Column(Name = "RollWins"), NotNull]
        public int TotalRollWins { get; set; }
        [Column(Name = "QuickdrawWins"), NotNull]
        public int TotalQuickdrawWins { get; set; }
        [Column(Name = "QuickdrawLosses"), NotNull]
        public int TotalQuickdrawLosses { get; set; }
        [Column(Name = "FishBait"), NotNull]
        public int FishBait { get; set; }
        [Column(Name = "TotalTradedFish"), NotNull]
        public int TotalTradedFish { get; set; }
        [Column(Name = "BlacklistExpiration"), NotNull]
        public double BlacklistExpiration { get; set; }
        [Column(Name = "LatestEXP"), NotNull]
        public double LatestExp { get; set; }
        [Column(Name = "LatestTimelyBonus"), NotNull]
        public double LatestTimelyBonus { get; set; }
        [Column(Name = "LatestWeeklyBonus"), NotNull]
        public double LatestWeeklyBonus { get; set; }
        [Column(Name = "LastGivenRep"), NotNull]
        public double LastGivenRep { get; set; }
        [Column(Name = "LastRatelimited"), NotNull]
        public double LastRatelimited { get; set; }
        [Column(Name = "LastFished"), NotNull]
        public double LastFished { get; set; }
        [Column(Name = "UpvoteBonusExpiration"), NotNull]
        public double UpvoteBonusExpiration { get; set; }
        public bool IsBlacklisted => BlacklistExpiration - DateTime.Now.ToOADate() > 0;
        

        public double SupporterExpirationDate
        {
            get
            {
                var now = DateTime.Now.ToOADate();
                var allUserKeys = UtilityQueries.GetSupporterKeysBoundToUserAsync(UserId).Result;
                return now + allUserKeys.Sum(key => key.Expiration - now);
            }
        }

        public bool IsSupporter => SupporterExpirationDate - DateTime.Now.ToOADate() > 0;
        public bool CanGiveRep => LastGivenRep < DateTime.Now.AddHours(-24).ToOADate();
        public bool CanGetTimelyPoints => DateTime.Now.AddHours(-24).ToOADate() < LatestTimelyBonus;
        public bool CanGetWeeklyPoints => DateTime.Now.AddHours(-24).ToOADate() < LatestWeeklyBonus;

        /// <summary>
        /// FK_KaguyaUser_GambleHistory_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<GambleHistory> GambleHistory { get; set; }

        /// <summary>
        /// FK_KaguyaUser_ServerExp_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "ServerId")]
        public IEnumerable<ServerExp> ServerExp { get; set; }

        /// <summary>
        /// FK_KaguyaUser_Reminder_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Reminder> Reminders { get; set; }
    }
}
