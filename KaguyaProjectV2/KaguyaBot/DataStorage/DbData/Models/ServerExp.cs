using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "serverexp")]
    public class ServerExp
    {
        [Column(Name = "ServerId"), NotNull, PrimaryKey]
        public ulong ServerId { get; set; }
        [Column(Name = "UserId"), NotNull, PrimaryKey]
        public ulong UserId { get; set; }
        [Column(Name = "Exp"), NotNull]
        public int Exp { get; set; }
        [Column(Name = "LatestExp"), NotNull]
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