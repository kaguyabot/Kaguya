using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices
{
    public class ConsoleLogger
    {
        private static readonly string _logDirectory = $"{ConfigProperties.KaguyaMainFolder}\\Resources\\Logs";
        private static readonly string _logFileName = $"KaguyaLog_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";

        /// <summary>
        ///     Logs a message to the console and the log file.
        /// </summary>
        /// <param name="message">The <see cref="string" /> to display inside of the console.</param>
        /// <param name="logLevel">The <see cref="LogLvl" /> that determines this log's severity.</param>
        /// <param name="foregroundColor">Assuming we override the colors, this will alter the color of the text shown in the console.</param>
        /// <param name="displaySeverity">Whether to display the date and time in the console.</param>
        /// <param name="showDate">Whether to display the date and time in the console.</param>
        /// <returns></returns>
        public static async Task LogAsync(string message,
            LogLvl logLevel,
            ConsoleColor foregroundColor = ConsoleColor.White,
            bool displaySeverity = true,
            bool showDate = true)
        {
            string logP = LogPrefix(logLevel);
            string dateString = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}";
            string contents = $"{(showDate ? $"{dateString} " : "")}{(displaySeverity ? $"{logP} " : "")}{message}";

            // If the loglevel provided in the Config is only set to display more severe logs, return.
            if (ConfigProperties.LogLevel > logLevel) return;

            // If the color wasn't overridden by the caller...
            if (foregroundColor == ConsoleColor.White)
            {
                foregroundColor = GetConsoleForegroundColor(logLevel);
            }
            await LogFinisher(contents, foregroundColor);
        }

        /// <summary>
        ///     Logs a Discord command to the console. This log message is special in that the format is completely different from other log messages.
        /// </summary>
        /// <param name="context">The <see cref="ICommandContext" /> that we will use to gather command data from.</param>
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

            await LogFinisher(contents);
        }

        /// <summary>
        ///     Asynchronously logs a Discord <see cref="CommandService" /> log message to the console and log file.
        ///     This is generally used for capturing thrown command exceptions.
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="cmdException"></param>
        /// <returns></returns>
        public static async Task LogAsync(LogMessage logMsg, CommandException cmdException)
        {
            string logP = LogPrefix(LogLvl.ERROR);
            string contents = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {logP} Exception thrown when executing command " +
                              $"{cmdException.Command.Name} in guild {cmdException.Context.Guild.Id} by user {cmdException.Context.User.Id}:\n" +
                              $"Inner Exception Message: {cmdException.InnerException?.Message ?? "NULL"}\n" +
                              $"Stack Trace: {logMsg.Exception.StackTrace ?? "NULL"}";

            await LogFinisher(contents);
        }

        public static async Task LogAsync(Exception e, LogLvl logLvl = LogLvl.ERROR)
        {
            Type type = e.GetType();

            if (type == typeof(KaguyaSupportException) || type == typeof(KaguyaPremiumException))
                return;

            string logP = LogPrefix(LogLvl.ERROR);
            string contents = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {logP} Exception thrown: " +
                              $"{e.Message}.\nInner Exception Message: {e.InnerException?.Message ?? "NULL"}\n" +
                              $"Stack Trace: {e.StackTrace ?? "NULL"}";

            await LogFinisher(contents);
        }

        public static async Task LogAsync(Exception e, string additionalInfo, LogLvl logLvl = LogLvl.ERROR)
        {
            Type type = e.GetType();

            if (type == typeof(KaguyaSupportException) || type == typeof(KaguyaPremiumException))
                return;

            string logP = LogPrefix(LogLvl.ERROR);
            string contents = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {logP} Exception thrown: " +
                              $"{e.Message}.\nInner Exception Message: {e.InnerException?.Message ?? "NULL"}\n" +
                              $"Stack Trace: {e.StackTrace ?? "NULL"}\nAdditional Information: {additionalInfo}";

            await LogFinisher(contents);
        }

        private static string LogPrefix(LogLvl logLevel) => logLevel switch
        {
            LogLvl.TRACE => "[TRACE]:",
            LogLvl.DEBUG => "[DEBUG]:",
            LogLvl.INFO => "[INFO]:",
            LogLvl.WARN => "[WARNING]:",
            LogLvl.ERROR => "[ERROR]:",
            _ => null
        };

        private static bool LogFileExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);

                return false;
            }

            if (File.Exists($"{_logDirectory}\\{_logFileName}")) return true;

            File.WriteAllText($"{_logDirectory}\\{_logFileName}", "");

            return false;
        }

        private static ConsoleColor GetConsoleForegroundColor(LogLvl level)
        {
            ConsoleColor color;
            switch (level)
            {
                case LogLvl.TRACE:
                    color = ConsoleColor.DarkGray;

                    break;
                case LogLvl.DEBUG:
                    color = ConsoleColor.Gray;

                    break;
                case LogLvl.INFO:
                    color = ConsoleColor.White;

                    break;
                case LogLvl.WARN:
                    color = ConsoleColor.Yellow;

                    break;
                case LogLvl.ERROR:
                    color = ConsoleColor.Red;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            return color;
        }

        private static async Task LogFinisher(string contents, ConsoleColor foregroundColor = ConsoleColor.White)
        {
            //Logs to console only if the log level is less or equally severe to what is specified in the config.
            SetConsoleForegroundColor(foregroundColor);
            Console.WriteLine(contents);

            if (LogFileExists())
            {
                try
                {
                    await File.AppendAllTextAsync($"{_logDirectory}\\{_logFileName}", $"{contents}\n");
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        private static void SetConsoleForegroundColor(ConsoleColor c) => Console.ForegroundColor = c;
    }
}