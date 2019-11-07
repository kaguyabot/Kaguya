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
            using (var db = new DataConnection())
            {
                return db.GetTable<Server>().ToList();
            }
        }

        public static Server GetServer(ulong Id)
        {
            using (var db = new DataConnection())
            {
                Server server = new Server();

                if (db.GetTable<Server>().Where(x => x.Id == Id).FirstOrDefault() == null)
                {
                    server.Id = Id;
                    db.Insert(server, "kaguyaserver");
                }

                return db.GetTable<Server>().Where(x => x.Id == Id).FirstOrDefault();
            }
        }

        public static void UpdateServer(Server server)
        {
            using (var db = new DataConnection())
            {
                db.InsertOrReplace<Server>(server);
            }
        }

        public static List<FilteredPhrase> GetAllFilteredPhrases()
        {
            using (var db = new DataConnection())
            {
                return db.GetTable<FilteredPhrase>().ToList();
            }
        }

        /// <summary>
        /// Adds a FilteredPhrase object to the database. Duplicates are skipped automatically.
        /// </summary>
        /// <param name="fpObject">FilteredPhrase object to add.</param>
        public static void AddFilteredPhrase(FilteredPhrase fpObject)
        {
            using (var db = new DataConnection())
            {
                db.Insert(fpObject);
            }
        }

        /// <summary>
        /// Deletes a filtered phrase object from the database.
        /// </summary>
        /// <param name="fpObject">FilteredPhrase object to remove.</param>
        public static void DeleteFilteredPhrase(FilteredPhrase fpObject)
        {
            using (var db = new DataConnection())
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
            using (var db = new DataConnection())
            {
                db.Insert(blObject);
            }
        }
    }
}
