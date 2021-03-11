using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;

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
	public class FilteredWord : IServerSearchable
	{
		[Key][Column(Order = 0)]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong ServerId { get; set; }
		[Key][Column(Order = 1)]
		public string Word { get; set; }
		public FilterReactionEnum FilterReaction { get; set; }
		public string FilterReactionString => this.FilterReaction.ToString();
	}
}