using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using Microsoft.Extensions.Logging;
using LogLevel = KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage.LogLevel;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class GenerateDummyData : ModuleBase<ShardedCommandContext>
    {
        [OwnerCommand]
        [Command("gendata", RunMode = RunMode.Async)]
        [Summary("Generates dummy data based on what number you put in as the " +
                 "parameter.")]
        [Remarks("<num>\n5000 => 5,000 dummy accounts generated.")]
        public async Task GenData(int num)
        {
            List<User> users = new List<User>(); 

            await ReplyAsync("Generating data...");
            for (int i = 0; i < num; i++)
            {
                Random rand = new Random();
                var id = rand.Next(100000000, 999999999);
                var user = new User
                {
                    Id = (ulong) id,
                    Experience = 0,
                    Points = 0,
                    OsuId = 0,
                    TotalCommandUses = 0,
                    TotalNSFWImages = 0,
                    ActiveRateLimit = 0,
                    RateLimitWarnings = 0,
                    TotalGamblingWins = 0,
                    TotalGamblingLosses = 0,
                    TotalCurrencyAwarded = 0,
                    TotalCurrencyLost = 0,
                    TotalRollWins = 0,
                    TotalQuickdrawWins = 0,
                    TotalQuickdrawLosses = 0,
                    BlacklistExpiration = 0,
                    LatestExp = 0,
                    LatestTimelyBonus = 0,
                    LatestWeeklyBonus = 0,
                    LastGivenRep = 0,
                    UpvoteBonusExpiration = 0,
                    GambleHistory = null
                };

                users.Add(user);
            }

            await UserQueries.UpdateUsers(users);
            await ReplyAsync("Complete.");
        }
    }
}
