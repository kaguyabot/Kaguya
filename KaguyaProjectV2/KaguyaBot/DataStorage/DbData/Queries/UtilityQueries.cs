using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
// ReSharper disable UseAwaitUsing

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public class UtilityQueries
    {
        public static List<SupporterKey> GetAllKeys()
        {
            using var db = new KaguyaDb();
            return db.GetTable<SupporterKey>().ToList();
        }

        public static void AddKey(SupporterKey key)
        {
            using var db = new KaguyaDb();
            db.Insert(key);
        }

        /// <summary>
        /// Should be used for inserting a very large amount of keys into the database.
        /// </summary>
        /// <param name="keys"></param>
        public static async void AddKeys(List<SupporterKey> keys)
        {
            using var db = new KaguyaDb();
            foreach (var element in keys)
            {
                await db.InsertAsync(element);
            }
        }

        /// <summary>
        /// Takes an existing supporter key and updates it.
        /// </summary>
        public static void UpdateKey(SupporterKey oldKey, SupporterKey newKey)
        {
            using var db = new KaguyaDb();
            db.Delete(oldKey);
            db.Insert(newKey);
        }

        public static void DeleteKey(SupporterKey key)
        {
            using var db = new KaguyaDb();
            db.Delete(key);
        }

        public static IEnumerable<ServerSpecificExp> GetAllExp()
        {
            using var db = new KaguyaDb();
            return db.GetTable<ServerSpecificExp>();
        }

        public static IEnumerable<ServerSpecificExp> GetAllExpForServer(Server server)
        {
            using var db = new KaguyaDb();
            return db.GetTable<ServerSpecificExp>().Where(x => x.ServerId == server.Id).ToList();
        }
    }
}
