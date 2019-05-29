using System;
using System.Collections.Generic;

namespace Kaguya.Core.UserAccounts
{
    public class UserAccount
    {
        public string Username { get; set; }
        public ulong ID { get; set; }
        public List<string> IsInServers { get; set; }
        public List<ulong> IsInServerIDs { get; set; }
        public uint Points { get; set; }
        public uint EXP { get; set; }
        public uint KaguyaDiamonds { get; set; }
        public int Rep { get; set; }
        public int KaguyaWarnings { get; set; }
        public DateTime LastReceivedEXP { get; set; }
        public DateTime LastReceivedTimelyPoints { get; set; }
        public DateTime LastGivenRep { get; set; }
        public DateTime LastReceivedWeeklyPoints { get; set; }
        public DateTime LastUpvotedKaguya { get; set; }
        public DateTime KaguyaSupporterExpiration { get; set; }
        public uint LevelNumber
        {
            get
            {
                return (uint)Math.Sqrt(EXP / 8 + -8);
            }
        }
        
        public string OsuUsername { get; set; }
        public int Blacklisted { get; set; }
        public int TotalDailyGambleWinnings { get; set; }
        public double LifetimeGambleWins { get; set; }
        public double LifetimeGambleLosses { get; set; }
        public double LifetimeGambles { get; set; }
        public int LifetimeEliteRolls { get; set; }

        public UserAccount(ulong id)
        {
            ID = id;
            IsInServers = new List<string>();
            IsInServerIDs = new List<ulong>();
            Points = 0;
            EXP = 0;
            KaguyaDiamonds = 0;
            Rep = 0;
            Blacklisted = 0;
            LifetimeGambleWins = 0;
            LifetimeGambleLosses = 0;
            LifetimeEliteRolls = 0;
            KaguyaWarnings = 0;
        }

        public void AddSName(string server)
        {
            IsInServers.Add(server);
        }

        public void AddSID(ulong serverID)
        {
            IsInServerIDs.Add(serverID);
        }

    }

}
