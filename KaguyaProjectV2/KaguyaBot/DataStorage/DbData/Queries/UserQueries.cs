using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class UserQueries
    {
        public static async Task<Fish> GetFishAsync(long fishId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from f in db.Fish
                    where f.FishId == fishId
                    select f).FirstAsync();
            }
        }
    }
}