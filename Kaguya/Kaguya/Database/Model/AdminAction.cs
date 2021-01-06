using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public class AdminAction
	{
		public const string KickAction = "Kick";
		public const string BanAction = "Ban";
		public const string UnbanAction = "Unban";
		public const string ShadowbanAction = "Shadowban";
		public const string UnshadowbanAction = "Unshadowban";
		public const string WarnAction = "Warn";
		public const string UnwarnAction = "Unwarn";
		public const string MuteAction = "Mute";
		public const string UnmuteAction = "Unmute";

		public static readonly string[] AllActions =
		{
			KickAction, BanAction, UnbanAction, ShadowbanAction, UnshadowbanAction, WarnAction, 
			UnwarnAction, MuteAction, UnmuteAction
		};
		
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; private set; }
		public ulong ServerId { get; init; }
		public ulong ModeratorId { get; init; }
		public ulong ActionedUserId { get; init; }
		public string Action { get; set; }
		public string Reason { get; set; }
		public DateTime? Expiration { get; set; }
		// Used if admins "erase history" for some data in their server to keep it from coming up in other commands.
		public bool IsHidden { get; set; }
		/// <summary>
		/// Whether this action was performed by the system as part of an automated service.
		/// </summary>
		public bool IsSystemAction { get; init; }
	}
}