using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using Humanizer.Localisation;

namespace Kaguya.Database.Model
{
    public class PremiumKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Key]
        public string Key { get; set; }
        public ulong KeyCreatorId { get; set; }
        public int LengthInSeconds { get; set; }
        public DateTime? Expiration { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
        public string HumanizedLength => TimeSpan.FromSeconds(this.LengthInSeconds).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);
        public bool IsRedeemed => Expiration.HasValue;
    }
}