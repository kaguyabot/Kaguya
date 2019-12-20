using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Column(Name = "userblacklists")]
    public class UserBlacklist
    {
        [PrimaryKey]
        public ulong UserId { get; set; }
        [Column(Name = "Expiration"), NotNull]
        public double Expiration { get; set; }
        [Column(Name = "Reason"), NotNull]
        public string Reason { get; set; }
    }
}
