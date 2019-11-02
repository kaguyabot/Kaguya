using System;
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
        public int NSFWUsesThisDay { get; set; }
        public int CommandRateLimit { get; set; }
        public int RatelimitStrikes { get; set; }
        public string NSFWAgeVerified { get; set; }
        public List<Dictionary<string, double>> Reminders { get; set; }
        public List<string> RecentlyUsedCommands { get; set; }
        public int GamblingBadLuckStreak { get; set; }
        public int QuickdrawWinnings { get; set; }
        public int QuickdrawLosses { get; set; }
        public bool IsSupporter
        {
            get
            {
                return (KaguyaSupporterExpiration - DateTime.Now).TotalSeconds > 0;
            }
        }
        public bool IsBenefitingFromUpvote
        {
            get
            {
                return (DateTime.Now - LastUpvotedKaguya).TotalSeconds > 0 && 
                    (DateTime.Now - LastUpvotedKaguya).TotalSeconds < 43200; //12 hours
            }
        }
        public DateTime TemporaryBlacklistExpiration { get; set; }
        public DateTime LastReceivedEXP { get; set; }
        public DateTime LastReceivedTimelyPoints { get; set; }
        public DateTime LastGivenRep { get; set; }
        public DateTime LastReceivedWeeklyPoints { get; set; }
        public DateTime LastUpvotedKaguya { get; set; }
        public DateTime KaguyaSupporterExpiration { get; set; }
        public DateTime NSFWCooldownReset { get; set; }
        public uint LevelNumber
        {
            get
            {
                return (uint)Math.Sqrt(EXP / 8 + -8); //DON'T TOUCH - THIS AFFECTS ALL LEVELS GLOBALLY!!
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
            Username = ""; //Global.client.GetUser(id).Username;
            ID = id;
            Points = 0;
            EXP = 0;
            Diamonds = 0;
            Rep = 0;
            CommandRateLimit = 0;
            KaguyaWarnings = 0;
            NSFWAgeVerified = "false";
            NSFWUsesThisDay = 0;
            OsuUsername = null;
            IsBlacklisted = false;
            LifetimeGambleWins = 0;
            LifetimeGambleLosses = 0;
            LifetimeGambles = 0;
            LifetimeEliteRolls = 0;
            RatelimitStrikes = 0;
            Reminders = new List<Dictionary<string, double>>();
            RecentlyUsedCommands = new List<string>();
            GambleHistory = new List<string>();
        }
    }
}