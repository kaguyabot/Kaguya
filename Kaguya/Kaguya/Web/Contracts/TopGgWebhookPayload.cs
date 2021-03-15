using System.Text.Json.Serialization;

namespace Kaguya.Web.Contracts
{
	public class TopGgWebhookPayload
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
}