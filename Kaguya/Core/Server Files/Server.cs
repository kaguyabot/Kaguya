using System.Collections.Generic;

namespace Kaguya.Core.Server_Files
{
    public class Server
    {
        public ulong ID { get; set; }
        public string ServerName { get; set; }
        public string CommandPrefix { get; set; }
        public bool MessageAnnouncements { get; set; }
        public Dictionary<string, string> MutedMembers { get; set; }
        public Dictionary<string, int> WarnActions { get; set; }
        public Dictionary<ulong, int> WarnedMembers { get; set; }
        public Dictionary<ulong, List<string>> PunishmentHistory { get; set; } //List of reasons for being warned, per warned user.
        public bool AntiRaid { get; set; }
        public string AntiRaidPunishment { get; set; }
        public int AntiRaidSeconds { get; set; }
        public int AntiRaidCount { get; set; }
        public List<ulong> AntiRaidList { get; set; }
        public List<string> FilteredWords { get; set; }
        public List<ulong> AutoAssignedRoles { get; set; }
        public List<ulong> BlacklistedChannels { get; set; }
        public List<ulong> WhitelistedChannels { get; set; }
        public ulong LogDeletedMessages { get; set; }
        public ulong LogUpdatedMessages { get; set; }
        public ulong LogWhenUserJoins { get; set; }
        public ulong LogWhenUserLeaves { get; set; }
        public ulong LogBans { get; set; }
        public ulong LogUnbans { get; set; }
        public ulong LogShadowbans { get; set; }
        public ulong LogUnShadowbans { get; set; }
        public ulong LogChangesToLogSettings { get; set; }
        public ulong LogWhenUserSaysFilteredPhrase { get; set; }
        public ulong LogWhenUserConnectsToVoiceChannel { get; set; }
        public ulong LogWhenUserDisconnectsFromVoiceChannel { get; set; }
        public ulong LogLevelUpAnnouncements { get; set; }
        public ulong LogAntiRaids { get; set; }
        public bool IsBlacklisted { get; set; }
        public string MostRecentBanReason { get; set; }
        public string MostRecentShadowbanReason { get; set; }
        public bool IsPurgingMessages { get; set; }
        public Server(ulong id)
        {
            ID = id;
            ServerName = "";
            CommandPrefix = "$";
            MessageAnnouncements = true;
            MutedMembers = new Dictionary<string, string>();
            WarnedMembers = new Dictionary<ulong, int>();
            WarnActions = new Dictionary<string, int>();
            PunishmentHistory = new Dictionary<ulong, List<string>>();
            BlacklistedChannels = new List<ulong>();
            WhitelistedChannels = new List<ulong>();
            FilteredWords = new List<string>();
            AutoAssignedRoles = new List<ulong>();
            AntiRaidList = new List<ulong>();
            AntiRaid = false;
            AntiRaidSeconds = 0;
            AntiRaidCount = 0;
            AntiRaidPunishment = null;
            IsBlacklisted = false;
            IsPurgingMessages = false;
        }
    }
}
