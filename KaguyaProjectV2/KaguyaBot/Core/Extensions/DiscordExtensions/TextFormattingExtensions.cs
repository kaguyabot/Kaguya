using System;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions
{
    public static class TextFormattingExtensions
    {
        public static string ToDiscordBold(this string str)
        {
            return $"**{str}**";
        }

        public static string ToDiscordUnderlinedBold(this string str)
        {
            return $"__**{str}**__";
        }
    }
}