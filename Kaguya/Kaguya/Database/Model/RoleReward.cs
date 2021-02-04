using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class RoleReward
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
        public int Level { get; set; }
    }
}