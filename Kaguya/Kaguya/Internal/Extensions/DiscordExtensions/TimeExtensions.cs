using Humanizer;
using Humanizer.Localisation;
using System;

namespace Kaguya.Internal.Extensions.DiscordExtensions
{
	public static class TimeExtensions
	{
		public static string HumanizeTraditionalReadable(this TimeSpan t)
		{
			return t.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
		}
	}
}