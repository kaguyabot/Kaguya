using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IFavoriteTrackRepository : IRepository<FavoriteTrack>
	{
		public Task<IList<FavoriteTrack>> GetAllAsync(ulong userId);
	}
}