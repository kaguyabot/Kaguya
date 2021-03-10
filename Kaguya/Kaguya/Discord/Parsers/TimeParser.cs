using Humanizer;
using Humanizer.Localisation;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kaguya.Discord.Parsers
{
	public class TimeParser
	{
		private readonly string _input;
		private readonly TimeSpan _time;

		public TimeParser(string input)
		{
			_input = input;
			_time = ParseTime();
		}

		/// <summary>
		///  Parses a string, formatted as 'XdXmXhXs' into a <see cref="System.TimeSpan" />.
		///  Returns TimeSpan.Zero if the string could not be parsed.
		/// </summary>
		/// <returns></returns>
		public TimeSpan ParseTime()
		{
			Regex[] regexs =
			{
				new("(([0-9])*s)"),
				new("(([0-9])*m)"),
				new("(([0-9])*h)"),
				new("(([0-9])*d)")
			};

			string s = regexs[0].Match(_input).Value;
			string m = regexs[1].Match(_input).Value;
			string h = regexs[2].Match(_input).Value;
			string d = regexs[3].Match(_input).Value;

			string seconds = s.Split('s').FirstOrDefault();
			string minutes = m.Split('m').FirstOrDefault();
			string hours = h.Split('h').FirstOrDefault();
			string days = d.Split('d').FirstOrDefault();

			if (string.IsNullOrWhiteSpace(seconds) &&
			    string.IsNullOrWhiteSpace(minutes) &&
			    string.IsNullOrWhiteSpace(hours) &&
			    string.IsNullOrWhiteSpace(days))
			{
				return TimeSpan.Zero;
			}

			int sec = default, min = default, hour = default, day = default;
			if (!string.IsNullOrWhiteSpace(seconds))
			{
				int.TryParse(seconds, out sec);
			}

			if (!string.IsNullOrWhiteSpace(minutes))
			{
				int.TryParse(minutes, out min);
			}

			if (!string.IsNullOrWhiteSpace(hours))
			{
				int.TryParse(hours, out hour);
			}

			if (!string.IsNullOrWhiteSpace(days))
			{
				int.TryParse(days, out day);
			}

			return new TimeSpan(day, hour, min, sec);
		}

		public string FormattedTimestring()
		{
			return _time.Humanize(3, maxUnit: TimeUnit.Day, minUnit: TimeUnit.Second);
		}
	}
}