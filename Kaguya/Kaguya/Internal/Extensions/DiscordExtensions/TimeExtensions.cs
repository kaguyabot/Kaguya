using Humanizer;
using Humanizer.Localisation;
using System;

namespace Kaguya.Internal.Extensions.DiscordExtensions
{
	public static class TimeExtensions
	{
		// TODO: Replace all implementations of the below humanization with this extension method.
		public static string HumanizeTraditionalReadable(this TimeSpan t)
		{
			return t.Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
		}
	}
}