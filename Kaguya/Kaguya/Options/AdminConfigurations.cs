namespace Kaguya.Options
{
	public class AdminConfigurations
	{
		public static string Position => "AdminSettings";
		public ulong OwnerId { get; set; }
		public string OsuApiKey { get; set; }
	}
}