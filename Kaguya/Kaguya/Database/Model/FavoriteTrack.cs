using System;

namespace Kaguya.Database.Model
{
    public class FavoriteTrack
    {
        public ulong UserId { get; set; }
        public string SongId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}