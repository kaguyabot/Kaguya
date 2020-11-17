using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "filtered_phrases")]
    public class FilteredPhrase : IKaguyaQueryable<FilteredPhrase>, IKaguyaUnique<FilteredPhrase>, IServerSearchable<FilteredPhrase>
    {
        [Column(Name = "phrase")]
        [NotNull]
        public string Phrase { get; set; }

        /// <summary>
        ///     FK_KaguyaServer_FilteredPhrases
        /// </summary>
        [Association(ThisKey = "server_id", OtherKey = "id", CanBeNull = false)]
        public Server Server { get; set; }

        // todo: Redundant if there is an association with server.
        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        public bool Equals(FilteredPhrase other) => ServerId == other?.ServerId && Phrase == other.Phrase;
    }
}