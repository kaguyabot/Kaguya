using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "warnedusers")]
    public class WarnedUser
    {
        [PrimaryKey]
        public ulong ServerId { get; set; }
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "Reason"), Nullable]
        public string Reason { get; set; }
        /// <summary>
        /// FK_KaguyaServer_WarnActions
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }
}
