using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Kaguya.Database.Model
{
	public class KaguyaServer
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong ServerId { get; set; }

		[NotNull]
		public string CommandPrefix { get; set; } = "$";

		public int TotalCommandCount { get; set; }

		public int TotalAdminActions { get; set; }

		public int PraiseCooldown { get; set; } = 24;

		public int NextQuoteId { get; set; } = 1;
		public ulong MuteRoleId { get; set; }
		public DateTime DateFirstTracked { get; set; }

		public DateTime? PremiumExpiration { get; set; }

		/// <summary>
		/// A boolean that determines whether the server is currently purging messages.
		/// We log this so that we don't bombard log channels with messages whenever they are bulk
		/// cleared. Instead, we use this boolean to determine whether to skip the 'Deleted Message'
		/// log event. We log bulk-deletion of messages by checking the audit log instead. This
		/// value is not in the database.
		/// </summary>
		[NotNull]
		public bool IsCurrentlyPurgingMessages { get; set; }

		public string CustomGreeting { get; set; }

		public bool CustomGreetingIsEnabled { get; set; } = false;

		public bool LevelAnnouncementsEnabled { get; set; } = true;

		public bool OsuLinkParsingEnabled { get; set; } = true;

		/// <summary>
		/// Whether or not the server currently has an active premium subscription.
		/// </summary>
		[NotMapped]
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTime.Now;

		public AntiRaidConfig AntiRaid { get; set; }
	}
}