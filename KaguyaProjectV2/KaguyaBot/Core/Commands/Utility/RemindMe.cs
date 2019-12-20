using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class RemindMe : ModuleBase<ShardedCommandContext>
    {
        [UtilityCommand]
        [Command("RemindMe")]
        [Summary("Reminds you to do something after a certain amount of time has passed. Specify " +
                 "the time using this format (order doesn't matter): `0d0h0m0s`")]
        [Remarks("<time> <text>\n2h35m Walk the dog.\n18d12h30m Watch the game!")]
        public async Task Remind(string timeString, [Remainder]string text)
        {
            var time = RegexTimeParser.ParseToTimespan(timeString);
            KaguyaEmbedBuilder embed;

            text = Regex.Replace(text, "[mM][yY]", "your");
            text = text.ToLower();

            if (time.TotalSeconds < 10)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = "You must set a reminder for at least 10 seconds from now."
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
                return;
            }

            var reminder = new Reminder
            {
                UserId = Context.User.Id,
                Expiration = DateTime.Now.AddSeconds(time.TotalSeconds).ToOADate(),
                Text = text,
                HasTriggered = false
            };

            await UtilityQueries.AddReminderAsync(reminder);

            embed = new KaguyaEmbedBuilder
            {
                Description = $"Okay! I'll remind you in `{time.Humanize()}` to `{text}`"
            };
            await ReplyAsync(embed: embed.Build());
        }
    }
}
