using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaApi.Database.Models
{
    [Table(Name = "upvotes")]
    public class TopGgWebhook
    {
        [Column(Name = "VoteId"), NotNull]
        public string VoteId { get; set; }
        /// <summary>
        /// ID of the bot that received a vote
        /// </summary>
        [JsonPropertyName("bot")]
        [Column(Name = "BotId"), NotNull]
        public ulong BotId { get; set; }
        /// <summary>
        /// ID of the user who voted
        /// </summary>
        [JsonPropertyName("user")]
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
        [JsonPropertyName("type")]
        [Column(Name = "VoteType"), NotNull]
        public string UpvoteType { get; set; }
        /// <summary>
        /// Whether the weekend multiplier is in effect, meaning users' votes count as two
        /// </summary>
        [JsonPropertyName("isWeekend")]
        [Column(Name = "IsWeekend"), NotNull]
        public bool IsWeekend { get; set; }
        /// <summary>
        /// Query string params found on the /bot/:ID/vote page. Example: ?a=1&b=2
        /// </summary>
        [JsonPropertyName("query")]
        [Column(Name = "QueryParams"), NotNull]
        public string? QueryParams { get; set; }
    }
}
