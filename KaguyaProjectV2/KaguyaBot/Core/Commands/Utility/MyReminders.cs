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
using System.Text;
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
            var reminders = user.Reminders.Where(x => x.Expiration > DateTime.Now.ToOADate()).ToArray();
            var embed = new KaguyaEmbedBuilder();

            int i = 0;

            if(!(reminders.Length == 0))
            {
                foreach (var reminder in reminders)
                {
                    i++;

                    var expirationStr = DateTime.FromOADate(reminder.Expiration).Humanize(false);

                    var fSb = new StringBuilder();
                    fSb.AppendLine($"Reminder: `{reminder.Text}`");
                    fSb.AppendLine($"Expires: `{expirationStr}`");
                    
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"#{i}",
                        Value = fSb.ToString()
                    };

                    embed.AddField(field);
                }

                embed.Footer = new EmbedFooterBuilder
                {
                    Text = "To delete a reminder, click the corresponding reaction."
                };
            }

            else
            {
                var field = new EmbedFieldBuilder
                {
                    Name = "No reminders active",
                    Value = "You currently don't have any active reminders."
                };

                embed.AddField(field);
            }

            int j = 0;
            var data = new ReactionCallbackData("", embed.Build());
            foreach (var reminder in reminders)
            {
                data.AddCallBack(GlobalProperties.EmojisOneThroughNine()[j], 
                async (c, r) =>
                {
                    await DatabaseQueries.DeleteAsync(reminder);
                    await ReplyAsync($"{Context.User.Mention} Successfully deleted reminder #{j}.");
                });

                j++;
            }

            await InlineReactionReplyAsync(data);
        }
    }
}
