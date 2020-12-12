using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

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
		public DateTime? ExpirationTime { get; set; }
		public string Reason { get; set; }

		[NotMapped]
		public bool HasExpired => ExpirationTime.HasValue && ExpirationTime.Value < DateTime.Now;
	}
}