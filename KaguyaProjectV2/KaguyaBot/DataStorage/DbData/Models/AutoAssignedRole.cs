using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "autoassignedroles")]
    public class AutoAssignedRole : IKaguyaQueryable<AutoAssignedRole>, IServerSearchable<AutoAssignedRole>
    {
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "role_id")]
        [NotNull]
        public ulong RoleId { get; set; }

        /// <summary>
        /// FK_KaguyaServer_AutoAssignedRoles
        /// </summary>
        [Association(ThisKey = "server_id", OtherKey = "id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}