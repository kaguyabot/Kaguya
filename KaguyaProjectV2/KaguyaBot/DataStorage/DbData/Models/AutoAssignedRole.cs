using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "auto_assigned_roles")]
    public class AutoAssignedRole : IKaguyaQueryable<AutoAssignedRole>, IServerSearchable<AutoAssignedRole>
    {
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "role_id")]
        [NotNull]
        public ulong RoleId { get; set; }

        [Association(ThisKey = "server_id", OtherKey = "server_id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}