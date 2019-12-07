using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogLevel = KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage.LogLevel;
using Timer = System.Timers.Timer;

namespace KaguyaProjectV2.KaguyaBot.Core.Global
{
    public static class MemoryStorage
    {
        public static HashSet<Server> Servers { get; set; } = new HashSet<Server>();
        public static HashSet<User> Users { get; set; } = new HashSet<User>();
        public static HashSet<SupporterKey> SupporterKeys { get; set; } = new HashSet<SupporterKey>();

        public static void Populate()
        {
            using (var db = new KaguyaDb())
            {
                Servers = db.GetTable<Server>().ToHashSet();
                Users = db.GetTable<User>().ToHashSet();
                SupporterKeys = db.GetTable<SupporterKey>().ToHashSet();
            }
        }

        /// <summary>
        /// Starts the timer that updates the database.
        /// </summary>
        public static async Task Start()
        {
            Timer timer = new Timer(30000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (sender, e) =>
            {
                using (var db = new KaguyaDb())
                {
                    var servers = db.GetTable<Server>();
                    foreach (var server in servers)
                    {
                        if (!Equals(Servers.Select(x => x.Id == server.Id), server))
                        {
                            await ConsoleLogger.Log($"Server [ID: {server.Id}] updated.", LogLevel.TRACE);
                            await db.UpdateAsync(server);
                        }
                    }

                    foreach (var user in db.GetTable<User>())
                    {
                        if (!Equals(Users.Select(x => x.Id == user.Id), user))
                        {
                            await ConsoleLogger.Log($"User [ID: {user.Id}] updated.", LogLevel.TRACE);
                            await db.UpdateAsync(user);
                        }
                    }

                    foreach (var key in db.GetTable<SupporterKey>())
                    {
                        if (!Equals(SupporterKeys.Select(x => x.Key == key.Key), key))
                        {
                            await ConsoleLogger.Log($"Server [ID: {key.Key}] updated.", LogLevel.TRACE);
                            await db.UpdateAsync(key);
                        }
                    }
                }
            };
        }
    }
}
