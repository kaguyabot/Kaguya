using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "praise")]
    public class Praise : IKaguyaQueryable<Praise>,
        IServerSearchable<Praise>,
        IUserSearchable<Praise>
    {
        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "given_by")]
        [NotNull]
        public ulong GivenBy { get; set; }

        [Column(Name = "time_given")]
        [NotNull]
        public double TimeGiven { get; set; }

        [Column(Name = "reason")]
        [NotNull]
        public string Reason { get; set; }

        [Association(ThisKey = "server_id", OtherKey = "server_id")]
        public Server Server { get; set; }
    }
}