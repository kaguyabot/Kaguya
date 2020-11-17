using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;
using System;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "command_history")]
    public class CommandHistory : IKaguyaQueryable<CommandHistory>,
        IUserSearchable<CommandHistory>,
        IServerSearchable<CommandHistory>
    {
        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "command")]
        [NotNull]
        public string Command { get; set; }

        [Column(Name = "timestamp")]
        public DateTime Timestamp { get; set; }

        [Association(ThisKey = "user_id", OtherKey = "user_id", CanBeNull = false)]
        public User User { get; set; }
        
        [Association(ThisKey = "server_id", OtherKey = "server_id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}