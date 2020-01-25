using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class NSFWImageHandler
    {
        public static Task Start()
        {
            Timer timer = new Timer(7200000);
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Elapsed += async (s, a) =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var usersToUpdate = new List<User>();
                foreach(var user in await DatabaseQueries.GetAllAsync<User>(x => x.TotalNSFWImages < 12))
                {
                    user.TotalNSFWImages += 1;
                    await DatabaseQueries.InsertOrReplaceAsync(user);
                }
            };
            return Task.CompletedTask;
        }
    }
}
