using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public enum ExpChannel
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

		public int FishExp { get; set; } = 0;

		public int Points { get; private set; } = 0;

		public int? OsuId { get; set; }

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

		public DateTime? LastGivenExp { get; set; }

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
		public ExpChannel ExpNotificationType { get; set; } = ExpChannel.Chat;

		// public FishHandler.FishLevelBonuses FishLevelBonuses => new FishHandler.FishLevelBonuses(FishExp);
		// public bool IsBotOwner => UserId == ConfigProperties.BotConfig.BotOwnerId;
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTime.Now;

		public bool CanGiveRep => !LastGivenRep.HasValue || LastGivenRep.Value < DateTime.Now.AddHours(-24);

		public bool CanGetDailyPoints => !LastDailyBonus.HasValue || LastDailyBonus.Value < DateTime.Now.AddHours(-24);

		public bool CanGetWeeklyPoints => !LastWeeklyBonus.HasValue || LastWeeklyBonus.Value < DateTime.Now.AddDays(-7);

		public int GlobalExpLevel => ToFloor(ExactGlobalExpLevel);

		/// <summary>
		/// The user's level as returned by the experience formula.
		/// </summary>
		public double ExactGlobalExpLevel => CalculateLevel(this.GlobalExp);

		public int FishLevel => ToFloor(ExactFishLevel);

		public double ExactFishLevel => CalculateLevel(this.FishExp);

		public int ExpToNextGlobalLevel => CalculateExpFromLevel(GlobalExpLevel + 1) - this.GlobalExp;
		public double PercentToNextLevel => CalculatePercentToNextLevel();

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
		
		private static int ToFloor(double d) => (int)Math.Floor(d);
		
		private static double CalculateLevel(int exp)
		{
			if (exp < 64)
				return 0;
	        
			return Math.Sqrt((exp / 8) - 8);
		}

		private static int CalculateExpFromLevel(double level)
		{
			return (int) (8 * Math.Pow(level, 2));
		}

		private double CalculatePercentToNextLevel()
		{
			int baseExp = CalculateExpFromLevel(GlobalExpLevel);
			int nextExp = CalculateExpFromLevel(GlobalExpLevel + 1);
			int difference = nextExp - baseExp;
			int remaining = nextExp - this.GlobalExp;

			return (difference - remaining) / (double)difference;
		}
	}
}