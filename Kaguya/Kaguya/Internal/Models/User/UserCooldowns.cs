using Kaguya.Database.Model;
using System;

namespace Kaguya.Internal.Models.User
{
	public class UserCooldowns : IUserCooldowns
	{
		private const int VOTE_COOLDOWN_HOURS = 12;
		private const int DAILY_COOLDOWN_HOURS = 24;
		private const int WEEKLY_COOLDOWN_HOURS = DAILY_COOLDOWN_HOURS * 7;
		private readonly KaguyaUser _user;
		public UserCooldowns(KaguyaUser user) { _user = user; }
		public TimeSpan? TopGgVoteCooldown
		{
			get
			{
				if (!_user.LastUpvotedTopGg.HasValue)
				{
					return null;
				}

				return CalculateDifference(VOTE_COOLDOWN_HOURS, _user.LastUpvotedTopGg!.Value);
			}
		}
		public TimeSpan? DailyCooldown
		{
			get
			{
				if (!_user.LastDailyBonus.HasValue)
				{
					return null;
				}

				return CalculateDifference(DAILY_COOLDOWN_HOURS, _user.LastDailyBonus!.Value);
			}
		}
		public TimeSpan? WeeklyCooldown
		{
			get
			{
				if (!_user.LastWeeklyBonus.HasValue)
				{
					return null;
				}

				return CalculateDifference(WEEKLY_COOLDOWN_HOURS, _user.LastWeeklyBonus!.Value);
			}
		}

		private static TimeSpan CalculateDifference(int baseCd, DateTimeOffset userOffset)
		{
			return TimeSpan.FromHours(baseCd) - DateTimeOffset.Now.Subtract(userOffset);
		}
	}
}