using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class Rep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GivenBy { get; set; }
        public DateTimeOffset TimeGiven { get; set; }
        public string Reason { get; set; }
    }
}