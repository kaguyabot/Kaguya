using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "server_role_rewards")]
    public class ServerRoleReward : IKaguyaQueryable<ServerRoleReward>, IServerSearchable<ServerRoleReward>
    {
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "role_id")]
        [NotNull]
        public ulong RoleId { get; set; }

        [Column(Name = "level")]
        [NotNull]
        public int Level { get; set; }

        [Association(ThisKey = "server_id", OtherKey = "ServerId")]
        public Server Server { get; set; }
    }
}