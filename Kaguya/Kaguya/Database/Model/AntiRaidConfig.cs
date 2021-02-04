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

		public uint UserThreshold { get; set; }

		public uint Seconds { get; set; }

		public AntiraidAction Action { get; set; }
		
		/// <summary>
		/// How long should this user be punished for by the system?
		/// </summary>
		public TimeSpan? PunishmentLength { get; set; }
		
		/// <summary>
		/// Upon anti-raid execution, if this value is set, Kaguya will send a DM to whoever was punished
		/// by the anti-raid service with this property as the message's content.
		/// </summary>
		public string AntiraidPunishmentDirectMessage { get; set; }
		public bool ConfigEnabled { get; set; }
		public bool PunishmentDmEnabled { get; set; }
	}
}