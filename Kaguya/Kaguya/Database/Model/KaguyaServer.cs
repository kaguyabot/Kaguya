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

		public ulong? MuteRoleId { get; set; }
		
		public ulong? ShadowbanRoleId { get; set; }
		public DateTimeOffset DateFirstTracked { get; set; }

		public DateTimeOffset? PremiumExpiration { get; set; }
		public DateTimeOffset? NsfwAllowanceTime { get; set; }
		public bool IsNsfwAllowed { get; set; } = false;
		public ulong? NsfwAllowedId { get; set; }

		/// <summary>
		/// A boolean that determines whether the server is currently purging messages.
		/// We log this so that we don't bombard log channels with messages whenever they are bulk
		/// cleared. Instead, we use this boolean to determine whether to skip the 'Deleted Message'
		/// log event. We log bulk-deletion of messages by checking the audit log instead. This
		/// value is not in the database.
		/// todo: come up with a better solution - perhaps a concurrent dictionary somewhere else.
		/// </summary>
		[NotNull]
		public bool IsCurrentlyPurgingMessages { get; set; }

		public string CustomGreeting { get; set; }

		public bool CustomGreetingIsEnabled { get; set; } = false;
		public ulong? CustomGreetingTextChannelId { get; set; }

		public bool LevelAnnouncementsEnabled { get; set; } = true;
		/// <summary>
		/// Should we scan for osu! links?
		/// </summary>
		public bool AutomaticOsuLinkParsingEnabled { get; set; } = false;

		/// <summary>
		/// Whether or not the server currently has an active premium subscription.
		/// </summary>
		public bool IsPremium => PremiumExpiration.HasValue && PremiumExpiration.Value > DateTimeOffset.Now;

		public AntiRaidConfig AntiRaid { get; set; }
	}
}