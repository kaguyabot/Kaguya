using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class MyReminders : KaguyaBase
    {
        [UtilityCommand]
        [Command("MyReminders")]
        [Alias("reminders", "mr")]
        [Summary("Allows you to view your current reminders. Cancel them by clicking the " +
                 "corresponding reaction.")]
        [Remarks("")]
        public async Task Command()
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var reminders = user.Reminders.ToList();
            var embed = new KaguyaEmbedBuilder();

            int i = 0;
            foreach (var reminder in reminders)
            {
                i++;
                var field = new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = $"#{i}",
                    Value = $"Reminder: `{reminder.Text}`\n" +
                            $"Notification: `{DateTime.FromOADate(reminder.Expiration).Humanize(false)}`"
                };

                embed.AddField(field);
            }

            int j = 0;
            var data = new ReactionCallbackData("", embed.Build());
            foreach (var reminder in reminders)
            {
                var callback = new ReactionCallbackItem(GlobalProperties.EmojisOneThroughNine()[j], 
                async (c, r) =>
                {
                    await DatabaseQueries.DeleteAsync(reminder);
                    await ReplyAsync($"{Context.User.Mention} Successfully deleted reminder #{j}.");
                });

                j++;
            }
        }
    }
}
