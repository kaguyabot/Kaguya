using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "reactionroles")]
    public class ReactionRole : IKaguyaQueryable<ReactionRole>, IServerSearchable<ReactionRole>
    {
        /// <summary>
        /// The ID of the emote that this reaction role is assigned to.
        /// </summary>
        [Column(Name = "emoteid"), NotNull]
        public ulong EmoteId { get; }
        
        /// <summary>
        /// The ID of the role that this reaction role is assigned to.
        /// </summary>
        [Column(Name = "roleid"), NotNull]
        public ulong RoleId { get; }
        
        /// <summary>
        /// The ID of the message that this reaction role is attached to.
        /// </summary>
        [Column(Name = "messageid"), NotNull]
        public ulong MessageId { get; }
        /// <summary>
        /// The ID of the server in which this reaction role lives.
        /// </summary>
        [Column(Name = "serverid"), NotNull]
        public ulong ServerId { get; }
        
        public ReactionRole(ulong emoteId, ulong roleId, ulong messageId, ulong serverId)
        {
            this.EmoteId = emoteId;
            this.RoleId = roleId;
            this.MessageId = messageId;
            this.ServerId = serverId;
        }

        public ReactionRole()
        {
            
        }
    }
}