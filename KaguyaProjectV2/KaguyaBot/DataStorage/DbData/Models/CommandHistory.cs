using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;
using System;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "commandhistory")]
    public class CommandHistory : IKaguyaQueryable<CommandHistory>,
        IUserSearchable<CommandHistory>,
        IServerSearchable<CommandHistory>
    {
        [Column(Name = "UserId")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "ServerId")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "Command")]
        [NotNull]
        public string Command { get; set; }

        [Column(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// FK_KaguyaServer_AutoAssignedRoles
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }
}