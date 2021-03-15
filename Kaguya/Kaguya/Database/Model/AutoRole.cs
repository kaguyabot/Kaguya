using Kaguya.Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
	public class AutoRole : IServerSearchable
	{
		[Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public ulong ServerId { get; set; }
		public ulong RoleId { get; set; }
	}
}