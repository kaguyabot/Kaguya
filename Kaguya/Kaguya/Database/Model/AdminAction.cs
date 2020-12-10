using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public class AdminAction
	{
		public const string KICK_ACTION = "Kick";
		public const string BAN_ACTION = "Ban";
		public const string TEMP_BAN_ACTION = "Temporary Ban";
		public const string UNBAN_ACTION = "Unban";
		public const string SHADOWBAN_ACTION = "Shadowban";
		public const string UNSHADOWBAN_ACTION = "Unshadowban";
		public const string WARN_ACTION = "Warn";
		public const string UNWARN_ACTION = "Unwarn";
		public const string MUTE_ACTION = "Mute";
		public const string UNMUTE_ACTION = "Unmute";
		
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public uint Id { get; private set; }
		public ulong ServerId { get; init; }
		public ulong ModeratorId { get; init; }
		public ulong ActionedUserId { get; init; }
		public string Action { get; init; }
		public DateTime? Expiration { get; init; }
	}
}