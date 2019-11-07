using LinqToDB.Mapping;
using System;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "commands")]
    public class CoreCommand
    {
        [Column(Name = "CommandName"), NotNull]
        public string CommandName { get; set; }
        [Column(Name = "Alias"), Nullable]
        public string Phrase { get; set; }
        [Column(Name = "HelpString"), NotNull]
        public string HelpString { get; set; }
        [Column(Name = "Enabled"), NotNull]
        public bool Enabled { get; set; }
    }
}
