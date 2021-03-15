using Kaguya.Database.Model;
using System;
using System.Collections.Generic;

namespace Kaguya.Internal.Models.User
{
	public class UserCooldowns : IUserCooldowns
	{
		private const int TOP_GG_VOTE_COOLDOWN_HOURS = 12;
		private const int DAILY_COOLDOWN_HOURS = 24;
		private const int WEEKLY_COOLDOWN_HOURS = DAILY_COOLDOWN_HOURS * 7;
		private readonly KaguyaUser _user;
		public UserCooldowns(KaguyaUser user) { _user = user; }
		public IInternalCooldown TopGgVoteCooldown =>
			new InternalCooldown(TOP_GG_VOTE_COOLDOWN_HOURS, _user.LastUpvotedTopGg, "Top GG Voting");
		public IInternalCooldown DailyCooldown => new InternalCooldown(DAILY_COOLDOWN_HOURS, _user.LastDailyBonus, "Daily Bonus");
		public IInternalCooldown WeeklyCooldown => new InternalCooldown(WEEKLY_COOLDOWN_HOURS, _user.LastWeeklyBonus, "Weekly Bonus");

		public List<IInternalCooldown> ToList()
		{
			return new()
			{
				this.TopGgVoteCooldown,
				this.DailyCooldown,
				this.WeeklyCooldown
			};
		}
	}

	public interface IInternalCooldown
	{
		/// <summary>
		///  How much time is left on the cooldown. Null if the user has never been placed on cooldown.
		/// </summary>
		/// <returns></returns>
		public TimeSpan? CooldownRemaining();

		/// <summary>
		///  Whether the user's cooldown has expired or not.
		/// </summary>
		/// <returns></returns>
		public bool HasExpired();

		/// <summary>
		///  A printable version of this cooldown.
		/// </summary>
		/// <returns></returns>
		public string ToString();
	}

	public class InternalCooldown : IInternalCooldown
	{
		private readonly string _formattedText;
		private readonly int _initialCooldownHours;
		private readonly DateTimeOffset? _lastUse;

		public InternalCooldown(int initialCooldownHours, DateTimeOffset? lastUse, string formattedText)
		{
			_initialCooldownHours = initialCooldownHours;
			_lastUse = lastUse;
			_formattedText = formattedText;
		}

		public TimeSpan? CooldownRemaining()
		{
			if (!_lastUse.HasValue)
			{
				return null;
			}

			return CalculateDifference(_initialCooldownHours, _lastUse!.Value);
		}

		public bool HasExpired() { return _lastUse == null || CooldownRemaining() <= TimeSpan.Zero; }

		/// <summary>
		///  Formats the InternalCooldown as a readable name of the property.
		///  E.g. TopGgVoteCooldown = "Top GG Voting"
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return _formattedText; }

		/// <summary>
		///  Returns the difference between now and when the user most recently used a cooldown-invoking feature.
		/// </summary>
		/// <param name="baseCd">The amount of hours this cooldown lasts</param>
		/// <param name="lastUsage">When the user last used the feature</param>
		/// <returns></returns>
		private static TimeSpan CalculateDifference(int baseCd, DateTimeOffset lastUsage)
		{
			return TimeSpan.FromHours(baseCd) - DateTimeOffset.Now.Subtract(lastUsage);
		}
	}
}