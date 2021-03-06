using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;

namespace Kaguya.Database.Model
{
    public class RoleReward : IServerSearchable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
        public int Level { get; set; }
    }
}