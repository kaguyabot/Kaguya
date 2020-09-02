using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "reactionroles")]
    public class ReactionRole : IKaguyaQueryable<ReactionRole>, IServerSearchable<ReactionRole>
    {
        /// <summary>
        /// The ID of the role that this reaction role is assigned to.
        /// </summary>
        [Column(Name = "roleid"), NotNull]
        public ulong RoleId { get; set; }
        /// <summary>
        /// The ID of the message that this reaction role is attached to.
        /// </summary>
        [Column(Name = "messageid"), NotNull]
        public ulong MessageId { get; set;}
        /// <summary>
        /// The ID of the server in which this reaction role lives.
        /// </summary>
        [Column(Name = "serverid"), NotNull]
        public ulong ServerId { get; set; }
        /// <summary>
        /// The ID of the emote that this reaction role is assigned to. If the reaction itself is
        /// actually a standard <see cref="Emoji"/>, we need to store its name instead. i.e. :joy: :beach:
        /// </summary>
        [Column(Name = "emotenameid"), Nullable]
        public string EmoteNameorId { get; set; }
    }
}