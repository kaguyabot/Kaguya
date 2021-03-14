using Kaguya.Database.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public class Poll : IUserSearchable, IServerSearchable
	{
		[Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		/// <summary>
		/// The ID of the user who created the poll
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The ID of the server in which this poll lives
		/// </summary>
		public ulong ServerId { get; set; }
		/// <summary>
		/// The ID of the channel that this poll lives in
		/// </summary>
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The ID of the message that this poll belongs to
		/// </summary>
		public ulong MessageId { get; set; }
		/// <summary>
		/// The title of the poll
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// When the poll is set to expire
		/// </summary>
		public DateTimeOffset Expiration { get; set; }
		/// <summary>
		/// All votable options for this poll
		/// </summary>
		public string Args { get; set; }
		/// <summary>
		/// A flag that indicates whether the poll has concluded. A poll must be marked as
		/// triggered once the poll message is updated to reflect that the poll has ended.
		/// </summary>
		public bool HasTriggered { get; set; }
	}
}