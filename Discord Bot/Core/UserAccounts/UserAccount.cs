using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Core.UserAccounts
{
    public class UserAccount
    {
        public ulong ID { get; set; }

        public uint Points { get; set; }

        public uint EXP { get; set; }

        public DateTime LastReceivedEXP { get; set; }

        public DateTime LastReceivedTimelyPoints { get; set; }

        public uint LevelNumber
        {
            get
            {
                return (uint)Math.Sqrt(EXP / 20);
            }
        }
    }
}
