using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "server_exp")]
    public class ServerExp : IKaguyaQueryable<ServerExp>,
        IKaguyaUnique<ServerExp>,
        IServerSearchable<ServerExp>,
        IUserSearchable<ServerExp>
    {
        [PrimaryKey]
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "exp")]
        [NotNull]
        public int Exp { get; set; }

        [Column(Name = "latest_exp")]
        [NotNull]
        public double LatestExp { get; set; }

        /// <summary>
        /// FK_KaguyaServer_ServerExp
        /// </summary>
        [Association(ThisKey = "server_id", OtherKey = "id", CanBeNull = false)]
        public Server Server { get; set; }

        /// <summary>
        /// FK_KaguyaUser_ServerExp
        /// </summary>
        [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
        public User User { get; set; }
    }
}