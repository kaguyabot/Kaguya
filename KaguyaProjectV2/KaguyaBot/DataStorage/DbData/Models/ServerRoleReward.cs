using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "serverrolerewards")]
    public class ServerRoleReward : IKaguyaQueryable<ServerRoleReward>, IServerSearchable<ServerRoleReward>
    {
        [Column(Name = "ServerId")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "RoleId")]
        [NotNull]
        public ulong RoleId { get; set; }

        [Column(Name = "Level")]
        [NotNull]
        public int Level { get; set; }

        [Association(ThisKey = "ServerId", OtherKey = "ServerId")]
        public Server Server { get; set; }
    }
}