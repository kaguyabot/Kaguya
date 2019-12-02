using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using NodaTime;
using System;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using NodaTime.TimeZones;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SetTimezone : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("TimeZone")]
        [Alias("tz")]
        [Summary("Sets the timezone for the server. The default timezone is `American/New_York`. " +
                 "To properly setup the timezone, [find your timezone in this database.]" +
                 "(https://en.m.wikipedia.org/wiki/List_of_tz_database_time_zones) " +
                 "Your timezone can be found under the `TZ database name` column. Configuring your timezone " +
                 "allows for consistent reporting of scheduled tasks, such as reminders and mutes for users. " +
                 "**The timezone is case sensitive.**")]
        [Remarks("<timezone>")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetZone(string tzInput)
        {
            var server = ServerQueries.GetServer(Context.Guild.Id);
            DateTimeZone timezone;

            try
            {
                timezone = DateTimeZoneProviders.Tzdb[tzInput];
            }
            catch (DateTimeZoneNotFoundException)
            {
                throw new DateTimeZoneNotFoundException($"The timezone `{tzInput}` is invalid.\n" +
                                                        $"Please review [this list of timezones]" +
                                                        $"(https://en.m.wikipedia.org/wiki/List_of_tz_database_time_zones) to find " +
                                                        $"the one that matches your preferred timezone.\n\n" +
                                                        $"Your timezone will be under the `TZ database name` column. " +
                                                        $"Remember, timezones are case sensitive.");
            }

            server.Timezone = tzInput;
            ServerQueries.UpdateServer(server);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully updated your server's timezone to `{server.Timezone}`"
            };
            await ReplyAsync(embed: embed.Build());
        }
    }
}