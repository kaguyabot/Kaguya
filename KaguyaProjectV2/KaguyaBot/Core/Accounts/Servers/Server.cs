using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Accounts.Servers
{
    public class Server
    {
        string Name { get; set; }
        uint ID { get; set; }
        string CommandPrefix { get; set; }
        Dictionary<ulong, long> MutedMembers { get; set; } //Dictionary<UserID, Duration>

        //Continue tomorrow.
    }
}
