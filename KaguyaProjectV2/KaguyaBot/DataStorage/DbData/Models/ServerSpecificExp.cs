using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "serverexp")]
    public class ServerSpecificExp
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "UserId"), Nullable]
        public ulong UserId { get; set; }
        [Column(Name = "Exp"), Nullable]
        public int Exp { get; set; }
        [Column(Name = "LatestExp"), Nullable]
        public double LatestExp { get; set; }
    }
}