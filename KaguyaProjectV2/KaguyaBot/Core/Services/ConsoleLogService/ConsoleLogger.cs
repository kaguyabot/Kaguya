using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using Microsoft.Extensions.Primitives;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService
{
    public class ConsoleLogger
    {
        private static readonly string LogDirectory = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Resources\\Logs\\Debug";
        private static readonly string LogFileName = $"KaguyaLog_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to be logged to the console.</param>
        /// <param name="logLevel">The severity of the message. Higher severity log messages get special coloring in the console.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task Log(string message, LogLevel logLevel, ICommandContext context = null)
        {
            string logP = LogPrefix(logLevel);
            string contents = $"{DateTime.Now.ToLongTimeString()} {logP} {message}";

            if (context != null)
            {
                int shardId = Global.ConfigProperties.client.GetShardIdFor(context.Guild);
                contents = $"\n{DateTime.Now.ToLongTimeString()} {logP} " +
                           $"Command: [{context.Message}]\nUser: [Name: {context.User} | ID: {context.User.Id}]\n" +
                           $"Guild: [Name: {context.Guild} | ID: {context.Guild.Id} | Shard: {shardId}]\n" +
                           $"Channel: [Name: {context.Channel.Name} | ID: {context.Channel.Id}]\n";
            }

            if (Global.ConfigProperties.logLevel > logLevel) return Task.CompletedTask;

            //Logs to console only if the log level is less or equally severe to what is specified in the config.
            SetConsoleColor(logLevel);
            Console.WriteLine(contents);

            if(LogFileExists())
                File.AppendAllText($"{LogDirectory}\\{LogFileName}", $"{contents}\n");

            return Task.CompletedTask;
        }

        private static string LogPrefix(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.TRACE:
                    return "[TRACE]:";
                case LogLevel.DEBUG:
                    return "[DEBUG]:";
                case LogLevel.INFO:
                    return "[INFO]:";
                case LogLevel.WARN:
                    return "[WARNING]:";
                case LogLevel.ERROR:
                    return "[ERROR]:";
                default: return null;
            }
        }

        private static bool LogFileExists()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
                return false;
            }
            if(!File.Exists($"{LogDirectory}\\{LogFileName}"))
            {
                File.WriteAllText($"{LogDirectory}\\{LogFileName}", "");
                return false;
            }

            return true;
        }

        private static void SetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.TRACE:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;
            }
        }
    }
}