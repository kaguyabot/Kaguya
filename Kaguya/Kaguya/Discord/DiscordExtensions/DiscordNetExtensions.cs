using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.DiscordExtensions
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

        /// <summary>
        /// Compares two <see cref="IUser"/> objects for equality, based only on user id.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsEqual(this IUser user, IUser other) => user.Id == other.Id;

        public static async Task<IUserMessage> SendEmbedAsync(this IUser user, EmbedBuilder embedBuilder) => await user.SendMessageAsync(embed: embedBuilder.Build());
        public static async Task<IUserMessage> SendEmbedAsync(this IUser user, Embed embed) => await user.SendMessageAsync(embed: embed);
    }
}