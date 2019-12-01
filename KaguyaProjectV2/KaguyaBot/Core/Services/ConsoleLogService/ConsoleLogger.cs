using System;
using System.IO;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService
{
    public class ConsoleLogger
    {
        private static readonly string logDirectory = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Resources\\Logs\\Debug";
        private static readonly string logFileName = $"KaguyaLog_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";

        public static Task Log(string message, LogLevel logLevel)
        {
            string logP = LogPrefix(logLevel);
            string contents = $"{DateTime.Now.ToLongTimeString()} {logP} {message}";

            if (Global.ConfigProperties.logLevel <= logLevel)
            {
                //Logs to console only if the log level is less or equally severe to what is specified in the config.
                Console.WriteLine(contents);

                if(LogFileExists())
                    File.AppendAllText($"{logDirectory}\\{logFileName}", $"{contents}\n");
            }

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