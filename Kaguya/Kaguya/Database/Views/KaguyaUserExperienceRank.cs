using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Views
{
    [Keyless]
    public class KaguyaUserExperienceRank
    {
        public ulong UserId { get; set; }
        public int Rank { get; set; }
    }
}