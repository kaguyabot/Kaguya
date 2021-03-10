namespace Kaguya.Web.Options
{
	public class TopGgConfigurations
	{
		public static string Position => "TopGgSettings";
		/// <summary>
		///  The Top.GG api key
		/// </summary>
		public string ApiKey { get; set; }
		/// <summary>
		///  The authorization string used on top.gg to ensure the webhooks are being sent from top.gg.
		/// </summary>
		public string AuthHeader { get; set; }
	}
}