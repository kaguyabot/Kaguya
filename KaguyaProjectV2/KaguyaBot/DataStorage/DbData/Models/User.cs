using KaguyaProjectV2.KaguyaBot.Core.Commands.EXP;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    //todo: Add properties for collections such as IEnumerable<FavoriteTrack>
    [Table(Name = "kaguya_user")]
    public class User : IKaguyaQueryable<User>, IKaguyaUnique<User>, IUserSearchable<User>
    {
        [PrimaryKey]
        [Column(Name = "user_id")]
        public ulong UserId { get; set; }

        [Column(Name = "experience")]
        [NotNull]
        public int Experience { get; set; }

        [Column(Name = "fish_exp")]
        [NotNull]
        public int FishExp { get; set; }

        [Column(Name = "points")]
        [NotNull]
        public int Points { get; set; }

        [Column(Name = "osu_id")]
        [NotNull]
        public int OsuId { get; set; }

        [Column(Name = "osu_beatmaps_linked")]
        [NotNull]
        public int OsuBeatmapsLinked { get; set; }

        [Column(Name = "commands_used")]
        [NotNull]
        public int TotalCommandUses { get; set; }

        [Column(Name = "total_days_premium")]
        [NotNull]
        public int TotalDaysPremium { get; set; }

        /// <summary>
        /// Whenever a user uses a command, increase this by one.
        /// The ratelimit service will check for whether the user
        /// has too many commands allowed by the ratelimit (x cmds in y seconds).
        /// </summary>
        [Column(Name = "active_ratelimit")]
        [NotNull]
        public int ActiveRateLimit { get; set; }

        [Column(Name = "ratelimit_warnings")]
        [NotNull]
        public int RateLimitWarnings { get; set; }

        [Column(Name = "total_upvotes")]
        [NotNull]
        public int TotalUpvotes { get; set; }

        [Column(Name = "last_given_exp")]
        [NotNull]
        public double LastGivenExp { get; set; }

        [Column(Name = "last_daily_bonus")]
        [NotNull]
        public double LastDailyBonus { get; set; }

        [Column(Name = "last_weekly_bonus")]
        [NotNull]
        public double LastWeeklyBonus { get; set; }

        [Column(Name = "last_given_rep")]
        [NotNull]
        public double LastGivenRep { get; set; }

        [Column(Name = "last_ratelimited")]
        [NotNull]
        public double LastRatelimited { get; set; }

        [Column(Name = "last_fished")]
        [NotNull]
        public double LastFished { get; set; }

        [Column(Name = "premium_expiration")]
        public double PremiumExpiration { get; set; }

        /// <summary>
        /// If a user wants to receive level-up notifications in chat, what type should it be?
        /// </summary>
        [Column(Name = "exp_chatnotification_typenum")]
        [NotNull]
        public int ExpChatNotificationTypeNum { private get; set; } = 2;

        /// <summary>
        /// If a user wants to receive level-up notifications in their DMs, what type should it be?
        /// </summary>
        [Column(Name = "exp_dmnotification_typenum")]
        [NotNull]
        public int ExpDmNotificationTypeNum { private get; set; } = 3;

        public bool IsBlacklisted => Blacklist != null && (Blacklist.Expiration - DateTime.Now.ToOADate()) > 0;
        public ExpType ExpChatNotificationType => (ExpType) ExpChatNotificationTypeNum;
        public ExpType ExpDmNotificationType => (ExpType) ExpDmNotificationTypeNum;
        public FishHandler.FishLevelBonuses FishLevelBonuses => new FishHandler.FishLevelBonuses(FishExp);
        public bool IsBotOwner => UserId == ConfigProperties.BotConfig.BotOwnerId;
        public bool IsPremium => PremiumExpiration > DateTime.Now.ToOADate();
        public bool CanGiveRep => LastGivenRep < DateTime.Now.AddHours(-24).ToOADate();
        public bool CanGetDailyPoints => LastDailyBonus < DateTime.Now.AddHours(-24).ToOADate();
        public bool CanGetWeeklyPoints => LastWeeklyBonus < DateTime.Now.AddDays(-7).ToOADate();
        public IEnumerable<Praise> Praise => DatabaseQueries.GetAllForUserAsync<Praise>(UserId).Result;

        /// <summary>
        /// Adds the specified number of points to the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public User AddPoints(uint points)
        {
            Points += (int) points;

            return this;
        }

        /// <summary>
        /// Adds the specified number of points to the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public User AddPoints(int points)
        {
            Points += points;

            return this;
        }

        /// <summary>
        /// Removes the specified number of points from the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public User RemovePoints(uint points)
        {
            Points -= (int) points;

            return this;
        }

        public User AddGlobalExp(uint exp)
        {
            Experience += (int) exp;

            return this;
        }

        public User RemoveGlobalExp(uint exp)
        {
            Experience -= (int) exp;

            return this;
        }

        public async Task<ServerExp> GetServerExp(ulong serverId)
        {
            List<ServerExp> expCol = await DatabaseQueries.GetAllForServerAndUserAsync<ServerExp>(UserId, serverId);
            ServerExp curExp = expCol[0];

            return curExp;
        }

        /// <summary>
        /// FK_UserBlacklists_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public UserBlacklist Blacklist { get; set; }

        /// <summary>
        /// FK_CommandHistory_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<CommandHistory> CommandHistory { get; set; }

        /// <summary>
        /// FK_Fish_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Fish> Fish { get; set; }

        /// <summary>
        /// FK_GambleHistory_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<GambleHistory> GambleHistory { get; set; }

        /// <summary>
        /// FK_KaguyaUser_Reminder
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Reminder> Reminders { get; set; }

        /// <summary>
        /// FK_Rep_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Rep> Rep { get; set; }

        /// <summary>
        /// FK_ServerExp_KaguyaUser_BackReference
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<ServerExp> ServerExp { get; set; }

        /// <summary>
        /// FK_Quotes_KaguyaUser_BackReference
        /// </summary>
        /// <value></value>
        [Association(ThisKey = "UserId", OtherKey = "UserId")]
        public IEnumerable<Quote> Quotes { get; set; }
    }
}