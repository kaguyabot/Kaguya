using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System.IO;

namespace KaguyaProjectV2.KaguyaBot.Core.Logger
{
    public class Logger
    {
        private static string logDirectory = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Resources\\Logs\\Debug";
        private static string logFileName = $"KaguyaLog_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";

        public static Task Log(string message, LogLevel logLevel)
        {
            string _logP = LogPrefix(logLevel);
            string contents = $"{DateTime.Now.ToLongTimeString()} {_logP} {message}";

            if (GlobalProperties.logLevel <= logLevel)
            {
                //Logs to console only if the log level is less or equally severe to what is specified in the config.
                Console.WriteLine(contents);
            }

            //Logs to file regardless of log level.
            if (LogFileExists())
            {
                File.AppendAllText($"{logDirectory}\\{logFileName}", $"{contents}\n");
            }
            return Task.CompletedTask;
        }

        private static string LogPrefix(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    return "[DEBUG]:";
                case LogLevel.INFO:
                    return "[INFO]:";
                case LogLevel.WARN:
                    return "[WARNING]:";
                case LogLevel.ERROR:
                    return "[ERROR]:";
                case LogLevel.FATAL:
                    return "[FATAL]:";
                default: return null;
            }
        }

        private static bool LogFileExists()
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                return false;
            }
            if(!File.Exists($"{logDirectory}\\{logFileName}"))
            {
                File.WriteAllText($"{logDirectory}\\{logFileName}", "");
                return false;
            }

            return true;
        }
    }
}