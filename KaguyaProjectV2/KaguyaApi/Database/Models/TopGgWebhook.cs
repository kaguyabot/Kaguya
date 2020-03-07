using System.Text.Json.Serialization;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaApi.Database.Models
{
    public class TopGgWebhook
    {
        [JsonPropertyName("bot")]
        public string BotId { get; set; }
        [JsonPropertyName("user")]
        public string UserId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("isWeekend")]
        public bool IsWeekend { get; set; }
        [JsonPropertyName("query")]
        public string Query { get; set; }
    }

    /// <summary>
    /// The object we insert into the database to represent a standard Top.GG webhook.
    /// </summary>
    [Table(Name = "upvotes")]
    public class DatabaseUpvoteWebhook : 
        IKaguyaQueryable<DatabaseUpvoteWebhook>, 
        IKaguyaUnique<DatabaseUpvoteWebhook>,
        IUserSearchable<DatabaseUpvoteWebhook>
    {
        [Column("VoteId"), NotNull]
        public string VoteId { get; set; }
        /// <summary>
        /// ID of the bot that received a vote
        /// </summary>
        [Column(Name = "BotId"), NotNull]
        public ulong BotId { get; set; }
        /// <summary>
        /// ID of the user who voted
        /// </summary>
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        /// <summary>
        /// The time the user upvoted, in OADate form.
        /// </summary>
        [Column(Name = "Time"), NotNull]
        public double TimeVoted { get; set; }
        /// <summary>
        /// The type of the vote (should always be "upvote" except when using the test button it's "test")
        /// </summary>
        [Column(Name = "VoteType"), NotNull]
        public string UpvoteType { get; set; }
        /// <summary>
        /// Whether the weekend multiplier is in effect, meaning users' votes count as two
        /// </summary>
        [Column(Name = "IsWeekend"), NotNull]
        public bool IsWeekend { get; set; }
        /// <summary>
        /// Query string params found on the /bot/:ID/vote page. Example: ?a=1
        /// </summary>
        [Column(Name = "QueryParams"), Nullable]
        public string QueryParams { get; set; }
    }
}
