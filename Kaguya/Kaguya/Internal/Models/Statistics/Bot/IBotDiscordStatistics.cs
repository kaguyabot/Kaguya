using Discord.Rest;
using System;

namespace Kaguya.Internal.Models.Statistics.Bot
{
	public interface IBotDiscordStatistics
	{
		/// <summary>
		///  The currently logged-in user (bot account)
		/// </summary>
		public RestSelfUser RestUser { get; }
	}
}