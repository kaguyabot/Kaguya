using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService
{
    public class ConsoleLogger
    {
        private static readonly string LogDirectory = $"{ConfigProperties.KaguyaMainFolder}\\Resources\\Logs\\Debug";
        private static readonly string LogFileName = $"KaguyaLog_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to be logged to the console.</param>
        /// <param name="logLevel">The severity of the message. Higher severity log messages get special coloring in the console.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task LogAsync(string message, LogLvl logLevel)
        {
            string logP = LogPrefix(logLevel);
            string contents = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {logP} {message}";

            if (ConfigProperties.LogLevel > logLevel) return;
            await LogFinisher(logLevel, contents);
        }

        public static async Task LogAsync(ICommandContext context, LogLvl logLevel)
        {
            string logP = LogPrefix(logLevel);

            int shardId = ConfigProperties.Client.GetShardIdFor(context.Guild);
            string contents = $"\n{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {logP} " +
                       $"Command: [{context.Message}]\nUser: [Name: {context.User} | ID: {context.User.Id}]\n" +
                       $"Guild: [Name: {context.Guild} | ID: {context.Guild.Id} | Shard: {shardId}]\n" +
                       $"Channel: [Name: {context.Channel.Name} | ID: {context.Channel.Id}]\n";

            await LogFinisher(logLevel, contents);
        }

        private static string LogPrefix(LogLvl logLevel)
        {
            return logLevel switch
            {
                LogLvl.TRACE => "[TRACE]:",
                LogLvl.DEBUG => "[DEBUG]:",
                LogLvl.INFO => "[INFO]:",
                LogLvl.WARN => "[WARNING]:",
                LogLvl.ERROR => "[ERROR]:",
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

        private static void SetConsoleColor(LogLvl level)
        {
            switch (level)
            {
                case LogLvl.TRACE:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogLvl.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogLvl.INFO:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    break;
                case LogLvl.WARN:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogLvl.ERROR:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private static async Task LogFinisher(LogLvl logLevel, string contents)
        {
            //Logs to console only if the log level is less or equally severe to what is specified in the config.
            SetConsoleColor(logLevel);
            Console.WriteLine(contents);

            if (LogFileExists())
                await File.AppendAllTextAsync($"{LogDirectory}\\{LogFileName}", $"{contents}\n");
        }
    }
}