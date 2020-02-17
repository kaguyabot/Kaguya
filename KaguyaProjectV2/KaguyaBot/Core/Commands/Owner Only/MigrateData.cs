using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using Newtonsoft.Json;
using OsuSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using User = KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models.User;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class MigrateData : KaguyaBase
    {
        [OwnerCommand]
        [DangerousCommand]
        [Command("MigrateAllData", RunMode = RunMode.Async)]
        [Summary("Migrates all data from the old bot into the new database.")]
        [Remarks("<Accounts file path> <Servers file path>")]
        public async Task Command(string accountsFilePath, string serversFilePath)
        {
            bool enabled = false;
            if (!enabled)
            {
                throw new Exception("Unabled to execute this command. This command " +
                                    "has been hardcoded to never execute as a safe-guard " +
                                    "to an accidental data migration.");
            }

#if DEBUG
            await ReplyAsync($"{Context.User.Mention} Clearing database...");

            await DatabaseQueries.DeleteAllAsync<AntiRaidConfig>();
            await DatabaseQueries.DeleteAllAsync<AutoAssignedRole>();
            await DatabaseQueries.DeleteAllAsync<BlackListedChannel>();
            await DatabaseQueries.DeleteAllAsync<CommandHistory>();
            await DatabaseQueries.DeleteAllAsync<FilteredPhrase>();
            await DatabaseQueries.DeleteAllAsync<Fish>();
            await DatabaseQueries.DeleteAllAsync<GambleHistory>();
            await DatabaseQueries.DeleteAllAsync<MutedUser>();
            await DatabaseQueries.DeleteAllAsync<PremiumKey>();
            await DatabaseQueries.DeleteAllAsync<Rep>();
            await DatabaseQueries.DeleteAllAsync<Reminder>();
            await DatabaseQueries.DeleteAllAsync<ServerExp>();
            await DatabaseQueries.DeleteAllAsync<ServerRoleReward>();
            await DatabaseQueries.DeleteAllAsync<SupporterKey>();
            await DatabaseQueries.DeleteAllAsync<ServerExp>();
            await DatabaseQueries.DeleteAllAsync<UserBlacklist>();
            await DatabaseQueries.DeleteAllAsync<WarnedUser>();
            await DatabaseQueries.DeleteAllAsync<WarnSetting>();
            await DatabaseQueries.DeleteAllAsync<TopGgWebhook>();

            await ReplyAsync($"{Context.User.Mention} Users, servers, and 8ball still remain.");
#endif
            var userjsonText = await File.ReadAllTextAsync(accountsFilePath);
            var oldUserJson = JsonConvert.DeserializeObject<List<OldUser>>(userjsonText);

            var serverjsonText = await File.ReadAllTextAsync(serversFilePath);
            var oldServerJson = JsonConvert.DeserializeObject<List<OldServer>>(serverjsonText);

            await ReplyAsync($"{Context.User.Mention} Users and servers parsed from JSON into memory.");

            foreach (var u in oldUserJson)
            {
                if (u.KaguyaSupporterExpiration.ToOADate() > DateTime.Now.ToOADate())
                {
                    var suppKey = new SupporterKey
                    {
                        Key = SupporterKeyGen.RandomString(),
                        LengthInSeconds = (long)(u.KaguyaSupporterExpiration - DateTime.Now).TotalSeconds,
                        KeyCreatorId = 538910393918160916,
                        UserId = u.ID,
                        Expiration = u.KaguyaSupporterExpiration.ToOADate()
                    };
                    await DatabaseQueries.InsertAsync(suppKey);
                }

                if (u.IsBlacklisted)
                {
                    var uBlacklist = new UserBlacklist
                    {
                        Expiration = DateTime.MaxValue.ToOADate(),
                        Reason = $"Transfer of Kaguya V1 blacklist during data migration.",
                        UserId = u.ID
                    };

                    await DatabaseQueries.InsertAsync(uBlacklist);
                }

                if (u.Rep > 0)
                {
                    var r = new List<Rep>();
                    for (int i = 0; i < u.Rep; i++)
                    {
                        r.Add(new Rep
                        {
                            UserId = u.ID,
                            GivenBy = 538910393918160916,
                            TimeGiven = DateTime.Now.ToOADate(),
                            Reason = ""
                        });
                    }

                    await DatabaseQueries.InsertAsync(r);
                }

                await ConsoleLogger.LogAsync($"User {u.ID} added to list of users to be added.", LogLvl.TRACE);
            }

            await ReplyAsync($"{Context.User.Mention} User-related tables populated in database.");
            await Task.Delay(500);
            await ReplyAsync($"{Context.User.Mention} beginning migration of servers into database...");

            var serversToCopy = new List<Server>();

            foreach (var server in oldServerJson)
            {
                var newServer = new Server
                {
                    ServerId = server.ID,
                    CommandPrefix = server.CommandPrefix,
                    TotalCommandCount = server.TotalCommandCount,
                    TotalAdminActions = 0,
                    PraiseCooldown = 24,
                    ModLog = 0,
                    LogDeletedMessages = server.LogDeletedMessages,
                    LogUpdatedMessages = server.LogUpdatedMessages,
                    LogFilteredPhrases = server.LogWhenUserSaysFilteredPhrase,
                    LogUserJoins = server.LogWhenUserJoins,
                    LogUserLeaves = server.LogWhenUserLeaves,
                    LogBans = server.LogBans,
                    LogUnbans = server.LogUnbans,
                    LogVoiceChannelConnections = server.LogWhenUserConnectsToVoiceChannel,
                    LogLevelAnnouncements = server.LogLevelUpAnnouncements,
                    LogFishLevels = 0,
                    LogAntiraids = server.LogAntiRaids,
                    LogGreetings = 0,
                    LogTwitchNotifications = 0,
                    IsBlacklisted = server.IsBlacklisted,
                    CustomGreeting = "",
                    CustomGreetingIsEnabled = false,
                    AutoWarnOnBlacklistedPhrase = false,
                    IsCurrentlyPurgingMessages = false,
                    LevelAnnouncementsEnabled = true
                };

                serversToCopy.Add(newServer);
            }

            await ReplyAsync($"{Context.User.Mention} Servers populated into memory as list. " +
                             $"Now attempting to bulk-copy into database...");

            await DatabaseQueries.DeleteAllAsync<Server>();
            await DatabaseQueries.BulkCopy(serversToCopy);
            await ConsoleLogger.LogAsync($"{serversToCopy.Count} servers bulk-copied to database.", LogLvl.DEBUG);
            await ReplyAsync($"{Context.User.Mention} {serversToCopy.Count:N0} servers bulk copied to database.");

            int j = 0;
            foreach (var server in oldServerJson)
            { 
                if (server.FilteredWords.Length > 0)
                {
                    var fp = new List<FilteredPhrase>(server.FilteredWords.Length);
                    foreach (var phrase in server.FilteredWords)
                    {
                        fp.Add(new FilteredPhrase
                        {
                            ServerId = server.ID,
                            Phrase = phrase,
                            Server = serversToCopy[j]
                        });
                    }

                    await DatabaseQueries.BulkCopy(fp);
                }

                if (server.AutoAssignedRoles.Length > 0)
                {
                    var aar = new List<AutoAssignedRole>(server.AutoAssignedRoles.Length);
                    foreach (var roleId in server.AutoAssignedRoles)
                    {
                        aar.Add(new AutoAssignedRole
                        {
                            ServerId = server.ID,
                            RoleId = roleId,
                            Server = serversToCopy[j]
                        });
                    }

                    await DatabaseQueries.BulkCopy(aar);
                }

                if (server.BlacklistedChannels.Length > 0)
                {
                    var bc = new List<BlackListedChannel>();
                    foreach (var bcId in server.BlacklistedChannels)
                    {
                        try
                        {
                            bc.Add(new BlackListedChannel
                            {
                                ServerId = server.ID,
                                ChannelId = bcId,
                                Expiration = DateTime.MaxValue.ToOADate(),
                                Server = serversToCopy[j]
                            });
                        }
                        catch (Exception)
                        {
                        }
                    }

                    await DatabaseQueries.BulkCopy(bc);
                }

                if (server.WarnedMembers.WarnedUsers.Count != 0)
                {
                    var wm = new List<WarnedUser>(server.WarnedMembers.WarnedUsers.Count);
                    foreach (var user in server.WarnedMembers.WarnedUsers)
                    {
                        try
                        {
                            wm.Add(new WarnedUser
                            {
                                ServerId = server.ID,
                                UserId = user.Keys.First(),
                                ModeratorName = "Kaguya#2708",
                                Reason = "Automatic warning transfer from V1 ==> V2 data migration.",
                                Date = DateTime.Now.ToOADate(),
                                Server = serversToCopy[j]
                            });
                        }
                        catch(Exception)
                        {
                        }
                    }

                    await DatabaseQueries.BulkCopy(wm);
                }

                //If this server had the antiraid service enabled...
                if (server.AntiRaid)
                {
                    try
                    {
                        var ar = new AntiRaidConfig
                        {
                            ServerId = server.ID,
                            Users = server.AntiRaidCount,
                            Seconds = server.AntiRaidSeconds,
                            Action = server.AntiRaidPunishment,
                            Server = serversToCopy[j]
                        };

                        await DatabaseQueries.InsertAsync(ar);
                    }
                    catch (Exception)
                    {
                        //
                    }
                    
                }

                try
                {
                    var wa = new WarnSetting
                    {
                        Ban = server.WarnActions.ban,
                        Kick = server.WarnActions.kick,
                        Mute = server.WarnActions.mute,
                        Shadowban = server.WarnActions.shadowban,
                        ServerId = server.ID
                    };

                    if (!(wa.Ban == 0 && wa.Kick == 0 &&
                          wa.Mute == 0 && wa.Shadowban == 0))
                    {
                        await DatabaseQueries.InsertAsync(wa);
                    }
                }
                catch (Exception)
                {
                }

                j++;
            }
            
            await ReplyAsync($"{Context.User.Mention} Database migration completed. Yay!");
        }
    }

    public class OldUser
    {
        public ulong ID { get; set; }
        public int Points { get; set; }
        public int EXP { get; set; }
        public int Diamonds { get; set; }
        public int Rep { get; set; }
        public int QuickdrawWinnings { get; set; }
        public int QuickdrawLosses { get; set; }
        public DateTime KaguyaSupporterExpiration { get; set; }
        public string OsuUsername { get; set; }
        public bool IsBlacklisted { get; set; }
        public float LifetimeGambleWins { get; set; }
        public float LifetimeGambleLosses { get; set; }
        public int TotalCurrencyAwarded { get; set; }
        public int TotalCurrencyLost { get; set; }
    }


    public class OldServer
    {
        public ulong ID { get; set; }
        public string CommandPrefix { get; set; }
        public bool MessageAnnouncements { get; set; }
        public Warnactions WarnActions { get; set; }
        public Warnedmembers WarnedMembers { get; set; }
        public bool AntiRaid { get; set; }
        public string AntiRaidPunishment { get; set; }
        public int AntiRaidSeconds { get; set; }
        public int AntiRaidCount { get; set; }
        public int TotalCommandCount { get; set; }
        public string[] FilteredWords { get; set; }
        public ulong[] AutoAssignedRoles { get; set; }
        public ulong[] BlacklistedChannels { get; set; }
        public ulong LogDeletedMessages { get; set; }
        public ulong LogUpdatedMessages { get; set; }
        public ulong LogWhenUserJoins { get; set; }
        public ulong LogWhenUserLeaves { get; set; }
        public ulong LogBans { get; set; }
        public ulong LogUnbans { get; set; }
        public ulong LogWhenUserSaysFilteredPhrase { get; set; }
        public ulong LogWhenUserConnectsToVoiceChannel { get; set; }
        public ulong LogLevelUpAnnouncements { get; set; }
        public ulong LogAntiRaids { get; set; }
        public bool IsBlacklisted { get; set; }
    }

    public class Warnactions
    {
        public int mute { get; set; }
        public int kick { get; set; }
        public int ban { get; set; }
        public int shadowban { get; set; }
    }

    public class Warnedmembers
    {
        public List<Dictionary<ulong, int>> WarnedUsers { get; set; } = new List<Dictionary<ulong, int>>();
    }
}
