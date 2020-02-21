using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable 4014

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Returns a <see cref="Tuple"/> containing two integers. The first is the user's Xp Rank in the <see cref="Server"/>
        /// passed to this method. The second is how many users have Xp in the <see cref="Server"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static (int, int) GetServerXpRank(this User user, Server server)
        {
            var exp = server.ServerExp.FirstOrDefault(x => x.UserId == user.UserId);
            var rank = server.ServerExp.OrderByDescending(x => x.Exp).ToList().IndexOf(exp);

            return (rank + 1, server.ServerExp.Count());
        }

        /// <summary>
        /// Returns a <see cref="Tuple"/> containing two integers. The first integer is the user's Xp Rank
        /// out of all users in the database. The second is how many total users there are in the database.
        /// </summary>
        /// <param name="user">The user of whom we are finding the Xp rank of.</param>
        /// <returns></returns>
        public static async Task<(int, int)> GetGlobalXpRank(this User user)
        {
            var allExp = (await DatabaseQueries.GetAllAsync<User>()).OrderByDescending(x => x.Experience).ToList();
            var rank = allExp.IndexOf(allExp.First(x => x.UserId == user.UserId)) + 1;

            return (rank, allExp.Count);
        }

        /// <summary>
        /// Returns this user's exact global level.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GlobalLevel(this User user)
        {
            return GlobalProperties.CalculateLevelFromExp(user.Experience);
        }

        /// <summary>
        /// Returns the lowest possible amount of EXP needed to reach this <see cref="User"/>'s next level.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int NextGlobalLevelExp(this User user)
        {
            return GlobalProperties.CalculateExpFromLevel(Math.Floor(user.GlobalLevel() + 1));
        }

        /// <summary>
        /// Returns the current exact level of the user in the given server.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static double ServerLevel(this User user, Server server)
        {
            return GlobalProperties.CalculateLevelFromExp(user.ServerExp(server));
        }

        /// <summary>
        /// Returns the exact value of the next level this user will reach.
        /// Ex: <code>user.ServerLevel == 53.3465346 ~~~~ user.NextServerLevel == 54.3465346</code>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static double NextServerLevel(this User user, Server server)
        {
            return ServerLevel(user, server) + 1;
        }

        /// <summary>
        /// Returns the integer value of the very next server level the user will reach if they continue to earn EXP.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static int NextServerLevelRoundedDown(this User user, Server server)
        {
            return (int)Math.Floor(ServerLevel(user, server) + 1);
        }

        /// <summary>
        /// Returns the minimum required amount of EXP needed to reach the next level in the given server.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static int NextServerLevelExp(this User user, Server server)
        {
            return GlobalProperties.CalculateExpFromLevel(Math.Floor(user.NextServerLevel(server)));
        }

        public static int ServerExp(this User user, Server server)
        {
            if (server.ServerExp?.Count() != null || server.ServerExp?.Count() != 0)
            {
                try
                {
                    return server.ServerExp?.First(x => x?.UserId == user.UserId)?.Exp ?? 0;
                }
                catch (InvalidOperationException)
                {
                    var exp = new ServerExp
                    {
                        ServerId = server.ServerId,
                        UserId = user.UserId,
                        Exp = 0,
                        LatestExp = 0,
                        Server = server,
                        User = user
                    };

                    if (server.ServerExp != null && server.ServerExp.Any(x => x.UserId == exp.UserId))
                        DatabaseQueries.InsertOrReplaceAsync(exp);
                    ConsoleLogger.LogAsync($"User {user.UserId} in {server.ServerId} was not present " +
                                                 $"in the guild's ServerExp list when attempting to load this value. " +
                                                 $"They have now been added into the database under the ServerExp table.", LogLvl.DEBUG);

                    return 0;
                }
            }
            return 0;
        }

        public static double PercentToNextLevel(this User user)
        {
            var curLevel = user.GlobalLevel();
            var curLevelExpRoundedDown = GlobalProperties.CalculateExpFromLevel(Math.Floor(curLevel));
            var minMaxDifference = XpDifferenceBetweenLevels(user);

            if (curLevel.Rounded(RoundDirection.Down) == 0)
            {
                curLevelExpRoundedDown = 0;
                minMaxDifference = GlobalProperties.CalculateExpFromLevel(1);
            }

            return (((double)user.Experience - curLevelExpRoundedDown) / minMaxDifference);
        }

        public static double PercentToNextServerLevel(this User user, Server server)
        {
            var curLevel = user.ServerLevel(server);
            var curLevelExpRoundedDown = GlobalProperties.CalculateExpFromLevel(curLevel.Rounded(RoundDirection.Down));
            var minMaxDifference = XpDifferenceBetweenLevels(user, server);

            if (curLevel.Rounded(RoundDirection.Down) == 0)
            {
                curLevelExpRoundedDown = 0;
                minMaxDifference = GlobalProperties.CalculateExpFromLevel(1);
            }

            return (((double)user.ServerExp(server) - curLevelExpRoundedDown) / minMaxDifference);
        }

        #region Helper function for PercentToNextLevel -- NOT an extension.

        /// <summary>
        /// Calculates the difference of EXP between the user's current level (rounded
        /// down to the nearest integer) and one level above this value.
        /// </summary>
        /// <returns></returns>
        private static int XpDifferenceBetweenLevels(User user)
        {
            var curLevel = user.GlobalLevel().Rounded(RoundDirection.Down);
            var nextLevel = (user.GlobalLevel() + 1).Rounded(RoundDirection.Down);
            var curLevelExp = GlobalProperties.CalculateExpFromLevel(curLevel);
            var nextLevelExp = GlobalProperties.CalculateExpFromLevel(nextLevel);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (curLevel == 0)
                curLevelExp = user.Experience;

            return nextLevelExp - curLevelExp;
        }

        private static int XpDifferenceBetweenLevels(User user, Server server)
        {
            var curLevel = Math.Floor(user.ServerLevel(server));
            var nextLevel = Math.Floor(user.ServerLevel(server) + 1);
            var curLevelExp = GlobalProperties.CalculateExpFromLevel(curLevel);
            var nextLevelExp = GlobalProperties.CalculateExpFromLevel(nextLevel);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (curLevel == 0)
                curLevelExp = user.ServerExp(server);

            return nextLevelExp - curLevelExp;
        }
        #endregion

        public static bool HasRecentlyUsedNSFWCommands(this User user, int days = 7)
        {
            return user.CommandHistory?.Where(x => x.Command == "nsfw")
                .Any(x => x.Timestamp > DateTime.Now.AddDays(-days)) ?? false;
        }

        public static int FishbaitCost(this User user)
        {
            return user.IsSupporter 
                ? (int)(Fish.SUPPORTER_BAIT_COST * (1 + user.FishLevelBonuses.BaitCostIncreasePercent / 100))
                : (int)(Fish.BAIT_COST * (1 + (user.FishLevelBonuses.BaitCostIncreasePercent / 100)));
        }

        /// <summary>
        /// Returns whether the user has any active Kaguya Premium keys.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<bool> IsPremiumAsync(this User user)
        {
            var allKeys = await DatabaseQueries.GetAllForUserAsync<PremiumKey>(user.UserId, 
                x => x.Expiration > DateTime.Now.ToOADate());

            return allKeys.Count > 0;
        }

        /// <summary>
        /// Returns all unexpired, active premium keys for this user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<List<PremiumKey>> GetPremiumKeysAsync(this User user)
        {
            return (await DatabaseQueries.GetAllForUserAsync<PremiumKey>(user.UserId,
                x => x.Expiration > DateTime.Now.ToOADate())).ToList();
        }
    }
}
