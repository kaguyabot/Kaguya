using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "filteredphrases")]
    public class FilteredPhrases
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "Phase"), NotNull]
        public string Phase { get; set; }
        /// <summary>
        /// FK_KaguyaServer_FilteredPhrases
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }

}
