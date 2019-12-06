using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using LinqToDB.SqlQuery;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class ServerQueries
    {
        public static async Task<Server> GetServer(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                return await db.Servers
                    .LoadWith(x=>x.MutedUsers)
                    .LoadWith(x=>x.FilteredPhrases)
                    .LoadWith(x=>x.WarnedUsers)
                    .LoadWith(x=>x.WarnActions)
                    .LoadWith(x=>x.MutedUsers)
                    .LoadWith(x=>x.ServerExp)
                    .Where(s => s.Id == Id).FirstAsync();
            }
        }

        public static async Task UpdateServer(Server server)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(server);
            }
        }

        public static void UpdateServers(IEnumerable<Server> servers)
        {
            using (var db = new KaguyaDb())
            {
                var options = new BulkCopyOptions
                {
                    BulkCopyType = BulkCopyType.ProviderSpecific
                };
                db.BulkCopy(options, servers);
            }
        }

        public static List<FilteredPhrase> GetAllFilteredPhrases()
        {
            using (var db = new KaguyaDb())
            {
                return db.GetTable<FilteredPhrase>().ToList();
            }
        }

        public static async Task<List<FilteredPhrase>> GetAllFilteredPhrasesForServer(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                return await (from f in db.FilteredPhrases
                    where f.ServerId == Id
                    select f).ToListAsync();
            }
        }

        /// <summary>
        /// Adds a FilteredPhrase object to the database. Duplicates are skipped automatically.
        /// </summary>
        /// <param name="fpObject">FilteredPhrase object to add.</param>
        public static async Task AddFilteredPhrase(FilteredPhrase fpObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(fpObject);
            }
        }

        /// <summary>
        /// Removes a filtered phrase object from the database.
        /// </summary>
        /// <param name="fpObject">FilteredPhrase object to remove.</param>
        public static void RemoveFilteredPhrase(FilteredPhrase fpObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Delete(fpObject);
            }
        }

        /// <summary>
        /// Adds a blacklisted channel object to the database.
        /// </summary>
        /// <param name="blObject">The BlackListedChannl object to add.</param>
        public static async Task AddBlacklistedChannel(BlackListedChannel blObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertOrReplaceAsync(blObject);
            }
        }

        public static async Task AddAutoAssignedRole(AutoAssignedRole arObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(arObject);
            }
        }

        public static async Task RemoveAutoAssignedRole(AutoAssignedRole arObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(arObject);
            }
        }

        public static async Task<IEnumerable<MutedUser>> GetCurrentlyMutedUsers()
        {
            using (var db = new KaguyaDb())
            {
                return await (from m in db.MutedUsers
                    where m.ExpiresAt > DateTime.Now.ToOADate()
                    select m).ToListAsync();
            }
        }

        public static async Task AddMutedUser(MutedUser muObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(muObject);
            }
        }

        public static async Task RemoveMutedUser(MutedUser muObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(muObject);
            }
        }

        public static async Task<List<MutedUser>> GetMutedUsersForServer(ulong serverId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from m in db.MutedUsers
                    where m.ServerId == serverId
                    select m).ToListAsync();
            }
        }
        /// <summary>
        /// Returns one MutedUser object for a specific individual in a specific guild.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static async Task<MutedUser> GetSpecificMutedUser(ulong userId, ulong serverId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from m in db.MutedUsers
                    where m.ServerId == serverId && m.UserId == userId
                    select m).FirstAsync();
            }
        }

        /// <summary>
        /// Replaces an existing muted user object with an updated MutedUser object.
        /// </summary>
        /// <param name="muObject">The object you want to send to the database as a replacement.</param>
        /// <returns></returns>
        public static async Task ReplaceMutedUser(MutedUser muObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.UpdateAsync(from m in db.MutedUsers
                    where m.ServerId == muObject.ServerId &&
                          m.UserId == muObject.UserId
                    select m);
            }
        }

        public static async Task AddWarnAction(WarnAction waObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(waObject);
            }
        }

        public static async Task RemoveWarnAction(WarnAction waObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(waObject);
            }
        }

        public static async Task AddWarnedUser(WarnedUser wuObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(wuObject);
            }
        }

        public static async Task RemoveWarnedUser(WarnedUser wuObject)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(wuObject);
            }
        }

        public static async Task<List<WarnedUser>> GetWarnedUser(ulong serverId, ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from w in db.WarnedUsers
                    where w.ServerId == serverId && w.UserId == userId
                    select w).ToListAsync();
            }
        }

        public static async Task AddTwitchChannel(TwitchChannel tcObj)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(tcObj);
            }
        }

        public static async Task RemoveTwitchChannel(TwitchChannel tcObj)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(tcObj);
            }
        }

        public static async Task<List<TwitchChannel>> GetTwitchChannelsForServer(ulong serverId)
        {
            using (var db = new KaguyaDb())
            {
                return await (from t in db.TwitchChannels
                    where t.ServerId == serverId
                    select t).ToListAsync();
            }
        }

        public static async Task AddServerSpecificExpForUser(ServerExp expObj)
        {
            using (var db = new KaguyaDb())
            {
                await db.InsertAsync(expObj);
            }
        }

        /// <summary>
        /// Replaces the old EXP object with a new, updated one. This
        /// will remove and replace based on the userId and serverId
        /// properties of each object. This is used specifically for
        /// the server-specific EXP table. This does not affect global EXP.
        /// </summary>
        /// <param name="oldExpObj">The object to be replaced.</param>
        /// <param name="newExpObj">The object to insert, the updated version of oldExpObj.</param>
        /// <returns></returns>
        public static async Task ReplaceServerSpecificExpForUser(ServerExp oldExpObj, ServerExp newExpObj)
        {
            using (var db = new KaguyaDb())
            {
                if (oldExpObj == null)
                {
                    await db.InsertAsync(newExpObj);
                    return;
                }

                if (oldExpObj.ServerId == newExpObj.ServerId &&
                    oldExpObj.UserId == newExpObj.UserId)
                {
                    var selection =
                        from c in db.ServerExp
                        from p in db.Users.InnerJoin(pr => pr.Id == c.UserId)
                        select c;

                    await db.InsertAsync(newExpObj);
                    return;
                }
                throw new ArgumentException("The userId and serverId properties from " +
                                            "the parameters did not match each other's. " +
                                            // ReSharper disable once NotResolvedInText
                                            "Did you mix up the users?", "oldExpObj, newExpObj");
            }
        }

        public static async Task RemoveServerSpecificExpForUser(ServerExp expObj)
        {
            using (var db = new KaguyaDb())
            {
                await db.DeleteAsync(expObj);
            }
        }
    }
}
