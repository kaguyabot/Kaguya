using Kaguya.Internal.Enums;
using System;
using System.Collections.Generic;

namespace Kaguya.Internal.Events.ArgModels
{
	public class AntiraidEventData
	{
		/// <summary>
		///  A collection of IDs to action
		/// </summary>
		public IList<ulong> UserIds { get; set; }
		public string DmMessage { get; set; }
		public ModerationAction Action { get; set; }
		/// <summary>
		///  When the appropriate punishment would be lifted.
		/// </summary>
		public DateTimeOffset? Expiration { get; set; }
	}
}