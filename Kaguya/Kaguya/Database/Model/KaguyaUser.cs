using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Internal.PrimitiveExtensions;
using Kaguya.Internal.Services;
using OsuSharp;

namespace Kaguya.Database.Model
{
	public enum ExpNotificationPreference
	{
		Chat,
		Dm,
		Both,
		Disabled
	}
	public class KaguyaUser
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong UserId { get; set; }

		public int GlobalExp { get; private set; } = 0;

		public int FishExp { get; private set; } = 0;

		public int Coins { get; private set; } = 0;

		public long? OsuId { get; set; }
		public GameMode? OsuGameMode { get; set; }

		public int TotalCommandUses { get; set; } = 0;

		public int TotalDaysPremium { get; set; } = 0;
		public int TotalPremiumRedemptions { get; set; } = 0;

		/// <summary>
		/// Whenever a user uses a command, decrease this value by one.
		/// The ratelimit service will check for whether the user
		/// has too many commands allowed by the ratelimit (x cmds in y seconds).
		///
		/// This check is made if there are zero active ratelimits remaining and
		/// gets set back to 6 on a timer.
		/// </summary>
		public int ActiveRateLimit { get; set; } = 6;

		public int RateLimitWarnings { get; set; } = 0;

		public int TotalUpvotes { get; set; } = 0;
		public DateTimeOffset DateFirstTracked { get; set; }

		public DateTimeOffset? LastGivenExp { get; private set; }

		public DateTimeOffset? LastDailyBonus { get; set; }

		public DateTimeOffset? LastWeeklyBonus { get; set; }

		public DateTimeOffset? LastGivenRep { get; set; }

		public DateTimeOffset? LastRatelimited { get; set; }

		// blub blub >))'>
		public DateTimeOffset? LastFished { get; set; }

		public DateTimeOffset? LastBlacklisted { get; set; }
		public DateTimeOffset? LastUpvoted { get; set; }

		public DateTimeOffset? PremiumExpiration { get; set; }
		public DateTimeOffset? BlacklistExpiration { get; set; }
		
		/// <summary>
		/// If a user wants to receive level-up notifications, what type should it be?
		/// </summary>
		public ExpNotificationPreference ExpNotificationType { get; set; } = ExpNotificationPreference.Chat;

		// public FishHandler.FishLevelBonuses FishLevelBonuses => new FishHandler.FishLevelBonuses(FishExp);
		// public bool IsBotOwner => UserId == ConfigProperties.BotConfig.BotOwnerId;
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTimeOffset.Now;

		public bool CanGiveRep => !LastGivenRep.HasValue || LastGivenRep.Value < DateTimeOffset.Now.AddHours(-24);

		public bool CanGetDailyCoins => !LastDailyBonus.HasValue || LastDailyBonus.Value < DateTimeOffset.Now.AddHours(-24);

		public bool CanGetWeeklyCoins => !LastWeeklyBonus.HasValue || LastWeeklyBonus.Value < DateTimeOffset.Now.AddDays(-7);
		/// <summary>
		/// Whether or not the user can upvote on top.gg - checks for last 12 hours.
		/// </summary>
		public bool CanUpvote => !LastUpvoted.HasValue || LastUpvoted < DateTimeOffset.Now.AddHours(-12);

		public int GlobalExpLevel => ExactGlobalExpLevel.ToFloor();

		/// <summary>
		/// The user's level as returned by the experience formula.
		/// </summary>
		public double ExactGlobalExpLevel => ExperienceService.CalculateLevel(this.GlobalExp);

		public int FishLevel => ExactFishLevel.ToFloor();

		public double ExactFishLevel => ExperienceService.CalculateLevel(this.FishExp);

		public int ExpToNextGlobalLevel => ExperienceService.CalculateExpFromLevel(GlobalExpLevel + 1) - this.GlobalExp;
		public double PercentToNextLevel => ExperienceService.CalculatePercentToNextLevel(this.ExactGlobalExpLevel, this.GlobalExp);

		// public IEnumerable<Praise> Praise => DatabaseQueries.GetAllForUserAsync<Praise>(UserId).Result;

		/// <summary>
		/// Adjusts the user's coins by the <see cref="amount"/> given.
		/// </summary>
		/// <param name="amount"></param>
		public void AdjustCoins(int amount)
		{
			if (this.Coins + amount < 0)
			{
				this.Coins = 0;

				return;
			}
			
			this.Coins += amount;
		}

		/// <summary>
		/// Adjusts the user's global experience by the <see cref="amount"/> given.
		/// </summary>
		/// <param name="amount"></param>
		public void AdjustExperienceGlobal(int amount)
		{
			if (this.GlobalExp + amount < 0)
			{
				this.GlobalExp = 0;

				return;
			}

			this.LastGivenExp = DateTimeOffset.Now;
			this.GlobalExp += amount;
		}

		/// <summary>
		/// Adjusts the user's fish experience by the <see cref="amount"/> given.
		/// </summary>
		/// <param name="amount"></param>
		public void AdjustFishExperience(int amount)
		{
			if (this.FishExp + amount < 0)
			{
				this.FishExp = 0;

				return;
			}
			
			this.FishExp += amount;
		}

		public override string ToString() => UserId.ToString();
	}
}