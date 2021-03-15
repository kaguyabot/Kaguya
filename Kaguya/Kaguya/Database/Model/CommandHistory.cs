using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;

namespace Kaguya.Database.Model
{
	public class CommandHistory : IUserSearchable, IServerSearchable
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public ulong UserId { get; set; }
		public ulong ServerId { get; set; }
		public string CommandName { get; set; }
		public string Message { get; set; }
		public bool ExecutedSuccessfully { get; set; }
		public string ErrorMessage { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}