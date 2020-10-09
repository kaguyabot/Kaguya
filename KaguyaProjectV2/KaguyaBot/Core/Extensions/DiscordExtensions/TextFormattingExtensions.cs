using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions
{
    public static class TextFormattingExtensions
    {
        public static string ToDiscordBold(this string s) => $"**{s}**";
        public static string ToDiscordUnderlined(this string s) => $"__{s}__";
        public static string ToDiscordItalics(this string s) => $"*{s}*";
        public static string ToDiscordUnderlinedBold(this string s) => $"__**{s}**__";
        public static string ToDiscordBackticks(this string s) => $"`{s}`";
        public static string ToDiscordCodeblock(this string s) => $"```{s}```";
    }
}