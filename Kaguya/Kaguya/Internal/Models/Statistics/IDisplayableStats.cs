namespace Kaguya.Internal.Models.Statistics
{
	public interface IDisplayableStats
	{
		public string GetDiscordStatsString();
		public string GetFishingStatsString();
		public string GetKaguyaStatsString();
		public string GetCommandStatsString();
		public string GetGamblingStatsString();
	}
}