using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;
using LinqToDB.Data;

// ReSharper disable UseAwaitUsing

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public class UtilityQueries
    {
        public static async Task AddOrReplaceSupporterKeyAsync(SupporterKey key)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(key);
            }
        }

        public static async Task<bool> SupporterKeyExistsAsync(SupporterKey key)
        {
            using (var db = new KaguyaDb())
            {
                return await (from k in db.SupporterKeys
                    where k.Key == key.Key
                    select k != null).FirstAsync();
            }
        }

        public static void AddSupporterKeys(IEnumerable<SupporterKey> keys)
        {
            using (var db = new KaguyaDb())
            {
                db.BulkCopy(keys);
            }
        }

        public static async Task DeleteSupporterKeyAsync(SupporterKey key)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(key);
            }
        }

        public static async Task<IEnumerable<SupporterKey>> GetAllExpiredSupporterKeysAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.SupporterKeys
                    where s.Expiration < DateTime.Now.ToOADate()
                    select s).ToListAsync();
            }
        }

        public static async Task<List<SupporterKey>> GetAllActiveSupporterKeysAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.SupporterKeys
                    where s.Expiration > DateTime.Now.ToOADate() && s.UserId != 0
                    select s).ToListAsync();
            }
        }

        public static async Task<List<SupporterKey>> GetAllSupporterKeysAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<SupporterKey>().ToListAsync();
            }
        }

        /// <summary>
        /// Returns an IEnumerable<SupporterKey> containing a collection of all keys that a user has active currently.</SupporterKey> 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SupporterKey>> GetSupporterKeysBoundToUserAsync(ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from k in db.SupporterKeys
                    where k.UserId == userId
                    select k).ToListAsync();
            }
        }

        public static async Task AddOrReplacePremiumKeyAsync(PremiumKey key)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(key);
            }
        }

        public static async Task<bool> PremiumKeyExistsAsync(PremiumKey key)
        {
            using (var db = new KaguyaDb())
            {
                return await (from k in db.SupporterKeys
                              where k.Key == key.Key
                              select k != null).FirstAsync();
            }
        }

        public static void AddPremiumKeys(IEnumerable<PremiumKey> keys)
        {
            using (var db = new KaguyaDb())
            {
                db.BulkCopy(keys);
            }
        }

        public static async Task DeletePremiumKeyAsync(PremiumKey key)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(key);
            }
        }

        public static async Task<IEnumerable<PremiumKey>> GetAllExpiredPremiumKeysAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.PremiumKeys
                              where s.Expiration < DateTime.Now.ToOADate()
                              select s).ToListAsync();
            }
        }

        public static async Task<List<PremiumKey>> GetAllActivePremiumKeysAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.PremiumKeys
                              where s.Expiration > DateTime.Now.ToOADate() && s.UserId != 0
                              select s).ToListAsync();
            }
        }

        public static async Task<List<PremiumKey>> GetAllPremiumKeysAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<PremiumKey>().ToListAsync();
            }
        }

        /// <summary>
        /// Returns an IEnumerable<PremiumKey> containing a collection of all keys that a server currently has bound to it.</SupporterKey> 
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PremiumKey>> GetPremiumKeysBoundToServerAsync(ulong serverId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from k in db.PremiumKeys
                              where k.ServerId == serverId
                              select k).ToListAsync();
            }
        }

        public static async Task<List<ServerExp>> GetAllExpForServerAsync(Server server)
        {
            using (var db = new KaguyaDb())
            {
                return await (from s in db.ServerExp
                    where s.ServerId == server.Id
                    select s).ToListAsync();
            }
        }

        public static async Task<IEnumerable<Reminder>> GetAllRemindersAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await db.Reminders.ToListAsync();
            }
        }

        /// <summary>
        /// Gets all expired reminders.
        /// </summary>
        /// <param name="hasTriggered">Whether we want to return all expired reminders where the expiration DM
        /// has already been sent out.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<Reminder>> GetAllExpiredRemindersAsync(bool hasTriggered)
        {
            using (var db = new KaguyaDb())
            {
                return await (from r in db.Reminders
                    where r.HasTriggered == hasTriggered && r.Expiration < DateTime.Now.ToOADate()
                    select r).ToListAsync();
            }
        }

        public static async Task AddReminderAsync(Reminder reminder)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(reminder);
            }
        }
        public static async Task DeleteReminderAsync(Reminder reminder)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(reminder);
            }
        }
    }
}
