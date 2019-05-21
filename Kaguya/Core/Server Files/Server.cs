using Discord;
using Discord.WebSocket;
using Kaguya.Modules.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Dictionary<List<SocketGuildUser>, int> WarnedMembers { get; set; }
        public List<string> FilteredWords { get; set; }
        public ulong LogDeletedMessages { get; set; }
        public ulong LogUpdatedMessages { get; set; }
        public ulong LogWhenUserJoins { get; set; }
        public ulong LogWhenUserLeaves { get; set; }
        public ulong LogWhenUserIsBanned { get; set; }
        public ulong LogWhenUserIsUnbanned { get; set; }
        public ulong LogChangesToLogSettings { get; set; }
        public ulong LogWhenUserSaysFilteredPhrase { get; set; }
        public ulong LogWhenUserConnectsToVoiceChannel { get; set; }
        public ulong LogWhenUserDisconnectsFromVoiceChannel { get; set; }
        public ulong LogLevelUpAnnouncements { get; set; }
        public List<string> JoinedUsers { get; set; }
        public bool IsBlacklisted { get; set; }
        public bool BlackJackInProgress { get; set; }
        public Server(ulong id)
        {
            ID = id;
            ServerName = "";
            commandPrefix = "$";
            MessageAnnouncements = true;
            MutedMembers = new Dictionary<string, string>();
            WarnedMembers = new Dictionary<List<SocketGuildUser>, int>();
            WarnActions = new Dictionary<string, int>();
            FilteredWords = new List<string>();
            IsBlacklisted = false;
            JoinedUsers = new List<string>();
        }
    }
}
