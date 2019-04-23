using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kaguya;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Kaguya.Core
{
    public class Logger
    {
        /// <summary>Console logging event for successfully executed command.</summary>
        public void ConsoleCommandLog(SocketCommandContext context, long timeSpan)
        {
            Console.ForegroundColor = ConsoleColor.White;

            string cmd = context.Message.Content.Split(' ').First();
            Console.WriteLine($"\nUser: [{context.User.Username}#{context.User.Discriminator}] Command: [{cmd}]" +
                $"\nGuild: [{context.Guild.Name} | {context.Guild.Id}] " +
                $"Channel: [#{context.Channel.Name} | {context.Channel.Id}]" +
                $"\nTime: [{DateTime.Now}] | Executed After: [{timeSpan.ToString("N0")} milliseconds]" +
                $"\nMessage: [{context.Message.Content}]");
        }

        /// <summary>Console logging event for unsuccessfully executed command.</summary>
        public void ConsoleCommandLog(SocketCommandContext context, long timeSpan, CommandError error, string errorMsg)
        {
            if (error.Equals(CommandError.Unsuccessful))
            {
                Console.ForegroundColor = ConsoleColor.Red;
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

        /// <summary>Console logging event for command advisories. Usually this is for dangerous administrative actions.</summary>
        public void ConsoleCommandLog(SocketCommandContext context, long timeSpan, string warningMsg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            string cmd = context.Message.Content.Split(' ').First();
            Console.WriteLine($"\nCOMMAND WARNING:" +
                $"\nADVISORY MESSAGE: [\"{warningMsg}\"]" +
                $"\nUser: [{context.User.Username}#{context.User.Discriminator}] Command: [{cmd}]" +
                $"\nGuild: [{context.Guild.Name} | {context.Guild.Id}] " +
                $"Channel: [#{context.Channel.Name} | {context.Channel.Id}]" +
                $"\nTime: [{DateTime.Now}] | Executed After: [{timeSpan.ToString("N0")} milliseconds]" +
                $"\nMessage: [{context.Message.Content}]");
        }

        /// <summary>Console logging event for general status advisory messages/updates, such as when the _client.SetGameAsync() task is executed.</summary>
        public void ConsoleStatusAdvisory(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nSTATUS ADVISORY:" +
                $"\nADVISORY MESSAGE: [\"{message}\"]" +
                $"\nTime: [{DateTime.Now}]");
        }

        /// <summary>Console logging event for critical errors.</summary>
        public void ConsoleCriticalAdvisory(Exception e, string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\nCRITICAL ERROR:" +
                $"\nERROR MESSAGE: [\"{message}\"]" +
                $"\nEXCEPTION: [\"{e.Message}\"]" +
                $"\nTime: [{DateTime.Now}]");
        }

        /// <summary>Console logging event for guild connection advisories, such as joining a new guild or disconnecting from a guild.</summary>
        public void ConsoleGuildConnectionAdvisory(SocketGuild guild, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nINFORMATION:" +
                $"\nMESSAGE: [\"{message}\"]" +
                $"\nGuild: [{guild.Name} | {guild.Id}] " +
                $"\nTime: [{DateTime.Now}]");
        }

        /// <summary>Console logging event for when Kaguya changes a text channel's permissions.</summary>
        public void ConsoleGuildAdvisory(SocketGuild guild, SocketGuildChannel channel, string message) 
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nINFORMATION:" +
                $"\nMESSAGE: [\"{message}\"]" +
                $"\nChannels Affected: {channel.Name} | {channel.Id}" +
                $"\nGuild: [{guild.Name} | {guild.Id}] " +
                $"\nTime: [{DateTime.Now}]");
        }

        /// <summary>Console logging event for advisories that affect guild users, such as when a user is banned from a guild.</summary>
        public void ConsoleGuildAdvisory(SocketGuild guild, SocketGuildUser user, string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nINFORMATION:" +
                $"\nMESSAGE: [\"{message}\"]" +
                $"\nGuild: [{guild.Name} | {guild.Id}]" +
                $"\nUser: [{user.Username}#{user.Discriminator} | {user.Id}]" +
                $"\nTime: [{DateTime.Now}]");
        }
    }
}
