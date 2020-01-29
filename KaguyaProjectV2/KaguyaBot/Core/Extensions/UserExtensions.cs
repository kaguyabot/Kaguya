﻿using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

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
            var allExp = (await DatabaseQueries.GetAllAsync<User>(x => x.Experience > 0)).OrderByDescending(x => x.Experience).ToList();
            var rank = allExp.IndexOf(allExp.First(x => x.UserId == user.UserId));

            return (rank + 1, allExp.Count);
        }
    }
}
