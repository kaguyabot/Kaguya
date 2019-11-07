using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "warnactions")]
    public class WarnAction
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "AmountWarnings"), NotNull]
        public int AmountWarnings { get; set; }
        [Column(Name = "Action"), NotNull]
        public int Action { get; set; }
        /// <summary>
        /// FK_KaguyaServer_WarnActions
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }

}
