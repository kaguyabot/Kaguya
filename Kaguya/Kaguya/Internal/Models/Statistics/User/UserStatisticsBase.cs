using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using Kaguya.Database.Model;
using Kaguya.Internal.Services;

namespace Kaguya.Internal.Models.Statistics.User
{
    public abstract class UserStatisticsBase : IUserStatistics
    {
        private readonly KaguyaUser _user;
        protected UserStatisticsBase(KaguyaUser user)
        {
            _user = user;
        }

        public abstract RestUser RestUser { get; }

        public abstract IList<Fish> AllFish { get; }
        public abstract int GrossCoinsFromFishing { get; }
        public abstract int NetCoinsFishing { get; }
        public abstract IList<(FishRarity rarity, int count)> RaritiesCount { get; }
        public abstract Task<bool> HasRecentlyVotedAsync(TimeSpan threshold);
        public abstract int RepGiven { get; }
        public abstract int RepReceived { get; }

        public int FishExp => _user.FishExp;
        public int TotalGambles => _user.TotalGambles;
        public int TotalCoinsEarnedGambling => _user.TotalGambleWins;
        public int TotalCoinsLostGambling => _user.TotalGambleLosses;
        public int TotalCoinsGambled => _user.TotalCoinsGambled;
        public int TotalGambleWins => _user.TotalGambleWins;
        public int TotalGambleLosses => _user.TotalGambleLosses;
        public int NetCoinsGambling => _user.NetGambleCoingEarnings;
        public double PercentWinGambling => _user.TotalGambleWins / (double) _user.TotalGambleLosses;
        public bool EligibleToVote => _user.CanUpvote;
        public int TotalVotesTopGg => _user.TotalUpvotesTopGg;
        public int TotalVotesDiscordBoats => _user.TotalUpvotesDiscordBoats;
        public int Coins => _user.Coins;
        public int DaysPremium => _user.TotalDaysPremium;
    }
}