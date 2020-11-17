using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "upvotes")]
    public class TopGgWebhook : IKaguyaQueryable<TopGgWebhook>, IKaguyaUnique<TopGgWebhook>, IUserSearchable<TopGgWebhook>
    {
        // todo: Duplicate of KaguyaProjectV2.KaguyaApi.Database.Models.TopGgWebhook.cs ??
        [Column(Name = "vote_id")]
        public string VoteId { get; set; }

        /// <summary>
        /// ID of the bot that received a vote
        /// </summary>
        [Column(Name = "bot_id")]
        [NotNull]
        public ulong BotId { get; set; }

        /// <summary>
        /// ID of the user who voted
        /// </summary>
        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        /// <summary>
        /// The time the user upvoted, in OADate form.
        /// </summary>
        [Column(Name = "time_voted")]
        [NotNull]
        public double TimeVoted { get; set; }

        /// <summary>
        /// The type of the vote (should always be "upvote" except when using the test button it's "test")
        /// </summary>
        [Column(Name = "vote_type")]
        [NotNull]
        public string UpvoteType { get; set; }

        /// <summary>
        /// Whether the weekend multiplier is in effect, meaning users' votes count as two
        /// </summary>
        [Column(Name = "is_weekend")]
        [NotNull]
        public bool IsWeekend { get; set; }

        /// <summary>
        /// Query string params found on the /bot/:ID/vote page. Example: ?a=1&b=2
        /// </summary>
        [Column(Name = "query_params")]
        [NotNull]
        public string QueryParams { get; set; }

        /// <summary>
        /// Whether or not a reminder has been sent out for this upvote object.
        /// </summary>
        [Column(Name = "reminder_sent")]
        [NotNull]
        public bool ReminderSent { get; set; }
    }
}