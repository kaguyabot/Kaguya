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
		public ulong UserId { get; set; }

		public int Experience { get; set; } = 0;

		public int FishExp { get; set; } = 0;

		public int Points { get; set; } = 0;

		public int? OsuId { get; set; }

		public int TotalCommandUses { get; set; } = 0;

		public int TotalDaysPremium { get; set; } = 0;

		/// <summary>
		/// Whenever a user uses a command, increase this by one.
		/// The ratelimit service will check for whether the user
		/// has too many commands allowed by the ratelimit (x cmds in y seconds).
		/// </summary>
		// TODO: revisit this
		public int ActiveRateLimit { get; set; } = 0;

		public int RateLimitWarnings { get; set; } = 0;

		public int TotalUpvotes { get; set; } = 0;

		public DateTime? LastGivenExp { get; set; }

		public DateTime? LastDailyBonus { get; set; }

		public DateTime? LastWeeklyBonus { get; set; }

		public DateTime? LastGivenRep { get; set; }

		public DateTime? LastRatelimited { get; set; }

		// blub blub >))'>
		public DateTime? LastFished { get; set; }

		/// <summary>
		/// If a user wants to receive level-up notifications, what type should it be?
		/// </summary>
		public ExpChannel ExpNotificationType { get; set; } = ExpChannel.Chat;

		public DateTime? PremiumExpiration { get; set; }

		// public FishHandler.FishLevelBonuses FishLevelBonuses => new FishHandler.FishLevelBonuses(FishExp);
		// public bool IsBotOwner => UserId == ConfigProperties.BotConfig.BotOwnerId;
		[NotMapped]
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTime.Now;

		[NotMapped]
		public bool CanGiveRep => !LastGivenRep.HasValue || LastGivenRep.Value < DateTime.Now.AddHours(-24);

		[NotMapped]
		public bool CanGetDailyPoints => !LastDailyBonus.HasValue || LastDailyBonus.Value < DateTime.Now.AddHours(-24);

		[NotMapped]
		public bool CanGetWeeklyPoints => !LastWeeklyBonus.HasValue || LastWeeklyBonus.Value < DateTime.Now.AddDays(-7);

		// public IEnumerable<Praise> Praise => DatabaseQueries.GetAllForUserAsync<Praise>(UserId).Result;

	}
}