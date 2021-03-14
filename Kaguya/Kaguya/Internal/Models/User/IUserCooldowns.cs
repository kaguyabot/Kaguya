using System.Collections.Generic;

namespace Kaguya.Internal.Models.User
{
	/// <summary>
	///  A collection of TimeSpans representing an individual user's cooldowns.
	/// </summary>
	public interface IUserCooldowns
	{
		public IInternalCooldown TopGgVoteCooldown { get; }
		public IInternalCooldown DailyCooldown { get; }
		public IInternalCooldown WeeklyCooldown { get; }

		/// <summary>
		///  Returns a list of all of the user's cooldowns.
		/// </summary>
		/// <returns></returns>
		public List<IInternalCooldown> ToList();
	}
}