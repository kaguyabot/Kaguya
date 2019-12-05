using System.Collections.Generic;
using System.Linq;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries
{
    public static class UserQueries
    {
        public static IEnumerable<User> GetAllUsers()
        {
            using var db = new KaguyaDb();
            return db.GetTable<User>().ToList();
        }

        public static User GetUser(ulong Id)
        {
            using var db = new KaguyaDb();
            User user = new User();

            if (db.GetTable<User>().FirstOrDefault(x => x.Id == Id) == null)
            {
                user.Id = Id;
                db.Insert(user, "kaguyauser");
            }

            return db.GetTable<User>().FirstOrDefault(x => x.Id == Id);
        }

        public static void UpdateUser(User user)
        {
            using var db = new KaguyaDb();
            db.InsertOrReplace<User>(user);
        }

        public static void UpdateUsers(List<User> users)
        {
            using var db = new KaguyaDb();
            foreach (var user in users)
            {
                db.InsertOrReplace<User>(user);
            }
        }
        
        public static void AddCommandHistory(CommandHistory chObject)
        {
            using var db = new KaguyaDb();
            db.Insert(chObject);
        }

        public static void RemoveCommandHistory(CommandHistory chObject)
        {
            using var db = new KaguyaDb();
            db.Delete(chObject);
        }

        public static void AddGambleHistory(GambleHistory ghObject)
        {
            using var db = new KaguyaDb();
            db.Delete(ghObject);
        }

        public static void RemoveGambleHistory(GambleHistory ghObject)
        {
            using var db = new KaguyaDb();
            db.Delete(ghObject);
        }
    }
}