using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class DatabaseQueries
    {
        public static async Task<bool> TestConnection()
        {
            using (var db = new KaguyaDb())
            {
                try
                {
                    return db.Connection.State.Equals(ConnectionState.Open);
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync($"Failed to establish database connection!! {e.Message}", LogLvl.ERROR);
                    return false;
                }
            }
        }

        public static async Task<Server> GetOrCreateServerAsync(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                bool exists = db.Servers.Any(x => x.ServerId == Id);

                if (!exists)
                {
                    await db.InsertAsync(new Server
                    {
                        ServerId = Id
                    });
                    await ConsoleLogger.LogAsync($"Server {Id} created.", LogLvl.DEBUG);
                }

                return await db.Servers
                    .LoadWith(x => x.AntiRaid)
                    .LoadWith(x => x.AutoAssignedRoles)
                    .LoadWith(x => x.BlackListedChannels)
                    .LoadWith(x => x.FilteredPhrases)
                    .LoadWith(x => x.MutedUsers)
                    .LoadWith(x => x.Praise)
                    .LoadWith(x => x.ServerExp)
                    .LoadWith(x => x.RoleRewards)
                    .LoadWith(x => x.WarnedUsers)
                    .Where(s => s.ServerId == Id).FirstAsync();
            }
        }

        public static async Task<User> GetOrCreateUserAsync(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                User user;
                try
                {
                    user = await (from u in db.Users
                                  where u.UserId == Id
                                  select u).FirstAsync();
                }
                catch (InvalidOperationException)
                {
                    user = new User
                    {
                        UserId = Id
                    };
                    await db.InsertAsync(user);
                    await ConsoleLogger.LogAsync($"User {Id} created.", LogLvl.DEBUG);
                }

                return user;
            }
        }

        public static int GetGlobalExpRankIndex(User user)
        {
            using (var db = new KaguyaDb())
            {
                return db.Users.OrderByDescending(x => x.Experience).ToList().FindIndex(x => x.UserId == user.UserId);
            }
        }

        /// <summary>
        /// Returns what rank the user is on the server-specific EXP "leaderboard". If they are
        /// the highest EXP holder, this value would be 1.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int GetServerExpRankForUser(Server server, User user)
        {
            using (var db = new KaguyaDb())
            {
                return server.ServerExp.OrderByDescending(x => x.Exp).ToList().FindIndex(x => x.UserId == user.UserId) + 1;
            }
        }

        public static double GetLastPraiseTime(ulong userId, ulong serverId)
        {
            using (var db = new KaguyaDb())
            {
                return (from r in db.Praise.OrderByDescending(x => x.TimeGiven)
                        where r.GivenBy == userId && r.ServerId == serverId
                        select r).First()?.TimeGiven ?? 0;
            }
        }

        /// <summary>
        /// Sells a <see cref="Fish"/> to the "market", then adds the value of the fish to the
        /// <see cref="User"/>'s points balance. This action will add the points to the user's account.
        /// </summary>
        /// <param name="fish">The fish to sell.</param>
        /// <param name="userId">The ID of the user to add the value of the fish to.</param>
        /// <returns></returns>
        public static async Task SellFishAsync(Fish fish, ulong userId)
        {
            var user = await GetOrCreateUserAsync(userId);

            user.Points += Fish.GetPayoutForFish(fish, user.FishExp);
            fish.Sold = true;

            using (var db = new KaguyaDb())
            {
                await db.UpdateAsync(fish);
                await db.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Allows a user to sell a collection of <see cref="Fish"/> en masse.
        /// This is typically used for selling off all fish of the same type at once.
        /// This action will add points to the user's account.
        /// </summary>
        /// <param name="fishCollection">The collection of fish to sell off.</param>
        /// <param name="userId">The ID of the user we are selling the fish for.</param>
        /// <returns></returns>
        public static async Task SellFishAsync(IEnumerable<Fish> fishCollection, ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                var user = await GetOrCreateUserAsync(userId);
                foreach (var fish in fishCollection)
                {
                    user.Points += Fish.GetPayoutForFish(fish, user.FishExp);
                    fish.Sold = true;

                    await db.UpdateAsync(fish);
                }

                await db.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Returns a List of Fish that belong to the user ID.
        /// </summary>
        /// <param name="fishType">The type of fish we are selling.</param>
        /// <param name="userId">The id of the user who we want to get all of the fish from.</param>
        /// <returns></returns>
        public static async Task<List<Fish>> GetFishForUserMatchingTypeAsync(FishType fishType, ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from f in db.Fish
                              where f.UserId == userId && f.FishType == fishType
                              select f).ToListAsync();
            }
        }

        /// <summary>
        /// Returns a List of Fish that belongs to the user ID that have not been sold. This only returns
        /// actual fish, not the BAIT_STOLEN event.
        /// </summary>
        /// <param name="userId">The ID of the user who we want to get all of the unsold fish from.</param>
        /// <returns></returns>
        public static async Task<List<Fish>> GetUnsoldFishForUserAsync(ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from f in db.Fish
                              where f.UserId == userId && !f.Sold && f.FishType != FishType.BAIT_STOLEN
                              select f).ToListAsync();
            }
        }

        /// <summary>
        /// Inserts the <see cref="IKaguyaUnique{T}"/> object into the database, assuming it doesn't already exist.
        /// If it exists, this will replace it via the PrimaryKey.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task InsertOrReplaceAsync<T>(T arg) where T : class, IKaguyaQueryable<T>, IKaguyaUnique<T>
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(arg);
            }
        }

        /// <summary>
        /// Inerts a new <see cref="IKaguyaQueryable{T}"/> object into the database. Do not use if wanting to
        /// update.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task InsertAsync<T>(T arg) where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(arg);
            }
        }

        /// <summary>
        /// Returns a <see cref="T"/> object that belongs to the user, but is not necessarily
        /// mapped to the user object directly. <see cref="T"/> refers to the object in the database that
        /// we want to retreive, for example, someone's fish or something else that belongs to them.
        /// </summary>
        /// <typeparam name="T">The <see cref="T"/> object that we are looking for. Must inherit from
        /// <see cref="IKaguyaQueryable{T}"/>, <see cref="IKaguyaUnique{T}"/> and <see cref="IUserSearchable{T}"/></typeparam>
        /// <param name="userId">The user whom we are retreiving the <see cref="T"/> for.</param>
        /// <returns></returns>
        public static async Task<T> GetFirstForUserAsync<T>(ulong userId) where T :
            class,
            IKaguyaQueryable<T>,
            IKaguyaUnique<T>,
            IUserSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>()
                              where t.UserId == userId
                              select t).FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// Finds all <see cref="T"/> objects that belong to the <see cref="userId"/>
        /// with the corresponding <see cref="userId"/>.
        /// </summary>
        /// <typeparam name="T">The type of object we want to find for the user.</typeparam>
        /// <param name="userId">The Id of the user.</param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllForUserAsync<T>(ulong userId) where T :
            class,
            IKaguyaQueryable<T>,
            IUserSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>()
                              where t.UserId == userId
                              select t).ToListAsync();
            }
        }

        public static async Task<List<T>> GetAllForUserAsync<T>(ulong userId, Expression<Func<T, bool>> predicate) where T :
            class, IKaguyaQueryable<T>, IUserSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>().Where(predicate)
                    where t.UserId == userId
                    select t).ToListAsync();
            }
        }

        /// <summary>
        /// Finds the first (or default) <see cref="IKaguyaUnique{T}"/> object in the database that belongs to this <see cref="serverId"/>
        /// </summary>
        /// <typeparam name="T">The <see cref="IServerSearchable{T}"/> that belongs to this <see cref="serverId"/></typeparam>
        /// <param name="serverId">The Id of the server.</param>
        /// <returns>The first element of type <see cref="IKaguyaUnique{T}"/> that belongs to this <see cref="serverId"/></returns>
        public static async Task<T> GetFirstForServerAsync<T>(ulong serverId) where T :
            class,
            IKaguyaQueryable<T>,
            IKaguyaUnique<T>,
            IServerSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>()
                              where t.ServerId == serverId
                              select t).FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> of objects that match the given <see cref="serverId"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="IServerSearchable{T}"/> that we want to get for this <see cref="serverId"/>.</typeparam>
        /// <param name="serverId">The Id of the server we are searching for.</param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllForServerAsync<T>(ulong serverId) where T :
            class,
            IKaguyaQueryable<T>,
            IServerSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>()
                              where t.ServerId == serverId
                              select t).ToListAsync();
            }
        }

        /// <summary>
        /// Deletes all of the <see cref="IServerSearchable{T}"/> objects from the database
        /// where the serverId matches the provided <see cref="serverId"/>
        /// </summary>
        /// <typeparam name="T">The <see cref="IServerSearchable{T}"/> to remove from the database.</typeparam>
        /// <param name="serverId">The id of the server to clear the objects from.</param>
        /// <returns></returns>
        public static async Task DeleteAllForServerAsync<T>(ulong serverId) where T :
            class, IKaguyaQueryable<T>, IServerSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                await (from t in db.GetTable<T>()
                       where t.ServerId == serverId
                       select t).DeleteAsync();
            }
        }


        /// <summary>
        /// Bulk copies (inserts) the <see cref="IEnumerable{T}"/> into the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task BulkCopy<T>(IEnumerable<T> args) where T :
            class,
            IKaguyaQueryable<T>
            // IKaguyaUnique<T> may be necessary, but we'll see.
        {
            using (var db = new KaguyaDb())
            {
                await Task.Run(() => { db.BulkCopy(args); });
            }
        }

        /// <summary>
        /// Deletes the <see cref="T"/> object from the database.
        /// </summary>
        /// <typeparam name="T">The type of object we are removing from the database.</typeparam>
        /// <param name="arg">The exact object that we are deleting from the database.</param>
        /// <returns></returns>
        public static async Task DeleteAsync<T>(T arg) where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(arg);
            }
        }

#if DEBUG
        public static async Task DeleteAllAsync<T>() where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                var records = await db.GetTable<T>().ToListAsync();

                if (records.Count > 0)
                {
                    foreach (var record in records)
                    {
                        await db.DeleteAsync(record);
                    }
                }

                await ConsoleLogger.LogAsync($"Deleted all records of type {typeof(T)}", LogLvl.WARN);
            }
        }
#endif

        /// <summary>
        /// Deletes all objects from the database that are specified in <see cref="args"/>
        /// </summary>
        /// <typeparam name="T">The type of object we are removing from the database.</typeparam>
        /// <param name="args">The <see cref="IEnumerable{T}"/> collection of objects to delete.</param>
        /// <returns></returns>
        public static async Task DeleteAsync<T>(IEnumerable<T> args) where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                foreach (var arg in args)
                {
                    await db.DeleteAsync(arg);
                }
            }
        }

        /// <summary>
        /// Returns ALL objects of type <see cref="T"/> that exist in the database.
        /// </summary>
        /// <typeparam name="T">The type of object to retreive ALL items of.</typeparam>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>() where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<T>().ToListAsync();
            }
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> that match the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="List{T}"/> to return.</typeparam>
        /// <param name="predicate">A condition that each returned object in the <see cref="List{T}"/> must match.</param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>(Expression<Func<T, bool>> predicate) where T :
            class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>().Where(predicate)
                              select t).ToListAsync();
            }
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> where the <see cref="userId"/> and <see cref="serverId"/> matches
        /// the provided values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetAllForServerAndUserAsync<T>(ulong userId, ulong serverId) where T :
            class, IKaguyaQueryable<T>, IUserSearchable<T>, IServerSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>()
                              where t.UserId == userId && t.ServerId == serverId
                              select t).ToListAsync();
            }
        }

        /// <summary>
        /// Returns the FirstOrDefault <see cref="T"/> that matches the provided <see cref="userId"/> and <see cref="serverId"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static async Task<T> GetForServerAndUserAsync<T>(ulong userId, ulong serverId) where T :
            class, IKaguyaQueryable<T>, IUserSearchable<T>, IServerSearchable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>()
                              where t.UserId == userId && t.ServerId == serverId
                              select t).FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// Updates the specified <see cref="IKaguyaUnique{T}"/> <see cref="arg"/> in the database. Item
        /// must have a primary key in order for this query to execute. 
        /// </summary>
        /// <typeparam name="T">The type of object we are updating</typeparam>
        /// <param name="arg">The object to update.</param>
        /// <returns></returns>
        public static async Task UpdateAsync<T>(T arg) where T : class, IKaguyaQueryable<T>, IKaguyaUnique<T>
        {
            using (var db = new KaguyaDb())
            {
                await db.UpdateAsync(arg);
            }
        }

        /// <summary>
        /// Updates the specified <see cref="IEnumerable{T}"/> <see cref="args"/> in the database.
        /// </summary>
        /// <typeparam name="T">The type of object whom's collection we are updating.</typeparam>
        /// <param name="args">The collection to update.</param>
        /// <returns></returns>
        public static async Task UpdateAsync<T>(IEnumerable<T> args) where T :
            class, IKaguyaQueryable<T>, IKaguyaUnique<T>
        {
            using (var db = new KaguyaDb())
            {
                foreach (var arg in args)
                {
                    await db.UpdateAsync(arg);
                }
            }
        }

        /// <summary>
        /// Executes the provided sql query and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">The sql to execute asynchronously.</param>
        /// <returns>The number of affected rows</returns>
        public static async Task<int> ExecuteSqlAsync(string sql)
        {
            using (var db = new KaguyaDb())
            {
                return await db.ExecuteAsync(sql);
            }
        }

        /// <summary>
        /// Returns the first or default value from the database of a type that matches this <see cref="Predicate{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IKaguyaQueryable{T}"/> object to return</typeparam>
        /// <param name="predicate">An expression that the returned object must match.
        /// <code>
        /// await GetFirstMatchAsync{User}(x => x.UserId == SomeId);
        /// </code></param>
        /// <returns></returns>
        public static async Task<T> GetFirstMatchAsync<T>(Expression<Func<T, bool>> predicate) where T :
            class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.GetTable<T>().Where(predicate)
                              select t).FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// Returns a <see cref="bool"/> that determines whether the provided <see cref="arg"/> exists in the database.
        /// If it does, this function will return true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg">The object that we are checking to see exists in the database.</param>
        /// <returns></returns>
        public static async Task<bool> ItemExists<T>(T arg) where T :
            class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<T>().AnyAsync(x => x.Equals(arg));
            }
        }

        /// <summary>
        /// Returns a <see cref="bool"/> that determines whether this <see cref="T"/> object exists in the database
        /// where the <see cref="predicate"/> is true.
        /// If it does, this function will return true.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<bool> ItemExists<T>(Expression<Func<T, bool>> predicate) where T :
            class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<T>().Where(predicate).AnyAsync();
            }
        }

        public static async Task<bool> ItemExists<T>(IEnumerable<T> args, Expression<Func<T, bool>> predicate) where T :
            class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<T>().Where(predicate).AnyAsync(x => args.Contains(x));
            }
        }

        /// <summary>
        /// Inserts the object into the database. If it already exists, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg">The object to insert, assuming it doesn't already exist.</param>
        /// <returns></returns>
        public static async Task InsertIfNotExistsAsync<T>(T arg) where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                if (await db.GetTable<T>().AnyAsync(x => x.Equals(arg)))
                {
                    throw new Exception("Item already exists in the database.");
                }
                await db.InsertAsync(arg);
            }
        }

        /// <summary>
        /// Returns the number of objects that exist of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type of object to return the number of occurances of.</typeparam>
        /// <returns></returns>
        public static async Task<int> GetCountAsync<T>() where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<T>().CountAsync();
            }
        }

        /// <summary>
        /// Returns the number of objects that exist of type <see cref="T"/> matching the given <see cref="Predicate{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static async Task<int> GetCountAsync<T>(Expression<Func<T, bool>> predicate)
            where T : class, IKaguyaQueryable<T>
        {
            using (var db = new KaguyaDb())
            {
                return await db.GetTable<T>().Where(predicate).CountAsync();
            }
        }

        /// <summary>
        /// Returns the most popular command of all time, with how many uses it has.
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string, int>> GetMostPopularCommandAsync()
        {
            var dic = new Dictionary<string, int>();
            using (var db = new KaguyaDb())
            {
                var command = await (from h in db.GetTable<CommandHistory>()
                                     group h by h.Command
                    into grp
                                     orderby grp.Count() descending
                                     select grp.First().Command).FirstOrDefaultAsync();

                var count = await (from c in db.GetTable<CommandHistory>()
                                   group c by c.Command
                    into grp
                                   orderby grp.Count() descending
                                   select grp.Count()).FirstOrDefaultAsync();

                dic.Add(command, count);
                return dic;
            }
        }

        /// <summary>
        /// Returns the total amount of points in circulation across all users.
        /// </summary>
        /// <returns></returns>
        public static int GetTotalCurrency()
        {
            using (var db = new KaguyaDb())
            {
                return Enumerable.Sum(db.Users, user => user.Points);
            }
        }
    }
}
