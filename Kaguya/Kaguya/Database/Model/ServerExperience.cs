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

        /// <summary>
        /// Adds the absolute value of <see cref="amount"/> to the current object's <see cref="Exp"/> value.
        /// </summary>
        /// <param name="amount"></param>
        public void AddExp(int amount) => Exp += Math.Abs(amount);

        /// <summary>
        /// Subtracts the absolute value of <see cref="amount"/> to the current object's <see cref="Exp"/> value.
        /// </summary>
        /// <param name="amount"></param>
        public void SubtractExp(int amount)
        {
            if (Math.Abs(amount) - Exp < 0)
            {
                Exp = 0;
            }
            else
            {
                Exp -= Math.Abs(amount);  
            }
        } 
    }
}