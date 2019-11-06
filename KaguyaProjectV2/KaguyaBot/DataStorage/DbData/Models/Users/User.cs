using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Accounts.Users
{
    public class User
    {
        uint ID { get; set; }
        //int Level
        //{
        //    get
        //    {
        //        return (int)Math.Sqrt(EXP / 8 + 8);
        //    }
        //}
        int EXP { get; set; }
        int Points { get; set; }
        int Diamonds { get; set; }
        int Rep { get; set; }
        int CommandUses { get; set; }
        int NSFWImages { get; set; }
        int ActiveRateLimit { get; set; }
        int RateLimitWarnings { get; set; }
        int GamblingWins { get; set; }
        int GamblingLosses { get; set; }
        int CurrencyAwarded { get; set; }
        int CurrencyLost { get; set; }
        int RollWins { get; set; }
        int RollLosses { get; set; }
        int QuickdrawWins { get; set; }
        int QuickdrawLosses { get; set; }
        List<String> GambleHistory { get; set; }
        List<String> CommandHistory { get; set; }
        long BlacklistExpiration { get; set; }
        long LastReceivedEXP { get; set; }
        long LastReceivedTimelyBonus { get; set; }
        long LastReceivedWeeklyBonus { get; set; }
        long LastGivenRep { get; set; }
        long UpvoteBonusExpiration { get; set; }
        long KaguyaSupporterExpiration { get; set; }
        //bool IsSupporter
        //{
        //    get
        //    {
        //        return KaguyaSupporterExpiration > DateTime.Now.ToOADate();
        //    }
        //}
        //bool IsBenefitingFromUpvote
        //{
        //    get
        //    {
        //        return UpvoteBonusExpiration > DateTime.Now.ToOADate();
        //    }
        //}
        bool IsBlacklisted { get; set; }

        public User()
        {
            ID = 0;
            EXP = 0;
            Points = 0;
            Diamonds = 0;
            Rep = 0;
            CommandUses = 0;
            NSFWImages = 0;
            ActiveRateLimit = 0;
            RateLimitWarnings = 0;
            GamblingWins = 0;
            GamblingLosses = 0;
            CurrencyAwarded = 0;
            CurrencyLost = 0;
            RollWins = 0;
            RollLosses = 0;
            QuickdrawWins = 0;
            QuickdrawLosses = 0;
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
