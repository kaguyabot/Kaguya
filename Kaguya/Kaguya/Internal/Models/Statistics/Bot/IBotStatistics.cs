using Kaguya.Database.Model;
using System;

namespace Kaguya.Internal.Models.Statistics.Bot
{
	public interface IBotStatistics : IBotCommandStatistics, IBotDiscordStatistics, IBotFishingStatistics,
		IBotGamblingStatistics
	{
		/// <summary>
		///  The most recent entry of statistics from the database.
		/// </summary>
		public KaguyaStatistics DatabaseStatistics { get; }
		/// <summary>
		///  Application lifetime duration.
		/// </summary>
		public TimeSpan Uptime { get; }
	}
}