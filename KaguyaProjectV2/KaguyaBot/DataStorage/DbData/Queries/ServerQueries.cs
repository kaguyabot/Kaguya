using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Generic;
using System.Linq;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class ServerQueries
    {
        public static IEnumerable<Server> GetAllServers()
        {
            using (var db = new KaguyaDb())
            {
                return db.GetTable<Server>().ToList();
            }
        }

        public static Server GetServer(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                Server server = db.GetTable<Server>().Where(x => x.Id == Id).FirstOrDefault();

                if (server == null)
                {
                    server = new Server();
                    server.Id = Id;
                    db.Insert(server, "kaguyaserver");
                }

                return db.GetTable<Server>().Where(x => x.Id == Id).FirstOrDefault();
            }
        }

        public static void UpdateServer(Server server)
        {
            using (var db = new KaguyaDb())
            {
                db.InsertOrReplace<Server>(server);
            }
        }

        public static List<FilteredPhrase> GetAllFilteredPhrases()
        {
            using (var db = new KaguyaDb())
            {
                return db.GetTable<FilteredPhrase>().ToList();
            }
        }
        public static List<FilteredPhrase> GetAllFilteredPhrasesForServer(ulong Id)
        {
            using (var db = new KaguyaDb())
            {
                return db.GetTable<FilteredPhrase>().Where(x => x.ServerId == Id).ToList();
            }
        }

        /// <summary>
        /// Adds a FilteredPhrase object to the database. Duplicates are skipped automatically.
        /// </summary>
        /// <param name="fpObject">FilteredPhrase object to add.</param>
        public static void AddFilteredPhrase(FilteredPhrase fpObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Insert(fpObject);
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
        public static void UpdateBlacklistedChannels(BlackListedChannel blObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Insert(blObject);
            }
        }

        public static void AddAutoAssignedRole(AutoAssignedRole arObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Insert(arObject);
            }
        }

        public static void RemoveAutoAssignedRole(AutoAssignedRole arObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Delete(arObject);
            }
        }

        public static void AddMutedUser(MutedUser muObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Insert(muObject);
            }
        }

        public static void RemoveMutedUser(MutedUser muObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Delete(muObject);
            }
        }

        public static void AddWarnAction(WarnAction waObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Insert(waObject);
            }
        }

        public static void RemoveWarnAction(WarnAction waObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Delete(waObject);
            }
        }

        public static void AddWarnedUser(WarnedUser wuObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Insert(wuObject);
            }
        }

        public static void RemoveWarnedUser(WarnedUser wuObject)
        {
            using (var db = new KaguyaDb())
            {
                db.Delete(wuObject);
            }
        }

        public static List<WarnedUser> GetWarnedUser(ulong serverId, ulong userId)
        {
            using (var db = new KaguyaDb())
            {
                return db.GetTable<WarnedUser>().Where(x => x.ServerId == serverId && x.UserId == userId).ToList();
            }
        }
    }
}
