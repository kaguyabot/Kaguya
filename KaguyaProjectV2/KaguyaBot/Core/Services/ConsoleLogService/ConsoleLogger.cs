using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService
{
    public class ConsoleLogger
    {
        private static readonly string LogDirectory = $"{ConfigProperties.KaguyaMainFolder}\\Resources\\Logs";
        private static readonly string LogFileName = $"KaguyaLog_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";

        /// <summary>
        /// Logs a message to the console and the log file.
        /// </summary>
        /// <param name="message">The <see cref="string"/> to display inside of the console.</param>
        /// <param name="logLevel">The <see cref="LogLvl"/> that determines this log's severity.</param>
        /// <param name="colorOverride">Whether to override the console colors.
        /// These are normally automatically determined by the provided <see cref="logLevel"/></param>
        /// <param name="foregroundColor">Assuming we override the colors, this will alter the color of the text shown in the console.</param>
        /// <param name="backgroundColor">Modifies the background color </param>
        /// <param name="displaySeverity">Whether to display the date and time in the console.</param>
        /// <param name="showDate">Whether to display the date and time in the console.</param>
        /// <returns></returns>
        public static async Task LogAsync(string message, LogLvl logLevel, bool colorOverride = false,
             ConsoleColor backgroundColor = ConsoleColor.Black, ConsoleColor foregroundColor = ConsoleColor.White, bool displaySeverity = true,
            bool showDate = true)
        {
            string logP = LogPrefix(logLevel);
            string dateString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            string contents = $"{(showDate ? $"{dateString} " : "")}{(displaySeverity ? $"{logP} " : "")}{message}";

            if (colorOverride == false && foregroundColor != ConsoleColor.White || colorOverride == false && backgroundColor != ConsoleColor.Black)
                throw new InvalidOperationException("Cannot change the console colors with the \"colorOverride\" parameter set to false.");

            // If the loglevel provided in the Config is only set to display more severe logs, return.
            if (ConfigProperties.LogLevel > logLevel) return;

            await LogFinisher(logLevel, contents, colorOverride, backgroundColor, foregroundColor);
        }

        /// <summary>
        /// Logs a Discord command to the console. This log message is special in that the format is completely different from other log messages.
        /// </summary>
        /// <param name="context">The <see cref="ICommandContext"/> that we will use to gather command data from.</param>
        /// <param name="logLevel">The severity of this log message.</param>
        /// <returns></returns>
        public static async Task LogAsync(ICommandContext context, LogLvl logLevel = LogLvl.INFO)
        {
            string logP = LogPrefix(logLevel);

            int shardId = ConfigProperties.Client.GetShardIdFor(context.Guild);
            string contents = $"\n{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {logP} " +
                       $"Command: [{context.Message}]\nUser: [Name: {context.User} | ID: {context.User.Id}]\n" +
                       $"Guild: [Name: {context.Guild} | ID: {context.Guild.Id} | Shard: {shardId}]\n" +
                       $"Channel: [Name: {context.Channel.Name} | ID: {context.Channel.Id}]\n";

            await LogFinisher(logLevel, contents);
        }

        /// <summary>
        /// Asynchronously logs a Discord <see cref="CommandService"/> log message to the console and log file.
        /// This is generally used for capturing thrown command exceptions.
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="cmdException"></param>
        /// <returns></returns>
        public static async Task LogAsync(LogMessage logMsg, CommandException cmdException)
        {
            string logP = LogPrefix(LogLvl.ERROR);
            string contents = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} Exception thrown when executing command " +
                              $"{cmdException.Command.Name} in guild {cmdException.Context.Guild.Id} by user {cmdException.Context.User.Id}:\n" +
                              $"Message: {cmdException.Message}\n" +
                              $"Stack Trace: {logMsg.Exception.StackTrace}";
            await LogFinisher(LogLvl.ERROR, contents);
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

        private static async Task LogFinisher(LogLvl logLevel, string contents, bool colorOverride = false,
            ConsoleColor backgroundColor = ConsoleColor.Black, ConsoleColor foregroundColor = ConsoleColor.White)
        {
            //Logs to console only if the log level is less or equally severe to what is specified in the config.
            SetConsoleColor(logLevel);

            if (colorOverride)
            {
                Console.BackgroundColor = backgroundColor;
                Console.ForegroundColor = foregroundColor;
            }

            Console.WriteLine(contents);

            if (LogFileExists())
            {
                try
                {
                    await File.AppendAllTextAsync($"{LogDirectory}\\{LogFileName}", $"{contents}\n");
                }
                catch (Exception)
                {
                    //
                }
            }
        }
    }
}