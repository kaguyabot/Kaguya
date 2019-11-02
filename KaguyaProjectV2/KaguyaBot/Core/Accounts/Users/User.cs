using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Accounts.Users
{
    public class User
    {
        string Username { get; set; }
        uint ID { get; set; }
        int Level
        {
            get
            {
                return (int)Math.Sqrt(EXP / 8 + 8);
            }
        }
        int EXP { get; set; }
        int Points { get; set; }
        int Diamonds { get; set; }
        int Rep { get; set; }
        int TotalCommandUses { get; set; }
        int NSFWImages { get; set; }
        int ActiveRateLimit { get; set; }
        int RateLimitWarnings { get; set; }
        int TotalGamblingWins { get; set; }
        int TotalGamblingLosses { get; set; }
        int TotalCurrencyAwarded { get; set; }
        int TotalCurrencyLost { get; set; }
        int TotalRollWins { get; set; }
        int TotalRollLosses { get; set; }
        int TotalRolls
        {
            get
            {
                return TotalRollWins + TotalRollLosses;
            }
        }
        int TotalQuickdrawWins { get; set; }
        int TotalQuickdrawLosses { get; set; }
        int TotalQuickdraws
        {
            get
            {
                return TotalQuickdrawWins + TotalQuickdrawLosses;
            }
        }
        List<String> GambleHistory { get; set; }
        List<String> CommandHistory { get; set; }
        long BlacklistExpiration { get; set; }
        long LastReceivedEXP { get; set; }
        long LastReceivedTimelyBonus { get; set; }
        long LastReceivedWeeklyBonus { get; set; }
        long LastGivenRep { get; set; }
        long UpvoteBonusExpiration { get; set; }
        long KaguyaSupporterExpiration { get; set; }
        bool IsSupporter
        {
            get
            {
                return KaguyaSupporterExpiration > DateTime.Now.ToOADate();
            }
        }
        bool IsBenefitingFromUpvote
        {
            get
            {
                return UpvoteBonusExpiration > DateTime.Now.ToOADate();
            }
        }
        bool IsBlacklisted { get; set; }

        public User()
        {
            Username = "";
            ID = 0;
            EXP = 0;
            Points = 0;
            Diamonds = 0;
            Rep = 0;
            TotalCommandUses = 0;
            NSFWImages = 0;
            ActiveRateLimit = 0;
            RateLimitWarnings = 0;
            TotalGamblingWins = 0;
            TotalGamblingLosses = 0;
            TotalCurrencyAwarded = 0;
            TotalCurrencyLost = 0;
            TotalRollWins = 0;
            TotalRollLosses = 0;
            TotalQuickdrawWins = 0;
            TotalQuickdrawLosses = 0;
            GambleHistory = new List<String>();
            CommandHistory = new List<String>();
            BlacklistExpiration = 0;
            LastReceivedEXP = 0;
            LastReceivedTimelyBonus = 0;
            LastReceivedTimelyBonus = 0;
            LastReceivedWeeklyBonus = 0;
            LastGivenRep = 0;
            UpvoteBonusExpiration = 0;
            KaguyaSupporterExpiration = 0;
            IsBlacklisted = false;
        }
    }
}
