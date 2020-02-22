using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class NSFWImageHandler
    {
        public static Task Initialize()
        {
            Timer timer = new Timer(7200000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, a) =>
            {
                foreach (var user in await DatabaseQueries.GetAllAsync<User>(x => x.TotalNSFWImages < 12))
                {
                    user.TotalNSFWImages += 1;
                    await DatabaseQueries.InsertOrReplaceAsync(user);
                }
            };
            return Task.CompletedTask;
        }
    }
}
