using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Kaguya.Core.Server_Files
{
    public class ServerMessageLog
    {
        public ulong ID { get; set; }
        public string ServerName { get; set; }
        public List<string> LastFiveHundredMessages { get; set; }
        public ServerMessageLog(ulong id)
        {
            ID = id;
            ServerName = "";
            LastFiveHundredMessages = new List<string>();
        }

        public void AddMessage(SocketUserMessage msg)
        {
            LastFiveHundredMessages.Add($"#{LastFiveHundredMessages.Count} ℀ Author: {msg.Author} ℀ Channel: #{msg.Channel} ℀ Message: {msg.Content} ℀ MsgID: {msg.Id} ℀ Time: {DateTime.Now}");
            if(LastFiveHundredMessages.Count >= 500)
            {
                LastFiveHundredMessages.RemoveAt(0);
            }
        }
    }
}
