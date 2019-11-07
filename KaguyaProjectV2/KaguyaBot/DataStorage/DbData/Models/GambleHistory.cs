using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "gamblehistory")]
    public class GambleHistory
    {
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Action"), NotNull]
        public int Action { get; set; }
        /// <summary>
        /// FK_KaguyaUser_GambleHistory
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }

}
