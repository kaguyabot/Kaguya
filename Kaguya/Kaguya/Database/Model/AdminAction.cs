using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public class AdminAction
	{
		public const string KickAction = "Kick";
		public const string BanAction = "Ban";
		public const string TempBanAction = "Temporary Ban";
		public const string UnbanAction = "Unban";
		public const string ShadowbanAction = "Shadowban";
		public const string UnshadowbanAction = "Unshadowban";
		public const string WarnAction = "Warn";
		public const string UnwarnAction = "Unwarn";
		public const string MuteAction = "Mute";
		public const string UnmuteAction = "Unmute";
		
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public uint Id { get; private set; }
		public ulong ServerId { get; init; }
		public ulong ModeratorId { get; init; }
		public ulong ActionedUserId { get; init; }
		public string Action { get; init; }
		public string Reason { get; set; }
		public DateTime? Expiration { get; init; }
	}
}