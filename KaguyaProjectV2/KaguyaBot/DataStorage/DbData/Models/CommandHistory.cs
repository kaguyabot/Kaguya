using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "commandhistory")]
    public class CommandHistory
    {
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Command"), NotNull]
        public string Command { get; set; }
        /// <summary>
        /// FK_KaguyaUser_CommandHistory
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }

}
