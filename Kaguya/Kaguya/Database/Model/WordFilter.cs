using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public enum FilterReactionEnum
	{
		Delete,
		Mute,
		Kick,
		Ban,
		ShadowBan
	}
	public class WordFilter
	{
		[Key, Column(Order = 0)]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong ServerId { get; set; }
		[Key, Column(Order = 1)]
		public string Word { get; set; }

		public FilterReactionEnum FilterReaction { get; set; }
	}
}