using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "autoassignedroles")]
    public class AutoAssignedRole : IKaguyaQueryable<AutoAssignedRole>, IServerSearchable<AutoAssignedRole>
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "RoleId"), NotNull]
        public ulong RoleId { get; set; }
        /// <summary>
        /// FK_KaguyaServer_AutoAssignedRoles
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}
