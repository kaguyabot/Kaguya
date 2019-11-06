using System;
using System.Collections.Generic;
using System.Text;

/*
    MISSING PROPERTIES FOR ANTIRAID
*/

namespace KaguyaProjectV2.KaguyaBot.Core.Accounts.Servers
{
    public class Server
    {
        string Name { get; set; }
        uint ID { get; set; }
        string CommandPrefix { get; set; }
        Dictionary<uint, long> MutedMembers { get; set; } //Dictionary<UserID, Duration>
        Dictionary<int, int> WarnActions { get; set; } //Int amt of warnings, Int warnAction (perhaps use other type)
        Dictionary<uint, int> WarnedUsers { get; set; } //Dictionary<UserID, AmtofWarnings>
        Dictionary<uint, List<string>> PunishmentHistory { get; set; } //Dictionary<PunishedUserID, List<Reasons> 
                                                                        //containing all server user punishment history.
        List<string> FilteredPhrases { get; set; }
        Dictionary<uint, int> AutoAssignedRoles { get; set; } //Dictionary<RoleID, Duration> -- Duration == time before role is assigned.
        List<uint> BlacklistedChannels { get; set; }
        int TotalCommandCount { get; set; }
        int LogDeletedMessages { get; set; }
        int LogUpdatedMessages { get; set; }
        int LogUserJoins { get; set; }
        int LogUserLeaves { get; set; }
        int LogBans { get; set; }
        int LogUnbans { get; set; }
        int LogShadowbans { get; set; }
        int LogUnshadowbans { get; set; }
        int LogWarns { get; set; }
        int LogUnwarns { get; set; }
        int LogKaguyaSettingChanges { get; set; }
        int LogVoiceChannelConnections { get; set; }
        int LogVoiceChannelDisconnections { get; set; }
        int LogLevelAnnouncements { get; set; }
        int LogAntiraids { get; set; }
        bool IsBlacklisted { get; set; }
        bool AutoWarnOnBlacklistedPhrase { get; set; }

        public Server()
        {
            Name = "";
            ID = 0;
            CommandPrefix = "$";
            MutedMembers = new Dictionary<uint, long>();
            WarnActions = new Dictionary<int, int>();
            WarnedUsers = new Dictionary<uint, int>();
            PunishmentHistory = new Dictionary<uint, List<string>>();
            FilteredPhrases = new List<string>();
            AutoAssignedRoles = new Dictionary<uint, int>();
            BlacklistedChannels = new List<uint>();
            TotalCommandCount = 0;
            LogDeletedMessages = 0;
            LogUpdatedMessages = 0;
            LogUserJoins = 0;
            LogUserLeaves = 0;
            LogBans = 0;
            LogUnbans = 0;
            LogShadowbans = 0;
            LogUnshadowbans = 0;
            LogWarns = 0;
            LogUnwarns = 0;
            LogKaguyaSettingChanges = 0;
            LogVoiceChannelConnections = 0;
            LogVoiceChannelDisconnections = 0;
            LogLevelAnnouncements = 0;
            LogAntiraids = 0;
            IsBlacklisted = false;
            AutoWarnOnBlacklistedPhrase = false;
        }
    }
}