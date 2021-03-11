using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using Humanizer.Localisation;
using Kaguya.Internal.Extensions.DiscordExtensions;

namespace Kaguya.Database.Model
{
    public class PremiumKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Key { get; set; }
        public ulong KeyCreatorId { get; set; }
        public int LengthInSeconds { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public ulong? UserId { get; set; }
        public ulong? ServerId { get; set; }
        public string HumanizedLength => TimeSpan.FromSeconds(this.LengthInSeconds).HumanizeTraditionalReadable();
        public bool IsRedeemed => this.Expiration.HasValue;
    }
}