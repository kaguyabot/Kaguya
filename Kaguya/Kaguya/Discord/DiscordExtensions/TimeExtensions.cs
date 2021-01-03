using System;
using System.Globalization;
using Humanizer;
using Humanizer.Localisation;

namespace Kaguya.Discord.DiscordExtensions
{
    public static class TimeExtensions
    {
        // TODO: Replace all implementations of the below humanization with this extension method.
        public static string HumanizeTraditionalReadable(this TimeSpan t)
        {
            return t.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
        }

        public static DateTime GetCurrentDaylightSavingsUtcTime()
        {
            // Assuming current timezone is EST.
            return DateTime.Now.IsDaylightSavingTime()
                ? DateTime.Now.AddHours(5)
                : DateTime.Now.AddHours(4);
        }
    }
}