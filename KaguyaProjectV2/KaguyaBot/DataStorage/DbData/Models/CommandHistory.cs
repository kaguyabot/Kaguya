using System;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "commandhistory")]
    public class CommandHistory
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Command"), NotNull]
        public string Command { get; set; }
        [Column(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
