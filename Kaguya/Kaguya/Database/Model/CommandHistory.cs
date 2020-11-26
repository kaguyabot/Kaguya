using System;

namespace Kaguya.Database.Model
{
	public class CommandHistory
	{
		public ulong UserId { get; set; }
		public ulong ServerId { get; set; }
		public string CommandName { get; set; }
		public string Message { get; set; }
		public bool ExecutedSuccessfully { get; set; }
		public DateTime ExecutionTime { get; set; }
	}
}