using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "quotes")]
    public class Quote : IKaguyaQueryable<Quote>, IUserSearchable<Quote>, IServerSearchable<Quote>
    {
        /// <summary>
        /// The ID of the user that created the quote.
        /// </summary>
        /// <value></value>
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        /// <summary>
        /// The ID of the server in which this quote was created in.
        /// </summary>
        /// <value></value>
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        /// <summary>
        /// The quote's text.
        /// </summary>
        /// <value></value>
        [Column(Name = "Text"), NotNull]
        public string Text { get; set; }
        /// <summary>
        /// The time, in OADate, at which the quote was created.
        /// </summary>
        /// <value></value>
        [Column(Name = "TimeStamp"), NotNull]
        public double TimeStamp { get; set; }
        /// <summary>
        /// The incremental ID that represents how many total quotes have been created in the server.
        /// </summary>
        /// <value></value>
        [Column(Name = "Id"), NotNull]
        public int Id { get; set; }
    }
}