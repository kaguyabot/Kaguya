using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Threading.Tasks;
using System.Timers;

#pragma warning disable 1998

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class RemindService
    {
        public static async Task Start()
        {
            Timer timer = new Timer(7500);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += async (sender, args) =>
            {
                var unTriggeredReminders = await DatabaseQueries.GetAllAsync<Reminder>(r => r.HasTriggered == false && r.Expiration < DateTime.Now.ToOADate());
                if (unTriggeredReminders == null)
                {
                    return;
                }

                foreach (var reminder in unTriggeredReminders)
                {
                    var user = ConfigProperties.Client.GetUser(reminder.UserId);
                    var embed = new KaguyaEmbedBuilder
                    {
                        Title = "Kaguya Reminder",
                        Description = $"`{reminder.Text}`"
                    };
                    await user.SendMessageAsync(embed: embed.Build());
                    await DatabaseQueries.DeleteAsync(reminder);

                    await ConsoleLogger.LogAsync($"User {user} has been sent a reminder to \"{reminder.Text}\"", LogLvl.INFO);
                }
            };
        }
    }
}
