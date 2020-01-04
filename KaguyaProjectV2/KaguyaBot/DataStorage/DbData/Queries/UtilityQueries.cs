using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                    where s.ServerId == server.ServerId
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

        public static async Task<List<CommandHistory>> GetCommandHistoryLast24HoursAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await (from h in db.CommandHistories
                    where h.Timestamp >= DateTime.Now.AddHours(-24)
                    select h).ToListAsync();
            }
        }

        public static async Task<int> GetTotalCommandCountAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await db.CommandHistories.CountAsync();
            }
        }

        /// <summary>
        /// Returns the most popular command of all time, with how many uses it has.
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string, int>> GetMostPopularCommandAsync()
        {
            using (var db = new KaguyaDb())
            {
                var commandQuery = (from h in db.CommandHistories
                    group h by h.Command
                    into grp
                    orderby grp.Count() descending
                    select grp.Key);

                var command = await commandQuery.FirstAsync();
                var count = await commandQuery.CountAsync();

                var dic = new Dictionary<string, int>();
                dic.Add(command, count);

                return dic;
            }
        }

        public static async Task<int> GetCountOfUsersAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await db.Users.CountAsync();
            }
        }

        public static int GetTotalCurrency()
        {
            using (var db = new KaguyaDb())
            {
                int currency = 0;
                foreach (var user in db.Users)
                {
                    currency += user.Points;
                }

                return currency;
            }
        }

        public static async Task<int> GetTotalGamblesAsync()
        {
            using (var db = new KaguyaDb())
            {
                return await db.GambleHistories.CountAsync();
            }
        }

        public static async Task<bool> FishExistsAsync(long fishId)
        {
            using (var db = new KaguyaDb())
            {
                return await db.Fish.AnyAsync(x => x.FishId == fishId);
            }
        }

        /// <summary>
        /// Determines whether this <see cref="Fish"/> belongs to the specified <see cref="User"/>.
        /// </summary>
        /// <param name="fish"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<bool> FishBelongsToUserAsync(Fish fish, User user)
        {
            using (var db = new KaguyaDb())
            {
                return await db.Fish.AnyAsync(x => x.FishId == fish.FishId && x.UserId == user.Id);
            }
        }

        /// <summary>
        /// Determines whether the given fishId belongs to the specified <see cref="User"/>.
        /// </summary>
        /// <param name="fish"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<bool> FishBelongsToUserAsync(long fishId, User user)
        {
            using (var db = new KaguyaDb())
            {
                return await db.Fish.AnyAsync(x => x.FishId == fishId && x.UserId == user.Id);
            }
        }
    }
}