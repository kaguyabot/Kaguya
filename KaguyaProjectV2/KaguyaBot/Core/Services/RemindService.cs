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
using Discord.Net;

#pragma warning disable 1998

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class RemindService
    {
        public static async Task Initialize()
        {
            Timer timer = new Timer(5000);
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
                    try
                    {
                        if (user != null)
                        {
                            await user.SendMessageAsync(embed: embed.Build());
                        }
                        else
                        {
                            await ConsoleLogger.LogAsync($"Attempted to send user {reminder.UserId} a reminder to " +
                                                         $"\"{reminder.Text}\", but the SocketUser object was not found.",
                                LogLvl.WARN);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        await ConsoleLogger.LogAsync(
                            "Attempted to send a reminder to a user, but the user was not found. " +
                            "The reminder has been removed from the database.", LogLvl.WARN);
                    }
                    catch (HttpException e)
                    {
                        await ConsoleLogger.LogAsync($"Attempted to send user {reminder.UserId} a reminder to " +
                                                     $"\"{reminder.Text}\", but the user doesn't accept direct messages " +
                                                     $"or another error from Discord told us that we couldn't send them this message.",
                            LogLvl.WARN);
                    }
                    catch (Exception e)
                    {
                        await ConsoleLogger.LogAsync(e);
                    }

                    await DatabaseQueries.DeleteAsync(reminder);
                    await ConsoleLogger.LogAsync($"User {user} has been sent a reminder to \"{reminder.Text}\"", LogLvl.INFO);
                }
            };
        }
    }
}
