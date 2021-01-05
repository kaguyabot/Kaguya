using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Internal.Enums;

namespace Kaguya.Database.Model
{
	public class AntiRaidConfig
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong ServerId { get; set; }

		public int Users { get; set; }

		public int Seconds { get; set; }

		public AntiraidAction Action { get; set; }
		
		public string ActionString { get; set; }
		/// <summary>
		/// At what time should the user be un-punished, if applicable?
		/// </summary>
		public DateTime? Expiration { get; set; }
		
		/// <summary>
		/// Upon anti-raid execution, if this value is set, Kaguya will send a DM to whoever was punished
		/// by the anti-raid service with this property as the message's content.
		/// </summary>
		public string AntiraidPunishmentDirectMessage { get; set; }

		public bool Enabled { get; set; }

		public bool IsExpirable => this.Action != AntiraidAction.Kick;
	}
}