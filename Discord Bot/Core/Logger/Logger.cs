using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kaguya;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Discord_Bot.Core
{
    public class Logger
    {

        public void ConsoleCommandLog(SocketCommandContext context, long timeSpan)
        {
            Console.ForegroundColor = ConsoleColor.White;
            DiscordSocketClient _client = Global.Client;

            string cmd = context.Message.Content.Split(' ').First();
            Console.WriteLine($"\nUser: [{context.User.Username}#{context.User.Discriminator}] Command: [{cmd}]" +
                $"\nGuild: [{context.Guild.Name} | {context.Guild.Id}] " +
                $"Channel: [#{context.Channel.Name} | {context.Channel.Id}]" +
                $"\nTime: [{DateTime.Now}] | Executed After: [{timeSpan.ToString("N0")} milliseconds]" +
                $"\nMessage: [{context.Message.Content}]");
        }

        public void ConsoleCommandLog(SocketCommandContext context, long timeSpan, CommandError error, string errorMsg)
        {
            DiscordSocketClient _client = Global.Client;

            if (error.Equals(CommandError.Unsuccessful))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                string cmd = context.Message.Content.Split(' ').First();
                Console.WriteLine($"\nUSER COMMAND ERROR:" +
                    $"\nERROR MESSAGE: [\"{errorMsg}\"]" +
                    $"\nUser: [{context.User.Username}#{context.User.Discriminator}] Command: [{cmd}]" +
                    $"\nGuild: [{context.Guild.Name} | {context.Guild.Id}] " +
                    $"Channel: [#{context.Channel.Name} | {context.Channel.Id}]" +
                    $"\nTime: [{DateTime.Now}] | Executed After: [{timeSpan.ToString("N0")} milliseconds]" +
                    $"\nMessage: [{context.Message.Content}]");
            }
        }

        public void ConsoleCommandLog(SocketCommandContext context, long timeSpan, string advisoryMsg)
        {
            DiscordSocketClient _client = Global.Client;

            Console.ForegroundColor = ConsoleColor.Yellow;
            string cmd = context.Message.Content.Split(' ').First();
            Console.WriteLine($"\nCOMMAND EXECUTION ADVISORY:" +
                $"\nADVISORY MESSAGE: [\"{advisoryMsg}\"]" +
                $"\nUser: [{context.User.Username}#{context.User.Discriminator}] Command: [{cmd}]" +
                $"\nGuild: [{context.Guild.Name} | {context.Guild.Id}] " +
                $"Channel: [#{context.Channel.Name} | {context.Channel.Id}]" +
                $"\nTime: [{DateTime.Now}] | Executed After: [{timeSpan.ToString("N0")} milliseconds]" +
                $"\nMessage: [{context.Message.Content}]");
        }

    }
}
