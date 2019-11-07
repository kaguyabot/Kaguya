using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "autoassignedroles")]
    public class AutoAssignedRoles
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "RoleId"), NotNull]
        public ulong RoleId { get; set; }
        [Column(Name = "Duration"), NotNull]
        public long Duration { get; set; }
        /// <summary>
        /// FK_KaguyaServer_AutoAssignedRoles
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }

}
