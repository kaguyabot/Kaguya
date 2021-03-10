using System;

namespace Kaguya.Internal.Models.User
{
	/// <summary>
	///  A collection of TimeSpans representing an individual user's cooldowns.
	/// </summary>
	public interface IUserCooldowns
	{
		public TimeSpan? TopGgVoteCooldown { get; }
		public TimeSpan? DailyCooldown { get; }
		public TimeSpan? WeeklyCooldown { get; }
	}
}