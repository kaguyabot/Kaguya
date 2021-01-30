using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class ServerExperience
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ServerId { get; set; }
        [Key, Column(Order = 1)]
        public ulong UserId { get; set; }
        public int Exp { get; private set; }
        public DateTime? LastGivenExp { get; set; }

        public void AdjustExp(int amount)
        {
            if (amount + Exp < 0)
            {
                Exp = 0;
            }

            Exp += amount;
        }
    }
}