using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using LinqToDB.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class UserQueries
    {
        public static async Task<User> GetOrCreateUserAsync(ulong Id)
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

        /// <summary>
        /// Inserts a new <see cref="User"/> object into the database. If the user is already there,
        /// it will not be overwritten.
        /// </summary>
        /// <param name="user">The user to insert into the database.</param>
        /// <returns></returns>
        public static async Task InsertUserAsync(User user)
        {
            using (var db = new KaguyaDb())
            {
                if(!await db.Users.AnyAsync(x => x.Id == user.Id))
                    await db.InsertAsync(user);
            }
        }

        /// <summary>
        /// Bulk-inserts users into the database. All users that are bulk copied must NOT exist in the database already.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static async Task BulkInsertUsersAsync(List<User> users)
        {
            using (var db = new KaguyaDb())
            {
                if (!AnyUserExistsInDatabase(users))
                {
                    db.BulkCopy(users);
                    await ConsoleLogger.LogAsync($"{users.Count} new users have been added to the database.", LogLvl.INFO);
                }
                else
                {
                    throw new LinqToDBException("Failed to bulk copy users. This collection contains a user that is already in the database.");
                }
            }
        }

        /// <summary>
        /// Determines whether a <see cref="User"/> exists in the database or not based on their UserId.
        /// </summary>
        /// <param name="user">The user to check for. ID must not be 0.</param>
        /// <returns></returns>
        public static async Task<bool> UserExistsInDatabaseAsync(User user)
        {
            if (user.Id == 0)
                throw new ArgumentNullException(nameof(user.Id), "The Id parameter of the User must not be zero.");

            using (var db = new KaguyaDb())
            {
                return await db.Users.AnyAsync(x => x.Id == user.Id);
            }
        }

        /// <summary>
        /// Determines whether a <see cref="User"/> exists in the database or not based on their UserId.
        /// </summary>
        /// <param name="Id">The id of the user to check for.</param>
        /// <returns></returns>
        public static async Task<bool> UserExistsInDatabaseAsync(ulong Id)
        {
            if (Id == 0)
                throw new ArgumentNullException(nameof(Id), "The Id parameter must not be zero.");

            using (var db = new KaguyaDb())
            {
                return await db.Users.AnyAsync(x => x.Id == Id);
            }
        }

        /// <summary>
        /// Determines whether any <see cref="User"/> from the provided collection
        /// exists in the database, based on their Id.
        /// </summary>
        /// <param name="users">A collection of users. If any of the users in this collection are in the
        /// database, this function will return <code>true</code></param>
        /// <returns></returns>
        public static bool AnyUserExistsInDatabase(IEnumerable<User> users)
        {
            using (var db = new KaguyaDb())
            {
                return users.Any(x => db.Users.Any(y => y.Id == x.Id));
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
                await db.InsertAsync(ghObject);
            }
        }

        public static async Task RemoveGambleHistory(GambleHistory ghObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(ghObject);
            }
        }

        public static async Task<List<GambleHistory>> GetGambleHistoryAsync(ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from h in db.GambleHistories
                              where h.UserId == userId
                              select h).ToListAsync();
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

        /// <summary>
        /// Adds a new fish to the database.
        /// </summary>
        /// <param name="fish">The fish to add into the database.</param>
        /// <returns></returns>
        public static async Task AddFish(Fish fish)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(fish);
            }
        }

        /// <summary>
        /// Deletes a fish from the database. Don't use this if we're selling the fish.
        /// </summary>
        /// <param name="fish">The fish to delete from the database.</param>
        /// <returns></returns>
        public static async Task RemoveFish(Fish fish)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(fish);
            }
        }

        /// <summary>
        /// Sells a <see cref="Fish"/> to the "market", then adds the value of the fish to the
        /// <see cref="User"/>'s points balance. This action will add the points to the user's account.
        /// </summary>
        /// <param name="fish">The fish to sell.</param>
        /// <param name="user">The user to add the value of the fish to.</param>
        /// <returns></returns>
        public static async Task SellFishAsync(Fish fish, User user)
        {
            user.Points += Fish.GetPayoutForFish(fish);
            fish.Sold = true;

            using (var db = new KaguyaDb())
            {
                await db.UpdateAsync(fish);
                await db.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Sells a <see cref="Fish"/> to the "market", then adds the value of the fish to the
        /// <see cref="User"/> that matches this <see cref="userId"/> points balance.
        /// This action will add the points to the user's account.
        /// </summary>
        /// <param name="fish">The fish to sell.</param>
        /// <param name="userId">The id of the user to add the value of the fish to.</param>
        /// <returns></returns>
        public static async Task SellFishAsync(Fish fish, ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                User user = await GetOrCreateUserAsync(userId);

                user.Points += Fish.GetPayoutForFish(fish);
                fish.Sold = true;

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
        /// <param name="taxRate">The rate at which the fish is taxed. (0.05 would be a 5% fee)</param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task SellFishAsync(IEnumerable<Fish> fishCollection, User user)
        {
            using (var db = new KaguyaDb())
            {
                foreach (var fish in fishCollection)
                {
                    user.Points += Fish.GetPayoutForFish(fish);
                    fish.Sold = true;

                    await db.UpdateAsync(fish);
                }

                await db.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Allows a user to sell a collection of <see cref="Fish"/> en masse.
        /// This is typically used for selling off all fish of the same type at once.
        /// This action will add points to the user's account.
        /// </summary>
        /// <param name="fishCollection">The collection of fish to sell off.</param>
        /// <param name="taxRate">The rate at which the fish is taxed. (0.05 would be a 5% fee)</param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task SellFishAsync(IEnumerable<Fish> fishCollection, ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                var user = await GetOrCreateUserAsync(userId);
                foreach (var fish in fishCollection)
                {
                    user.Points += Fish.GetPayoutForFish(fish);
                    fish.Sold = true;

                    await db.UpdateAsync(fish);
                }

                await db.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Returns a List of Fish that belong to the user.
        /// </summary>
        /// <param name="user">The user who we want to get all of the fish from.</param>
        /// <returns></returns>
        public static async Task<List<Fish>> GetFishForUserAsync(User user)
        {
            using (var db = new KaguyaDb())
            {
                return await (from f in db.Fish
                    where f.UserId == user.Id
                    select f).ToListAsync();
            }
        }

        /// <summary>
        /// Returns a List of Fish that belong to the user ID.
        /// </summary>
        /// <param name="user">The user who we want to get all of the fish from.</param>
        /// <returns></returns>
        public static async Task<List<Fish>> GetFishForUserAsync(ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from f in db.Fish
                    where f.UserId == userId
                    select f).ToListAsync();
            }
        }

        /// <summary>
        /// Returns a List of Fish that belong to the user ID.
        /// </summary>
        /// <param name="user">The user who we want to get all of the fish from.</param>
        /// <returns></returns>
        public static async Task<List<Fish>> GetFishForUserAsync(FishType fishType, ulong userId)
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
        /// <param name="user">The user who we want to get all of the unsold fish from.</param>
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