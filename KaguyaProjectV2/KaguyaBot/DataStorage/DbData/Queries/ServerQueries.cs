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

        public static void UpdateFilteredPhrases(FilteredPhrase fpObject)
        {
            using (var db = new DataConnection())
            {
                db.Insert(fpObject);
            }
        }

        public static void UpdateBlacklistedChannels(BlackListedChannel blObject)
        {
            using (var db = new DataConnection())
            {
                db.Insert(blObject);
            }
        }
    }
}
