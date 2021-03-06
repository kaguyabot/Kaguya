using System;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;
using Kaguya.Internal.Enums;

namespace Kaguya.Database.Model
{
    public class GambleHistory : IUserSearchable, IServerSearchable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
        public GambleAction Action { get; set; }
        public int AmountBet { get; set; }
        public int AmountRewarded { get; set; }
        public bool IsWinner { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}