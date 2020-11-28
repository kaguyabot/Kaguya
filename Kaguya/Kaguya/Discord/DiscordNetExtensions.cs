using System;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord
{
    public static class DiscordNetExtensions
    {
        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Debug => LogLevel.Debug,
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };
        }

        /// <summary>
        /// Returns the full name of the command. This is the group name + any additional commands in the group.
        /// Example: [Group("ban")] [Command("-u")] for a command returns "ban -u".
        /// </summary>
        /// <param name="cmdInfo"></param>
        /// <returns></returns>
        public static string GetFullCommandName(this CommandInfo cmdInfo) => cmdInfo.Aliases[0];
    }
}