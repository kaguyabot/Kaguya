using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;

// ReSharper disable UseAwaitUsing

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public class UtilityQueries
    {
        public static async Task AddOrReplaceKeyAsync(SupporterKey key)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(key);
            }
        }

        public static async Task<bool> KeyExists(SupporterKey key)
        {
            using (var db = new KaguyaDb())
            {
                return await (from k in db.SupporterKeys
                    where k.Key == key.Key
                    select k != null).FirstAsync();
            }
        }

        public static async Task AddKeys(IEnumerable<SupporterKey> keys)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(keys);
            }
        }

        public static async Task DeleteKey(SupporterKey key)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(key);
            }
        }

        public static async Task<IEnumerable<SupporterKey>> GetAllExpiredKeys()
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.SupporterKeys
                    where s.Expiration < DateTime.Now.ToOADate()
                    select s).ToListAsync();
            }
        }

        public static async Task<List<SupporterKey>> GetAllActiveKeys()
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.SupporterKeys
                    where s.Expiration > DateTime.Now.ToOADate() && s.UserId != 0
                    select s).ToListAsync();
            }
        }

        /// <summary>
        /// Returns an IEnumerable<SupporterKey> containing a collection of all keys that a user has active currently.</SupporterKey> 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SupporterKey>> GetKeysBoundToUser(ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from k in db.SupporterKeys
                    where k.UserId == userId
                    select k).ToListAsync();
            }
        }

        public static async Task<IEnumerable<ServerExp>> GetAllExpForServer(Server server)
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.ServerExp
                    where s.ServerId == server.Id
                    select s).ToListAsync();
            }
        }
    }
}
