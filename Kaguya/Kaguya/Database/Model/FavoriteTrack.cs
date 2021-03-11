using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;

namespace Kaguya.Database.Model
{
    public class FavoriteTrack : IUserSearchable, IServerSearchable
    {
        [Key][Column(Order = 0)]
        public ulong UserId { get; set; }
        [Key][Column(Order = 1)]
        public string SongId { get; set; }
        /// <summary>
        /// The server id in which this track was favorited in.
        /// </summary>
        public ulong ServerId { get; set; }
        public DateTimeOffset DateAdded { get; set; }
    }
}