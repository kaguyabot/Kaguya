using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord.Net;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class AntiRaidService
    {
        public static async Task Initialize() => await Task.Run(() =>
        {
            DiscordShardedClient client = ConfigProperties.Client;
            client.UserJoined += async u =>
            {
                SocketGuild guild = u.Guild;
                Server server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);

                // The server hasn't configured an AntiRaid...
                if (server.AntiRaid == null)
                    return;

                AntiRaidConfig ar = server.AntiRaid;

                if (!CachedAntiraidData.ExistingData.Any(x => x.ServerId == server.ServerId))
                {
                    var newSt = new AntiraidData
                    {
                        ServerId = server.ServerId,
                        UserIds = new HashSet<ulong>
                        {
                            u.Id
                        }
                    };

                    CachedAntiraidData.AddToCache(newSt);
                }
                else
                {
                    var newIds = new HashSet<ulong>();
                    HashSet<ulong> existingIds = CachedAntiraidData.ExistingData.First(x => x.ServerId == server.ServerId).UserIds;

                    foreach (ulong id in existingIds)
                        newIds.Add(id);

                    newIds.Add(u.Id);
                    CachedAntiraidData.ReplaceData(new AntiraidData
                    {
                        ServerId = server.ServerId,
                        UserIds = newIds
                    });
                }

                var timer = new Timer(ar.Seconds * 1000);
                timer.Enabled = true;
                timer.AutoReset = false;
                timer.Elapsed += async (sender, args) =>
                {
                    AntiraidData existingObj = CachedAntiraidData.ExistingData.FirstOrDefault(x => x.ServerId == server.ServerId);

                    if (existingObj == null)
                        return;

                    if (existingObj.UserIds.Count >= ar.Users)
                        await ActionUsers(server, existingObj.UserIds, server.ServerId, ar.Action);
                };
            };
        });

        private static async Task ActionUsers(Server server, IEnumerable<ulong> userIds, ulong guildId, string action)
        {
            SocketGuild guild = ConfigProperties.Client.GetGuild(guildId);
            var guildUsers = new List<SocketGuildUser>();

            foreach (ulong userId in userIds)
            {
                SocketGuildUser guildUser = guild.GetUser(userId);

                if (guildUser != null)
                    guildUsers.Add(guildUser);
            }

            if (guildUsers.Count == 0)
            {
                await ConsoleLogger.LogAsync($"The antiraid service was triggered in guild: {guild.Id} " +
                                             "but no guild users were found from the provided set of IDs!", LogLvl.WARN);

                return;
            }

            AntiRaidEvent.Trigger(server, guildUsers, guild, action);

#region Antiraid DM to actioned users
            // We need to message the actioned users, if applicable, before actioning.
            string content = server.AntiraidPunishmentDirectMessage;

            if (!string.IsNullOrWhiteSpace(content))
            {
                var embed = new KaguyaEmbedBuilder(EmbedColor.ORANGE)
                {
                    Title = "Kaguya Anti-Raid Notification",
                    Description = content
                };

                var sb = new StringBuilder(content);
                foreach (SocketGuildUser guildUser in guildUsers)
                {
                    sb = sb.Replace("{USERNAME}", guildUser.UsernameAndDescriminator());
                    sb = sb.Replace("{USERMENTION}", guildUser.Mention);
                    sb = sb.Replace("{SERVER}", guild.Name);
                    sb = sb.Replace("{PUNISHMENT}", action.ToLower());

                    embed.Description = sb.ToString();

                    try
                    {
                        await guildUser.SendEmbedAsync(embed);
                    }
                    catch (HttpException httpEx)
                    {
                        await ConsoleLogger.LogAsync(
                            $"Tried to send user {guildUser.Id} a custom anti-raid DM notification " +
                            $"from guild [Name: {guild.Name} | ID: {guild.Id}] but failed to do so due to " +
                            $"an Http Exception (Failure Reason: {httpEx.Reason})", LogLvl.WARN);
                    }
                    catch (Exception ex)
                    {
                        await ConsoleLogger.LogAsync(ex, $"An unexpected error occurred when attempting to send user {guildUser.Id} a custom " +
                                                         $"anti-raid DM notification from guild [Name: {guild.Name} | ID: {guild.Id}].\n");
                    }
                }
            }
#endregion

            switch (server.AntiRaid.Action.ToLower())
            {
                case "mute":
                    await PunishMute(guildUsers);

                    break;
                case "kick":
                    await PunishKick(guildUsers);

                    break;
                case "shadowban":
                    await PunishShadowban(guildUsers);

                    break;
                case "ban":
                    await PunishBan(guildUsers);

                    break;
                default:
                    await ConsoleLogger.LogAsync("Antiraid service triggered, but no users actioned. " +
                                                 "Antiraid action string is different than expected. " +
                                                 "Expected \"mute\" \"kick\" \"shadowban\" OR \"ban\". " +
                                                 $"Received: '{action.ToLower()}'. " +
                                                 $"Guild: {guildId}.", LogLvl.ERROR);

                    break;
            }

            await ConsoleLogger.LogAsync($"Antiraid: Successfully actioned {guildUsers.Count:N0} users in guild {guild.Id}.", LogLvl.INFO);
        }

        private static async Task PunishBan(List<SocketGuildUser> guildUsers)
        {
            var ban = new Ban();
            foreach (SocketGuildUser user in guildUsers)
            {
                try
                {
                    await ban.AutoBanUserAsync(user, "Kaguya Anti-Raid protection.");
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync("Attempted to auto-ban user " +
                                                 $"{user.ToString() ?? "NULL"} as " +
                                                 "part of the antiraid service, but " +
                                                 "an exception was thrown!!", LogLvl.ERROR);
                }
            }
        }

        private static async Task PunishShadowban(IEnumerable<SocketGuildUser> guildUsers)
        {
            var sb = new Shadowban();
            foreach (SocketGuildUser user in guildUsers)
            {
                try
                {
                    await sb.AutoShadowbanUserAsync(user);
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync("Attempted to auto-shadowban user " +
                                                 $"{user.ToString() ?? "NULL"} as " +
                                                 "part of the antiraid service, but " +
                                                 "an exception was thrown!!", LogLvl.ERROR);
                }
            }
        }

        private static async Task PunishKick(IEnumerable<SocketGuildUser> guildUsers)
        {
            var kick = new Kick();
            foreach (SocketGuildUser user in guildUsers)
            {
                try
                {
                    await kick.AutoKickUserAsync(user, "Kaguya Anti-Raid protection.");
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync("Attempted to auto-kick user " +
                                                 $"{user.ToString() ?? "NULL"} as " +
                                                 "part of the antiraid service, but " +
                                                 "an exception was thrown!!", LogLvl.ERROR);
                }
            }
        }

        private static async Task PunishMute(IEnumerable<SocketGuildUser> guildUsers)
        {
            var mute = new Mute();
            foreach (SocketGuildUser user in guildUsers)
            {
                try
                {
                    await mute.AutoMute(user);
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync("Attempted to auto-mute user " +
                                                 $"{user.ToString() ?? "NULL"} as " +
                                                 "part of the antiraid service, but " +
                                                 "an exception was thrown!!", LogLvl.ERROR);
                }
            }
        }
    }

    public class AntiraidData
    {
        public ulong ServerId { get; set; }
        public HashSet<ulong> UserIds { get; set; }
    }

    public static class CachedAntiraidData
    {
        public static List<AntiraidData> ExistingData { get; set; } = new List<AntiraidData>();

        /// <summary>
        ///     Adds a timer to the cache.
        /// </summary>
        /// <param name="stObj"></param>
        public static void AddToCache(AntiraidData stObj) => ExistingData.Add(stObj);

        public static void ReplaceData(AntiraidData stObj)
        {
            AntiraidData existingObj = ExistingData.FirstOrDefault(x => x.ServerId == stObj.ServerId);

            ExistingData.Remove(existingObj);
            ExistingData.Add(stObj);
        }
    }

    public static class AntiRaidEvent
    {
        public static event Func<AntiRaidEventArgs, Task> OnRaid;
        public static void Trigger(Server server, List<SocketGuildUser> users, SocketGuild guild, string action) => AntiRaidEventTrigger(new AntiRaidEventArgs(server, users, guild, action));
        private static void AntiRaidEventTrigger(AntiRaidEventArgs e) => OnRaid?.Invoke(e);
    }

    public class AntiRaidEventArgs : EventArgs
    {
        public AntiRaidEventArgs(Server server, List<SocketGuildUser> users, SocketGuild guild, string punishment)
        {
            Server = server;
            GuildUsers = users;
            SocketGuild = guild;
            Punishment = punishment;
        }

        public Server Server { get; }
        public List<SocketGuildUser> GuildUsers { get; }
        public SocketGuild SocketGuild { get; }
        public string Punishment { get; }
    }
}