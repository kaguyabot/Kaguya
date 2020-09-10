using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "favorite_tracks")]
    public class FavoriteTrack : IKaguyaQueryable<FavoriteTrack>, IUserSearchable<FavoriteTrack>
    {
        [Column(Name = "user_id"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "track_id"), NotNull]
        public string TrackId { get; set; }
        [Column(Name = "track_title"), NotNull]
        public string TrackTitle { get; set; }
        [Column(Name = "track_author"), NotNull]
        public string TrackAuthor { get; set; }
        [Column(Name = "track_duration"), NotNull]
        public double TrackDuration { get; set; }
    }
}