using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class RoleReward
    {
        [Key, Column(Order = 0)]
        public ulong ServerId { get; set; }
        [Key, Column(Order = 1)]
        public ulong RoleId { get; set; }
        public int Level { get; set; }
    }
}