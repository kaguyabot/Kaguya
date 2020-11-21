using System;
using System.ComponentModel.DataAnnotations;
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
		public ulong EntityId { get; set; }
		public BlacklistedEntityType EntityType { get; set; }
		// null = forever muwahahaha
		public DateTime? ExpirationTime { get; set; }
		public string Reason { get; set; }
	}
}