using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Humanizer.Localisation;
using LinqToDB.Common;

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class RegexTimeParser
    {
        /// <summary>
        /// Takes in a time string formatted as 0d0h0m0s and outs all of the individual values as integers.
        /// </summary>
        /// <param name="duration">The time string for the regex to parse (Format: 0d0h0m0s</param>
        /// <returns></returns>
        public static void Parse(string duration, out int sec, out int min, out int hour, out int day)
        {
            var regex = new Regex("/([0-9])*s|([0-9])*m|([0-9])*h|([0-9])*d/g");

            Regex[] regexs =
            {
                new Regex("(([0-9])*s)"),
                new Regex("(([0-9])*m)"),
                new Regex("(([0-9])*h)"),
                new Regex("(([0-9])*d)")
            };

            var s = regexs[0].Match(duration).Value;
            var m = regexs[1].Match(duration).Value;
            var h = regexs[2].Match(duration).Value;
            var d = regexs[3].Match(duration).Value;

            var seconds = s.Split('s').First();
            var minutes = m.Split('m').First();
            var hours = h.Split('h').First();
            var days = d.Split('d').First();

            if (!StringIsMatch(seconds) && !StringIsMatch(minutes) && !StringIsMatch(hours) &&
                !StringIsMatch(days))
            {
                throw new FormatException("You did not specify a proper mute time. \nThe proper format is " +
                                          "`<user> <dhms>`. \nExample: `<user> 30m`");
            }

            int.TryParse(seconds, out sec);
            int.TryParse(minutes, out min);
            int.TryParse(hours, out hour);
            int.TryParse(days, out day);

            if (seconds.Length > 10 || minutes.Length > 10 || hours.Length > 8 || days.Length > 7)
            {
                throw new ArgumentOutOfRangeException("Too many digits to process.",
                    new Exception());
            }
        }

        /// <summary>
        /// Parses a time duration in the format 0d0h0m0s into a TimeSpan object.
        /// Only one of the four time units has to be specified, and it does not
        /// matter which one.
        /// </summary>
        /// <param name="duration">The result of the parse as a TimeSpan object.</param>
        /// <returns></returns>
        public static TimeSpan ParseToTimespan(string duration)
        {
            Parse(duration, out int sec, out int min, out int hour, out int day);
            return new TimeSpan(day, hour, min, sec);
        }

        private static bool StringIsMatch(string s)
        {
            return !s.IsNullOrEmpty();
        }

        public static string FormattedTimeString(string duration)
        {
            return HumanizedString(ParseToTimespan(duration));
        }

        public static string FormattedTimeString(int d, int h, int m, int s)
        {
            TimeSpan ts = new TimeSpan(d, h, m, s);
            return HumanizedString(ts);
        }

        public static string HumanizedString(TimeSpan ts)
        {
            return ts.Humanize(4, maxUnit: TimeUnit.Day, minUnit: TimeUnit.Second);
        }
    }
}