using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public class AntiRaidConfig
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong ServerId { get; set; }

		public int Users { get; set; }

		public int Seconds { get; set; }

		public string Action { get; set; }

		public bool Enabled { get; set; }

		/// <summary>
		/// Upon anti-raid execution, if this value is set, Kaguya will send a DM to whoever was punished
		/// by the anti-raid service with this property as the message's content.
		/// </summary>
		public string AntiraidPunishmentDirectMessage { get; set; }

		public KaguyaServer Server { get; set; }
	}
}