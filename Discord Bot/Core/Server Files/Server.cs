using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Core.Server_Files
{
    public class Server
    {
        public string ServerName { get; set; }

        public ulong ID { get; set; } 

        public string commandPrefix { get; set; }
    }
}
