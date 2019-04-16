using Discord;
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
        public List<string> FilteredWords { get; set; }
        public ulong LogDeletedMessages { get; set; }
        public ulong LogMessageEdits { get; set; }
        public ulong LogWhenUserJoins { get; set; }
        public ulong LogWhenUserLeaves { get; set; }
        public ulong LogWhenUserIsBanned { get; set; }
        public ulong LogWhenUserIsUnbanned { get; set; }
        public ulong LogChangesToLogSettings { get; set; }
        public ulong LogWhenUserSaysFilteredPhrase { get; set; }
        public ulong LogWhenUserConnectsToVoiceChannel { get; set; }
        public ulong LogWhenUserDisconnectsFromVoiceChannel { get; set; }
        public List<string> JoinedUsers { get; set; }
        public bool BlackJackInProgress { get; set; }
        public Server(ulong id)
        {
            ID = id;
            ServerName = "";
            commandPrefix = "$";
            MessageAnnouncements = true;
            FilteredWords = new List<string>();
            JoinedUsers = new List<string>();
        }
    }
}
