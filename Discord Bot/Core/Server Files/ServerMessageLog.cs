using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Core.Server_Files
{
    public class ServerMessageLog
    {
        public ulong ID { get; set; }

        public string ServerName { get; set; }

        public List<string> LastThousandMessages { get; set; }

        public ServerMessageLog(ulong id)
        {
            ID = id;
            ServerName = "";
            LastThousandMessages = new List<string>();
        }

        public void AddMessage(SocketUserMessage msg)
        {
            LastThousandMessages.Add($"#{LastThousandMessages.Count} ℀ Author: {msg.Author} ℀ Channel: #{msg.Channel} ℀ Message: {msg.Content} ℀ MsgID: {msg.Id} ℀ Time: {DateTime.Now}");
            if(LastThousandMessages.Count == 1000)
            {
                LastThousandMessages.RemoveAt(0);
            }
        }
    }
}
