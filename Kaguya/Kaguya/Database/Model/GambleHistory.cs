using System;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Internal.Enums;

namespace Kaguya.Database.Model
{
    public class GambleHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        public ulong UserId { get; set; }
        public GambleAction Action { get; set; }
        public int AmountBet { get; set; }
        public int AmountRewarded { get; set; }
        public bool IsWinner { get; set; }
        public DateTime Timestamp { get; set; }
    }
}