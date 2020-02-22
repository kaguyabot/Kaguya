using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class NSFWImageHandler
    {
        public static Task Initialize()
        {
            Timer timer = new Timer(900000); // 15 minutes.
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (s, a) =>
            {
                foreach (var user in await DatabaseQueries.GetAllAsync<User>(x => x.TotalNSFWImages < 12))
                {
                    user.TotalNSFWImages += 1;
                    await DatabaseQueries.InsertOrReplaceAsync(user);
                }

                await ConsoleLogger.LogAsync($"Users have been given +1 nsfw images.", LogLvl.INFO);
            };
            return Task.CompletedTask;
        }
    }
}
