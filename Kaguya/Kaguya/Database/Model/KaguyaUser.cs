using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Internal.PrimitiveExtensions;
using Kaguya.Internal.Services;
using OsuSharp;

namespace Kaguya.Database.Model
{
	public class KaguyaUser
	{
		/// <summary>
		/// The Discord ID for the user. Unique.
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong UserId { get; set; }
		/// <summary>
		/// The total amount of global experience the user has. Global experience is
		/// earned at a constant rate through typing in chat.
		/// </summary>
		public int GlobalExp { get; private set; }
		/// <summary>
		/// The total amount of fishing experience the user has. Fishing experience is
		/// earned through the fishing game. More experience is awarded for catching rarer fish.
		/// </summary>
		public int FishExp { get; private set; }
		/// <summary>
		/// The total amount of coins the user has. Coins are earned through upvoting on sites
		/// such as top.gg, using $daily, $weekly, and by playing games.
		/// </summary>
		public int Coins { get; private set; }
		/// <summary>
		/// An optional long representing the user's unique osu! (osu.ppy.sh) ID.
		/// </summary>
		public long? OsuId { get; set; }
		/// <summary>
		/// An optional <see cref="GameMode"/> representing the user's saved osu! <see cref="GameMode"/>.
		/// This is stored so that the user does not have to specify it each time they wish to use
		/// the osu! commands on themselves.
		/// </summary>
		public GameMode? OsuGameMode { get; set; }
		/// <summary>
		/// An all-time sum of how many commands the user has executed.
		/// </summary>
		public int TotalCommandUses { get; set; }
		/// <summary>
		/// An all-time sum of how long the user has been a premium subscriber for. Each
		/// publicly sold key (in exchange for real-life currency) is sold in an increment of
		/// 30 days. Each time a user redeems a key, this value is incremented by how many
		/// days the key is worth.
		/// </summary>
		public int TotalDaysPremium { get; set; }
		/// <summary>
		/// A count of how many times the user has successfully redeemed a premium key, not
		/// to be confused with the <see cref="TotalDaysPremium"/> property.
		/// </summary>
		public int TotalPremiumRedemptions { get; set; }

		/// <summary>
		/// Whenever a user uses a command, decrease this value by one.
		/// The ratelimit service will check for whether the user
		/// has too many commands allowed by the ratelimit (x cmds in y seconds).
		///
		/// This check is made if there are zero active ratelimits remaining and
		/// gets set back to 6 on a timer.
		/// </summary>
		public int ActiveRateLimit { get; set; } = 6;
		/// <summary>
		/// How many ratelimit warnings the user has accumulated.
		/// </summary>
		public int RateLimitWarnings { get; set; }
		/// <summary>
		/// The all-time sum of unique upvotes the user has performed on top.gg. A user
		/// may vote every 12 hours on top.gg.
		/// </summary>
		public int TotalUpvotesTopGg { get; set; }
		/// <summary>
		/// The all-time sum of unique upvotes on discord.boats.
		/// todo: Set this value and enable rewards for voting on discord.boats. NOT IMPLEMENTED.
		/// </summary>
		public int TotalUpvotesDiscordBoats { get; set; }
		/// <summary>
		/// The total amount of gambles the user has performed. Fishing is not a gamble.
		/// </summary>
		public int TotalGambles { get; set; }
		/// <summary>
		/// The total amount of times the user has won while playing a gambling game.
		/// </summary>
		public int TotalGambleWins { get; set; }
		/// <summary>
		/// The total amount of times the user has lost while playing a gambling game.
		/// </summary>
		public int TotalGambleLosses { get; set; }
		/// <summary>
		/// The total amount of points the user has offered up for gamble before any wins or losses.
		/// </summary>
		public int TotalCoinsGambled { get; set; }
		/// <summary>
		/// The gross sum of coins the user has won from gambling games.
		/// </summary>
		public int GrossGambleCoinWinnings { get; set; }
		/// <summary>
		/// The gross sum of coins the user has lost from gambling games.
		/// </summary>
		public int GrossGambleCoinLosses { get; set; }
		/// <summary>
		/// The <see cref="DateTimeOffset"/> at which the user was first created
		/// by the bot. This value is equivalent to zero if the user was created
		/// before the v4 release.
		/// </summary>
		public DateTimeOffset DateFirstTracked { get; set; }
		/// <summary>
		/// When the user was last given experience points.
		/// </summary>
		public DateTimeOffset? LastGivenExp { get; private set; }
		/// <summary>
		/// When the user last received a daily bonus via $daily.
		/// </summary>
		public DateTimeOffset? LastDailyBonus { get; set; }
		/// <summary>
		/// When the user last received a weekly bonus via $weekly.
		/// </summary>
		public DateTimeOffset? LastWeeklyBonus { get; set; }
		/// <summary>
		/// When the user last gave rep to another user via $rep.
		/// </summary>
		public DateTimeOffset? LastGivenRep { get; set; }
		/// <summary>
		/// When the user last received a <see cref="RateLimitWarnings"/> from the bot
		/// due to spam.
		/// </summary>
		public DateTimeOffset? LastRatelimited { get; set; }
		// blub blub >))'>
		/// <summary>
		/// When the user last fished via $fish
		/// </summary>
		public DateTimeOffset? LastFished { get; set; }
		/// <summary>
		/// When the user last upvoted on top.gg
		/// </summary>
		public DateTimeOffset? LastUpvotedTopGg { get; set; }
		/// <summary>
		/// When the user last upvoted on discord.boats
		/// todo: NOT IMPLEMENTED.
		/// </summary>
		public DateTimeOffset? LastUpvotedDiscordBoats { get; set; }
		/// <summary>
		/// When this user's premium benefits will expire, if applicable.
		/// </summary>
		public DateTimeOffset? PremiumExpiration { get; set; }
		/// <summary>
		/// When the user was last blacklisted by the bot or Kaguya Administration.
		/// </summary>
		public DateTimeOffset? LastBlacklisted { get; set; }
		/// <summary>
		/// When this user's blacklist will expire. Null if the user has never been blacklisted.
		/// </summary>
		public DateTimeOffset? BlacklistExpiration { get; set; }
		/// <summary>
		/// An all-time value representing the "net income" the user has earned
		/// or lost from gambling. This is equivalent to the <see cref="GrossGambleCoinWinnings"/>
		/// minus the <see cref="GrossGambleCoinLosses"/>.
		/// </summary>
		public int NetGambleCoingEarnings => GrossGambleCoinWinnings - GrossGambleCoinLosses;
		/// <summary>
		/// Whether or not this user is a premium user.
		/// </summary>
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTimeOffset.Now;
		/// <summary>
		/// Whether or not this user is eligible to give rep to another user.
		/// </summary>
		public bool CanGiveRep => !LastGivenRep.HasValue || LastGivenRep.Value < DateTimeOffset.Now.AddHours(-24);
		/// <summary>
		/// Whether or not this user is eligible to receive daily coins via $daily.
		/// </summary>
		public bool CanGetDailyCoins => !LastDailyBonus.HasValue || LastDailyBonus.Value < DateTimeOffset.Now.AddHours(-24);
		/// <summary>
		/// Whether or not this user is eligible to receive weekly coins via $weekly.
		/// </summary>
		public bool CanGetWeeklyCoins => !LastWeeklyBonus.HasValue || LastWeeklyBonus.Value < DateTimeOffset.Now.AddDays(-7);
		/// <summary>
		/// Whether or not the user can upvote on top.gg - checks for last 12 hours.
		/// </summary>
		public bool CanUpvote => !LastUpvotedTopGg.HasValue || LastUpvotedTopGg < DateTimeOffset.Now.AddHours(-12);
		/// <summary>
		/// A user-facing global experience level. This property should be used to display (to a user) what
		/// their global experience level is.
		/// </summary>
		public int GlobalExpLevel => ExactGlobalExpLevel.ToFloor();
		/// <summary>
		/// The user's level as returned by the experience formula. This is used for exact,
		/// internal calculations, such as progress through the level.
		/// </summary>
		public decimal ExactGlobalExpLevel => ExperienceService.CalculateLevel(this.GlobalExp);
		/// <summary>
		/// A user-facing fishing experience level.
		/// </summary>
		public int FishLevel => ExactFishLevel.ToFloor();
		/// <summary>
		/// The user's fishing level as returned by the experience formula. This is used for exact,
		/// internal calculations, such as progress through the level.
		/// </summary>
		public decimal ExactFishLevel => ExperienceService.CalculateLevel(this.FishExp);
		/// <summary>
		/// A positive value representing how many experience points the user needs in order to
		/// reach the next global experience level.
		/// </summary>
		public int ExpToNextGlobalLevel => ExperienceService.CalculateExpFromLevel(GlobalExpLevel + 1) - this.GlobalExp;
		/// <summary>
		/// A decimal value (range 0.00M - 99.99...M) that displays the user's % to the next level.
		/// If the user is 30% towards achieving the next level, this value is 30.00M.
		/// </summary>
		public decimal PercentToNextLevel => ExperienceService.CalculatePercentToNextLevel(this.ExactGlobalExpLevel);
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
		/// <summary>
		/// Displays the user's ID.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => UserId.ToString();
	}
}