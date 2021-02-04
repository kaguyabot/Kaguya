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

		public int Points { get; private set; } = 0;

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
		public DateTime DateFirstTracked { get; set; }

		public DateTime? LastGivenExp { get; private set; }

		public DateTime? LastDailyBonus { get; set; }

		public DateTime? LastWeeklyBonus { get; set; }

		public DateTime? LastGivenRep { get; set; }

		public DateTime? LastRatelimited { get; set; }

		// blub blub >))'>
		public DateTime? LastFished { get; set; }

		public DateTime? LastBlacklisted { get; set; }

		public DateTime? PremiumExpiration { get; set; }
		public DateTime? BlacklistExpiration { get; set; }
		
		/// <summary>
		/// If a user wants to receive level-up notifications, what type should it be?
		/// </summary>
		public ExpNotificationPreference ExpNotificationType { get; set; } = ExpNotificationPreference.Chat;

		// public FishHandler.FishLevelBonuses FishLevelBonuses => new FishHandler.FishLevelBonuses(FishExp);
		// public bool IsBotOwner => UserId == ConfigProperties.BotConfig.BotOwnerId;
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTime.Now;

		public bool CanGiveRep => !LastGivenRep.HasValue || LastGivenRep.Value < DateTime.Now.AddHours(-24);

		public bool CanGetDailyPoints => !LastDailyBonus.HasValue || LastDailyBonus.Value < DateTime.Now.AddHours(-24);

		public bool CanGetWeeklyPoints => !LastWeeklyBonus.HasValue || LastWeeklyBonus.Value < DateTime.Now.AddDays(-7);

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
		/// Adjusts the user's points by the <see cref="amount"/> given.
		/// </summary>
		/// <param name="amount"></param>
		public void AdjustPoints(int amount)
		{
			if (this.Points + amount < 0)
			{
				this.Points = 0;

				return;
			}
			
			this.Points += amount;
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

			this.LastGivenExp = DateTime.Now;
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