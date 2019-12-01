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
        public static Task Log(string message, LogLevel logLevel)
        {
            string logP = LogPrefix(logLevel);
            string contents = $"{DateTime.Now.ToLongTimeString()} {logP} {message}";

            if (Global.ConfigProperties.logLevel > logLevel) return Task.CompletedTask;
            return LogFinisher(logLevel, contents);
        }

        public static Task Log(ICommandContext context, LogLevel logLevel)
        {
            string logP = LogPrefix(logLevel);

            int shardId = Global.ConfigProperties.client.GetShardIdFor(context.Guild);
            string contents = $"\n{DateTime.Now.ToLongTimeString()} {logP} " +
                       $"Command: [{context.Message}]\nUser: [Name: {context.User} | ID: {context.User.Id}]\n" +
                       $"Guild: [Name: {context.Guild} | ID: {context.Guild.Id} | Shard: {shardId}]\n" +
                       $"Channel: [Name: {context.Channel.Name} | ID: {context.Channel.Id}]\n";

            return LogFinisher(logLevel, contents);
        }

        private static string LogPrefix(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.TRACE => "[TRACE]:",
                LogLevel.DEBUG => "[DEBUG]:",
                LogLevel.INFO => "[INFO]:",
                LogLevel.WARN => "[WARNING]:",
                LogLevel.ERROR => "[ERROR]:",
                _ => null
            };
        }

        private static bool LogFileExists()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
                return false;
            }

            if (File.Exists($"{LogDirectory}\\{LogFileName}")) return true;

            File.WriteAllText($"{LogDirectory}\\{LogFileName}", "");
            return false;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private static Task LogFinisher(LogLevel logLevel, string contents)
        {
            //Logs to console only if the log level is less or equally severe to what is specified in the config.
            SetConsoleColor(logLevel);
            Console.WriteLine(contents);

            if (LogFileExists())
                File.AppendAllText($"{LogDirectory}\\{LogFileName}", $"{contents}\n");

            return Task.CompletedTask;
        }
    }
}