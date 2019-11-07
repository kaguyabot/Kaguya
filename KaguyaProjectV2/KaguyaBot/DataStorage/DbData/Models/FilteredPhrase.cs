using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "filteredphrases")]
    public class FilteredPhrase
    {
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "Phrase"), NotNull]
        public string Phrase { get; set; }
        /// <summary>
        /// FK_KaguyaServer_FilteredPhrases
        /// </summary>
        [Association(ThisKey = "ServerId", OtherKey = "Id", CanBeNull = false)]
        public Server Server { get; set; }
    }

}
