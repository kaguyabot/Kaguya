using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class UserQueries
    {
        public static async Task<User> GetUser(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                return await (from u in db.Users
                    where u.Id == Id
                    select u).FirstAsync();
            }
        }

        public static async Task<int> GetCountOfUsers()
        {
            using(var db = new KaguyaDb())
            {
                return await db.Users.CountAsync();
            }
        }

        public static int UserGlobalExpRank(User user)
        {
            using (var db = new KaguyaDb())
            {
                return db.Users.OrderByDescending(x => x.Experience).ToList().FindIndex(x => x.Id == user.Id);
            }
        }

        public static async Task UpdateUser(User user)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(user);
            }
        }

        public static async Task UpdateUsers(IEnumerable<User> users)
        {
            using (var db = new KaguyaDb())
            {
                await Task.Run(() => { db.BulkCopy(users); });
            }
        }

        public static async Task AddCommandHistory(CommandHistory chObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(chObject);
            }
        }

        public static async Task RemoveCommandHistory(CommandHistory chObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(chObject);
            }
        }

        public static async Task AddGambleHistory(GambleHistory ghObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(ghObject);
            }
        }

        public static async Task RemoveGambleHistory(GambleHistory ghObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(ghObject);
            }
        }

        /// <summary>
        /// Returns a collection of users that have used a command recently. If their "ActiveRateLimit" is too high,
        /// we will ratelimit them as to protect the bot from malicious slowdowns.
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<User>> UsersWhoHaveAnActiveRatelimit()
        {
            using (var db = new KaguyaDb())
            {
                return await (from u in db.Users
                    where u.ActiveRateLimit > 0
                    select u).ToListAsync();
            }
        }
    }
}