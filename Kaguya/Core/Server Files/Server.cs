using System.Collections.Generic;

namespace Kaguya.Core.Server_Files
{
    public class Server
    {
        public ulong ID { get; set; }
        public string ServerName { get; set; }
        public string commandPrefix { get; set; }
        public bool MessageAnnouncements { get; set; }
        public Dictionary<string, string> MutedMembers { get; set; }
        public Dictionary<string, int> WarnActions { get; set; }
        public Dictionary<ulong, int> WarnedMembers { get; set; }
        public List<ulong> UsersJoinedLast30Seconds { get; set; }
        public bool AntiRaid { get; set; } = false;
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
        public List<string> JoinedUsers { get; set; }
        public bool IsBlacklisted { get; set; }
        public string MostRecentBanReason { get; set; }
        public string MostRecentShadowbanReason { get; set; }
        public Server(ulong id, string serverName)
        {
            ID = id;
            ServerName = serverName;
            commandPrefix = "$";
            MessageAnnouncements = true;
            MutedMembers = new Dictionary<string, string>();
            WarnedMembers = new Dictionary<ulong, int>();
            WarnActions = new Dictionary<string, int>();
            BlacklistedChannels = new List<ulong>();
            WhitelistedChannels = new List<ulong>();
            FilteredWords = new List<string>();
            AutoAssignedRoles = new List<ulong>();
            IsBlacklisted = false;
            JoinedUsers = new List<string>();
        }
    }
}
