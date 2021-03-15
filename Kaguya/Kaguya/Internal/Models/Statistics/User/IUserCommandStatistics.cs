namespace Kaguya.Internal.Models.Statistics.User
{
	public interface IUserCommandStatistics
	{
		/// <summary>
		///  The total amount of commands the user has executed successfully.
		/// </summary>
		public int CommandsExecuted { get; }
		/// <summary>
		///  The total amount of commands the user has executed successfully in the last 24 hours.
		/// </summary>
		public int CommandsExecutedLastTwentyFourHours { get; }
		public string MostUsedCommand { get; }
	}
}