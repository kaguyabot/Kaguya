using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Context;
using LinqToDB;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class MigrateData : ModuleBase<ShardedCommandContext>
    {
        [OwnerCommand]
        [DangerousCommand]
        [Command("MigrateAllData", RunMode = RunMode.Async)]
        [Summary("Migrates all data from the old bot into the new database.")]
        [Remarks("")]
        public async Task Command()
        {
            var filePath = @"C:\Users\Stage\Desktop\accounts.json";
            var jsonText = await File.ReadAllTextAsync(filePath);

            var oldUserJson = JsonConvert.DeserializeObject<List<Rootobject>>(jsonText);

            await ReplyAsync($"{Context.User.Mention} Migrating {oldUserJson.Count} accounts. Please wait...");

            var usersToCopy = new List<User>(oldUserJson.Count);
            foreach (var u in oldUserJson)
            {
                var newUser = new User
                {
                    Id = (ulong)u.ID,
                    Experience = u.EXP,
                    Points = u.Points,
                    OsuId = new OsuUserBuilder(u.OsuUsername).Execute()?.user_id ?? 0,
                    TotalCommandUses = 0,
                    TotalDaysSupported = 0,
                    TotalNSFWImages = 0,
                    ActiveRateLimit = 0,
                    RateLimitWarnings = u.RatelimitStrikes,
                    TotalGamblingWins = (int)u.LifetimeGambleWins,
                    TotalGamblingLosses = (int)u.LifetimeGambleLosses,
                    TotalCurrencyAwarded = u.TotalCurrencyAwarded,
                    TotalCurrencyLost = u.TotalCurrencyLost,
                    TotalRollWins = 0,
                    TotalQuickdrawWins = 0,
                    TotalQuickdrawLosses = 0,
                    BlacklistExpiration = 0,
                    LatestExp = 0,
                    LatestTimelyBonus = 0,
                    LatestWeeklyBonus = 0,
                    LastGivenRep = 0,
                    LastRatelimited = 0,
                    UpvoteBonusExpiration = 0,
                    GambleHistory = null,
                    ServerExp = null,
                    Reminders = null
                };

                if (!await UserQueries.UserExistsInDatabaseAsync(newUser))
                {
                    usersToCopy.Add(newUser);
                    await ConsoleLogger.LogAsync($"User {u.ID} added to list of users to be added.", LogLvl.TRACE);
                }
            }

            if (!UserQueries.AnyUserExistsInDatabase(usersToCopy))
            {
                await UserQueries.BulkInsertUsersAsync(usersToCopy);
                await ConsoleLogger.LogAsync($"{usersToCopy.Count} users bulk-copied to database.", LogLvl.DEBUG);
            }

            await ReplyAsync($"{Context.User.Mention} Completed.");
        }
    }

    public class Rootobject
    {
        public string Username { get; set; }
        public long ID { get; set; }
        public int Points { get; set; }
        public int EXP { get; set; }
        public int Diamonds { get; set; }
        public int Rep { get; set; }
        public int KaguyaWarnings { get; set; }
        public int NSFWUsesThisDay { get; set; }
        public int CommandRateLimit { get; set; }
        public int RatelimitStrikes { get; set; }
        public string NSFWAgeVerified { get; set; }
        public string[] RecentlyUsedCommands { get; set; }
        public int GamblingBadLuckStreak { get; set; }
        public int QuickdrawWinnings { get; set; }
        public int QuickdrawLosses { get; set; }
        public bool IsSupporter { get; set; }
        public bool IsBenefitingFromUpvote { get; set; }
        public DateTime TemporaryBlacklistExpiration { get; set; }
        public DateTime LastReceivedEXP { get; set; }
        public DateTime LastReceivedTimelyPoints { get; set; }
        public DateTime LastGivenRep { get; set; }
        public DateTime LastReceivedWeeklyPoints { get; set; }
        public DateTime LastUpvotedKaguya { get; set; }
        public DateTime KaguyaSupporterExpiration { get; set; }
        public DateTime NSFWCooldownReset { get; set; }
        public int LevelNumber { get; set; }
        public string OsuUsername { get; set; }
        public bool IsBlacklisted { get; set; }
        public float LifetimeGambleWins { get; set; }
        public float LifetimeGambleLosses { get; set; }
        public float LifetimeGambles { get; set; }
        public float LifetimeEliteRolls { get; set; }
        public int TotalCurrencyGambled { get; set; }
        public int TotalCurrencyAwarded { get; set; }
        public int TotalCurrencyLost { get; set; }
        public string[] GambleHistory { get; set; }
    }
}
