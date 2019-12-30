using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class UserQueries
    {
        public static async Task<User> GetOrCreateUser(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                User user;
                try
                {
                    user = await (from u in db.Users
                        where u.Id == Id
                        select u).FirstAsync();
                }
                catch (InvalidOperationException)
                {
                    user = new User
                    {
                        Id = Id
                    };
                    await db.InsertAsync(user);
                }

                return user;
            }
        }

        public static int GetGlobalExpRankIndex(User user)
        {
            using (var db = new KaguyaDb())
            {
                return db.Users.OrderByDescending(x => x.Experience).ToList().FindIndex(x => x.Id == user.Id);
            }
        }

        public static async Task UpdateUserAsync(User user)
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
                foreach (var user in users)
                {
                    var storedUser = from u in db.Users
                        where u.Id == user.Id
                        select u;

                    if (!storedUser.Equals(user))
                    {
                        await db.UpdateAsync(user);
                    }
                }
            }
        }

        public static async Task AddCommandHistory(CommandHistory chObject)
        {
            using (var db = new KaguyaDb())
            {
                if (!db.CommandHistories.Contains(chObject))
                {
                    await db.InsertAsync(chObject);
                }
            }
        }

        public static async Task RemoveCommandHistory(CommandHistory chObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(chObject);
            }
        }

        public static async Task<List<CommandHistory>> GetCommandHistoryForUserAsync(User user)
        {
            using (var db = new KaguyaDb())
            {
                return await (from h in db.CommandHistories
                    where h.UserId == user.Id
                    select h).ToListAsync();
            }
        }

        public static async Task<List<CommandHistory>> GetCommandHistoryForUserAsync(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                return await (from h in db.CommandHistories
                    where h.UserId == Id
                    select h).ToListAsync();
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

        /// <summary>
        /// Returns all rep obtained by a user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<List<Rep>> GetRepAsync(User user)
        {
            using (var db = new KaguyaDb())
            {
                return await (from r in db.Rep
                    where r.UserId == user.Id
                    select r).ToListAsync();
            }
        }

        /// <summary>
        /// Returns all rep obtained by a user.
        /// </summary>
        /// <param name="Id">The Id of the user.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<Rep>> GetRepAsync(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                return await (from r in db.Rep
                    where r.UserId == Id
                    select r).ToListAsync();
            }
        }

        public static async Task RemoveRepAsync(Rep repObj)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(repObj);
            }
        }

        /// <summary>
        /// Inserts a new rep object into the database. The Rep object's UserId parameter
        /// indicates who should be given the rep point. If we're adding +1 Rep to Johnny,
        /// we need to put Johnny's ID for this parameter.
        /// </summary>
        /// <param name="repObj"></param>
        /// <returns></returns>
        public static async Task AddRepAsync(Rep repObj)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(repObj);
            }
        }
    }
}