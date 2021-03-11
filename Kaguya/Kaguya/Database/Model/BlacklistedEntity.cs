using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public enum BlacklistedEntityType
	{
		User,
		Channel,
		Server
	}
	
	public class BlacklistedEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong EntityId { get; set; }
		public BlacklistedEntityType EntityType { get; set; }
		// null = forever muwahahaha
		public DateTimeOffset? ExpirationTime { get; set; }
		public string Reason { get; set; }

		[NotMapped]
		public bool HasExpired => this.ExpirationTime.HasValue && this.ExpirationTime.Value < DateTimeOffset.Now;
	}
}