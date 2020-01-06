using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AntiraidCommand : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("AntiRaid")]
        [Summary("Enables the antiraid service for this server. When enabled, the antiraid will either " +
                 "`mute`, `kick`, `ban`, or `shadowban` all members who join within the specified time frame.\n\n" +
                 "The `users` parameter determines how many users are allowed to join within the amount of " +
                 "seconds specified with the `seconds` parameter before it is classified as a raid. The `users` " +
                 "value must be at least `3` and the `seconds` value must be at least `5`. " +
                 "The `action` parameter refers to one of the aforementioned actions.\n\n" +
                 "Use without a parameter to disable the service.")]
        [Remarks("\n<users> <seconds> <action>\n5 20 shadowban")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Command(int users = 0, int seconds = 0, string action = null)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var antiraid = server.AntiRaid?.FirstOrDefault();

            if (users == 0 && seconds == 0 && action == null)
            {
                if (antiraid != null)
                {
                    await DatabaseQueries.DeleteAllForServerAsync<AntiRaidConfig>(server.ServerId);
                    await Context.Channel.SendBasicSuccessEmbedAsync("Successfully disabled this server's antiraid protection.");
                    return;
                }

                await Context.Channel.SendBasicErrorEmbedAsync("This server has not setup the antiraid service, therefore " +
                                                    "there is nothing to disable.");
                return;
            }

            if (users < 3 || seconds < 5 && action != null)
            {
                if (users < 3)
                    throw new ArgumentOutOfRangeException(nameof(users), "There must be at least `3` users to action before " +
                                                                   "it is classified as a raid.");
                if (seconds < 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(seconds), "The seconds parameter must be at least `5`.");
                }
            }

            if (users > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(users), "The users count must not be greater than `200`.");
            }

            if (seconds > 999)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds),
                    "The `seconds` duration must not be greater than `999`.");
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "The action must not be null and must be either " +
                                                          "`mute`, `kick`, `shadowban`, or `ban`.");
            }

            string[] actions = { "mute", "kick", "shadowban", "ban" };
            if (!actions.Any(x => x.Equals(action.ToLower())))
            {
                throw new ArgumentOutOfRangeException(nameof(action),
                    "The action must be either `mute`, `kick`, `shadowban`, " +
                    "or `ban`.");
            }

            var ar = new AntiRaidConfig
            {
                ServerId = server.ServerId,
                Users = users,
                Seconds = seconds,
                Action = action.ToLower(),
                Server = server
            };

            await Context.Channel.SendBasicSuccessEmbedAsync(
                $"Successfully enabled the antiraid service for `{Context.Guild.Name}`.\n\n" +
                $"I will `{action.ToUpper()}` anyone part of a raid. A raid is now defined as " +
                $"`{users.ToWords()}` users joining within `{seconds.ToWords()}` seconds " +
                $"of each other.");


            if (antiraid != null)
            {
                await DatabaseQueries.InsertAsync(ar);
                return;
            }

            await DatabaseQueries.InsertOrReplaceAsync(ar);
        }
    }
}
