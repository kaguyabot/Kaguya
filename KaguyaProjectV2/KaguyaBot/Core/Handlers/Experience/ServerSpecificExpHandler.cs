using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable RedundantAssignment

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience
{
    public class ServerSpecificExpHandler
    {
        public static async Task AddExp(User user, Server server, ICommandContext context)
        {
            var specificExps = await DatabaseQueries.FindAllForServerAsync<ServerExp>(server.ServerId);
            var levelAnnouncementChannel = await context.Guild.GetChannelAsync(server.LogLevelAnnouncements);
            var userExpObj = new ServerExp();

            // If the user can receive exp, give them between 5 and 8.
            if (!await CanGetExperience(specificExps, server, user))
            {
                return;
            }

            Random r = new Random();
            int exp = r.Next(5, 8);

            if (server.ServerExp != null)
            {
                userExpObj = server.ServerExp.FirstOrDefault(x => x.UserId == user.UserId);
            }
            else
            {
                userExpObj = new ServerExp
                {
                    ServerId = server.ServerId,
                    UserId = user.UserId,
                    Exp = 0,
                    LatestExp = 0
                };
            }

            var expObject = new ServerExp
            {
                ServerId = server.ServerId,
                UserId = user.UserId,
                Exp = userExpObj?.Exp ?? 0,
                LatestExp = 0
            };

            double oldLevel = ReturnLevel(server, user);

            expObject.Exp += exp;
            expObject.LatestExp = DateTime.Now.ToOADate();
            await DatabaseQueries.InsertOrReplaceAsync(expObject);

            // We update server again below because we have to refresh the serverExp list.

            server = await DatabaseQueries.GetOrCreateServerAsync(server.ServerId);
            double newLevel = ReturnLevel(server, user);
            await ConsoleLogger.LogAsync(
                $"[Server Exp]: User {user.UserId}] has received {exp} exp. [Guild: {server.ServerId}] " +
                $"Total Exp: {expObject.Exp:N0}]", LogLvl.TRACE);

            if (HasLeveledUp((int) oldLevel, (int) newLevel))
            {
                await ConsoleLogger.LogAsync(
                    $"[Server Exp]: [Server {server.ServerId} | User {user.UserId}] has leveled up! [Level: {(int)newLevel} | Experience: {GetExpForUser(server, user)}]",
                    LogLvl.INFO);

                // Don't send announcement if the channel is blacklisted.
                if (server.BlackListedChannels.Any(x => x.ChannelId == context.Channel.Id))
                    return;

                if (levelAnnouncementChannel != null && levelAnnouncementChannel is SocketTextChannel textChannel)
                {
                    await textChannel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
                }
                else
                {
                    await context.Channel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
                }
            }
        }

        private static async Task<bool> CanGetExperience(IEnumerable<ServerExp> serverExp, Server server, User user)
        {
            try
            {
                double twoMinutesAgo = DateTime.Now.AddSeconds(-120).ToOADate();
                return twoMinutesAgo >= serverExp.FirstOrDefault(x => x.UserId == user.UserId)?.LatestExp;
            }
            catch (NullReferenceException)
            {
                await DatabaseQueries.InsertAsync(new ServerExp
                {
                    Exp = 0,
                    LatestExp = 0,
                    ServerId = server.ServerId,
                    UserId = user.UserId
                });
                return true;
            }
        }

        private static double ReturnLevel(Server server, User user)
        {
            int exp = GetExpForUser(server, user);
            return Math.Sqrt(exp / 8 + -8);
        }

        private static bool HasLeveledUp(int oldLevel, int newLevel)
        {
            return oldLevel < newLevel;
        }

        /// <summary>
        /// Returns the number of EXP the user has in the specified server.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private static int GetExpForUser(Server server, User user)
        {
            return server.ServerExp?.FirstOrDefault(x => x.UserId == user.UserId)?.Exp ?? 0;
        }

        public static Embed LevelUpEmbed(User user, Server server, ICommandContext context)
        {
            var exp = GetExpForUser(server, user);
            var rank = DatabaseQueries.GetServerExpRankForUser(server, user);

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = "Level Up!",
                Description = $"`{context.User.Username}` just leveled up! \n" +
                              $"Server Level: `{(int)ReturnLevel(server, user)}` | EXP: `{exp:N0}`\n" +
                              $"Rank: `#{rank}/{server.ServerExp.ToList().Count:N0}`",
                ThumbnailUrl = ConfigProperties.Client.GetUser(user.UserId).GetAvatarUrl()
            };

            return embed.Build();
        }
    }
}
