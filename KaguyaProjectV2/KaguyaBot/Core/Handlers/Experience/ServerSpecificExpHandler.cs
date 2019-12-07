using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience
{
    public class ServerSpecificExpHandler
    {
        public static async void AddExp(User user, Server server, ICommandContext context)
        {
            // If the user can receive exp, give them between 5 and 8.
            if (!await CanGetExperience(server, user))
            {
                return;
            }

            var levelAnnouncementChannel = await context.Guild.GetChannelAsync(server.LogLevelAnnouncements);
            var userExpObj = server.ServerExp.FirstOrDefault(x => x.UserId == user.Id);

            double oldLevel = ReturnLevel(server, user);

            Random r = new Random();
            int exp = r.Next(5, 8);

            var expObject = new ServerExp
            {
                ServerId = server.Id,
                UserId = user.Id,
                Exp = userExpObj?.Exp ?? 0,
                LatestExp = 0
            };

            expObject.Exp += exp;
            expObject.LatestExp = DateTime.Now.ToOADate();
            await ServerQueries.ReplaceServerSpecificExpForUser(userExpObj, expObject);

            double newLevel = ReturnLevel(server, user);
            await ConsoleLogger.Log($"[Server Specific Exp]: User has received {exp} exp. [ID: {user.Id} | New EXP: {userExpObj?.Exp ?? exp:N0}]",
                DataStorage.JsonStorage.LogLevel.DEBUG);

            if (!HasLeveledUp(oldLevel, newLevel))
            {
                return;
            }

            await ConsoleLogger.Log($"[Server Specific Exp]: User has leveled up! [ID: {user.Id} | Level: {newLevel} | Experience: {user.Experience}]",
                DataStorage.JsonStorage.LogLevel.INFO);

            if (levelAnnouncementChannel != null && levelAnnouncementChannel is IMessageChannel textChannel)
            {
                await textChannel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
                return;
            }

            await context.Channel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
            await ServerQueries.UpdateServer(server);
        }

        private static async Task<bool> CanGetExperience(Server server, User user)
        {
            try
            {
                return DateTime.Now.AddSeconds(-120).ToOADate() >=
                       server.ServerExp.FirstOrDefault(x => x.UserId == user.Id)?.LatestExp;
            }
            catch (ArgumentNullException)
            {
                server.ServerExp.Add(new ServerExp
                {
                    Exp = 0,
                    LatestExp = 0,
                    ServerId = server.Id,
                    UserId = user.Id
                });
                await ServerQueries.UpdateServer(server);
                return true;
            }
        }

        private static double ReturnLevel(Server server, User user)
        {
            double? exp = server.ServerExp.FirstOrDefault(x => x.UserId == user.Id)?.Exp;
            if (exp != null)
                return Math.Sqrt((double) exp / 8 + -8);
            return 0;
        }

        private static bool HasLeveledUp(double oldLevel, double newLevel)
        {
            return Math.Floor(oldLevel) < Math.Floor(newLevel);
        }

        public static Embed LevelUpEmbed(User user, Server server, ICommandContext context)
        {
            var rankIndex = server.ServerExp.OrderByDescending(x => x.Exp).ToList().FindIndex(x => x.UserId == user.Id) + 1;

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = "Level Up!",
                Description = $"{context.User.Username} just leveled up! \n" +
                              $"[Server Level: {ReturnLevel(server, user)} | Experience Points: {user.Experience:N0}]\n" +
                              $"Rank: #{rankIndex}/{server.ServerExp.Count:N0}",
                ThumbnailUrl = ConfigProperties.client.GetUser(user.Id).GetAvatarUrl()
            };

            return embed.Build();
        }
    }
}
