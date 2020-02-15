﻿using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using KaguyaProjectV2.KaguyaBot.Core.Commands.EXP;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using MoreLinq.Experimental;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "kaguyauser")]
    public class User : IKaguyaQueryable<User>, IKaguyaUnique<User>, IUserSearchable<User>
    {
        [PrimaryKey]
        public ulong UserId { get; set; }
        [Column(Name = "Experience"), NotNull]
        public int Experience { get; set; }
        [Column(Name = "FishExp"), NotNull]
        public int FishExp { get; set; }
        [Column(Name = "Points"), NotNull]
        public int Points { get; set; }
        [Column(Name = "OsuId"), NotNull]
        public int OsuId { get; set; }
        [Column(Name = "OsuBeatmapsLinked"), NotNull]
        public int OsuBeatmapsLinked { get; set; }
        [Column(Name = "CommandUses"), NotNull]
        public int TotalCommandUses { get; set; }
        [Column(Name = "TotalDaysSupported"), NotNull]
        public int TotalDaysSupported { get; set; }
        [Column(Name = "NSFWImages"), NotNull] 
        public int TotalNSFWImages { get; set; } = 12;
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
        [Column(Name = "TotalUpvotes"), NotNull]
        public int TotalUpvotes { get; set; }
        [Column(Name = "LastGivenExp"), NotNull]
        public double LastGivenExp { get; set; }
        [Column(Name = "LastDailyBonus"), NotNull]
        public double LastDailyBonus { get; set; }
        [Column(Name = "LastGivenRep"), NotNull]
        public double LastGivenRep { get; set; }
        [Column(Name = "LastRatelimited"), NotNull]
        public double LastRatelimited { get; set; }
        [Column(Name = "LastFished"), NotNull]
        public double LastFished { get; set; }

        /// <summary>
        /// If a user wants to receive level-up notifications in chat, what type should it be?
        /// </summary>
        [Column(Name = "ExpChatNotificationType"), NotNull]
        public int ExpChatNotificationTypeNum { private get; set; } = 2;

        /// <summary>
        /// If a user wants to receive level-up notifications in their DMs, what type should it be?
        /// </summary>
        [Column(Name = "ExpDMNotificationType"), NotNull]
        public int ExpDmNotificationTypeNum { private get; set; } = 3;

        public bool IsBlacklisted => Blacklist != null && Blacklist.Expiration - DateTime.Now.ToOADate() > 0;
        public ExpType ExpChatNotificationType => (ExpType) ExpChatNotificationTypeNum;
        public ExpType ExpDmNotificationType => (ExpType) ExpDmNotificationTypeNum;

        public double SupporterExpirationDate
        {
            get
            {
                var now = DateTime.Now.ToOADate();
                var allUserKeys = DatabaseQueries.GetAllAsync<SupporterKey>(x => x.UserId == UserId).Result;
                if (allUserKeys.Count > 0)
                    return now + allUserKeys.Sum(key => key.Expiration - now);
                return DateTime.MinValue.ToOADate();
            }
        }

        public bool IsBotOwner => UserId == ConfigProperties.BotConfig.BotOwnerId;
        public bool IsSupporter => SupporterExpirationDate - DateTime.Now.ToOADate() > 0;
        public bool CanGiveRep => LastGivenRep < DateTime.Now.AddHours(-24).ToOADate();
        public bool CanGetDailyPoints => LastDailyBonus < DateTime.Now.AddHours(-24).ToOADate();
        public IEnumerable<Praise> Praise => DatabaseQueries.GetAllForUserAsync<Praise>(UserId).Result;

        /// <summary>
        /// FK_UserBlacklists_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public UserBlacklist Blacklist { get; set; }

        /// <summary>
        /// FK_CommandHistory_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<CommandHistory> CommandHistory { get; set; }

        /// <summary>
        /// FK_Fish_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Fish> Fish { get; set; }

        /// <summary>
        /// FK_GambleHistory_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<GambleHistory> GambleHistory { get; set; }

        /// <summary>
        /// FK_KaguyaUser_Reminder
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Reminder> Reminders { get; set; }

        /// <summary>
        /// FK_Rep_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Rep> Rep { get; set; }

        /// <summary>
        /// FK_ServerExp_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<ServerExp> ServerExp { get; set; }

        /// <summary>
        /// FK_SupporterKeys_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<SupporterKey> SupporterKeys { get; set; }

        /// <summary>
        /// FK_Upvotes_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Upvote> Upvotes { get; set; }
    }
}
