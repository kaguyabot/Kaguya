﻿using System;
using System.Collections.Generic;

namespace Kaguya.Core.UserAccounts
{
    public class UserAccount
    {
        public string Username { get; set; }
        public ulong ID { get; set; }
        public uint Points { get; set; }
        public uint EXP { get; set; }
        public uint Diamonds { get; set; }
        public int Rep { get; set; }
        public int KaguyaWarnings { get; set; }
        public int NBombUsesThisHour { get; set; }
        public int CommandRateLimit { get; set; }
        public int RatelimitStrikes { get; set; } //Normal command rate limit strikes
        public string NSFWAgeVerified { get; set; }
        public List<string> RecentlyUsedCommands { get; set; }
        public int GamblingBadLuckStreak { get; set; }
        public bool IsSupporter
        {
            get
            {
                return (KaguyaSupporterExpiration - DateTime.Now).TotalSeconds > 0;
            }
        }
        public DateTime TemporaryBlacklistExpiration { get; set; }
        public DateTime LastReceivedEXP { get; set; }
        public DateTime LastReceivedTimelyPoints { get; set; }
        public DateTime LastGivenRep { get; set; }
        public DateTime LastReceivedWeeklyPoints { get; set; }
        public DateTime LastUpvotedKaguya { get; set; }
        public DateTime KaguyaSupporterExpiration { get; set; }
        public DateTime NBombCooldownReset { get; set; }
        public uint LevelNumber
        {
            get
            {
                return (uint)Math.Sqrt(EXP / 8 + -8);
            }
        }
        
        public string OsuUsername { get; set; }
        public bool IsBlacklisted { get; set; }
        public double LifetimeGambleWins { get; set; }
        public double LifetimeGambleLosses { get; set; }
        public double LifetimeGambles { get; set; }
        public double LifetimeEliteRolls { get; set; }
        public int TotalCurrencyGambled { get; set; }
        public int TotalCurrencyAwarded { get; set; }
        public int TotalCurrencyLost { get; set; }
        public List<string> GambleHistory { get; set; }

        public UserAccount(ulong id)
        {
            Username = "";
            ID = id;
            Points = 0;
            EXP = 0;
            Diamonds = 0;
            Rep = 0;
            CommandRateLimit = 0;
            KaguyaWarnings = 0;
            NSFWAgeVerified = "false";
            OsuUsername = null;
            IsBlacklisted = false;
            LifetimeGambleWins = 0;
            LifetimeGambleLosses = 0;
            LifetimeGambles = 0;
            LifetimeEliteRolls = 0;
            RatelimitStrikes = 0;
            RecentlyUsedCommands = new List<string>();
            GambleHistory = new List<string>();
        }
    }

}
