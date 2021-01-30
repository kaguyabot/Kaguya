using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Kaguya.Database.Model
{
    public class ReactionRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 0)]
        public ulong MessageId { get; set; }
        [Key, Column(Order = 1)]
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
        public string Emote { get; set; }
        /// <summary>
        /// This should be set to false if the Emote is a
        /// custom Discord emote, not a standard unicode emoji.
        /// </summary>
        public bool IsStandardEmoji { get; set; }
    }
}