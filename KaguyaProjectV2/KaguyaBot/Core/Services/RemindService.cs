using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using System.Timers;
using LogLevel = KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage.LogLevel;
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
                var unTriggeredReminders = await UtilityQueries.GetAllExpiredRemindersAsync(false);
                if (unTriggeredReminders == null)
                {
                    return;
                }

                foreach (var reminder in unTriggeredReminders)
                {
                    var user = ConfigProperties.client.GetUser(reminder.UserId);
                    var embed = new KaguyaEmbedBuilder
                    {
                        Title = "Kaguya Reminder",
                        Description = $"`{reminder.Text}`"
                    };
                    await user.SendMessageAsync(embed: embed.Build());
                    await UtilityQueries.DeleteReminderAsync(reminder);

                    await ConsoleLogger.Log($"User {user} has been sent a reminder to \"{reminder.Text}\"", LogLevel.INFO);
                }
            };
        }
    }
}
