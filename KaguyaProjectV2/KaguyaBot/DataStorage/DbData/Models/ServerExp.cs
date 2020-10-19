using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "serverexp")]
    public class ServerExp : IKaguyaQueryable<ServerExp>,
        IKaguyaUnique<ServerExp>,
        IServerSearchable<ServerExp>,
        IUserSearchable<ServerExp>
    {
        [PrimaryKey]
        [Column(Name = "ServerId")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "UserId")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "Exp")]
        [NotNull]
        public int Exp { get; set; }

        [Column(Name = "LatestExp")]
        [NotNull]
        public double LatestExp { get; set; }

        /// <summary>
        /// FK_KaguyaServer_ServerExp
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }

        /// <summary>
        /// FK_KaguyaUser_ServerExp
        /// </summary>
        [Association(ThisKey = "UserId", OtherKey = "Id", CanBeNull = false)]
        public User User { get; set; }
    }
}