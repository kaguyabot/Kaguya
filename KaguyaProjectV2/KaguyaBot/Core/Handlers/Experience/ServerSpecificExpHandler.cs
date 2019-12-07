using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
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
            // If the user can receive exp, give them between 5 and 8 exp.
            if (!await CanGetExperience(server, user))
            {
                return;
            }

            var levelAnnouncementChannel = await context.Guild.GetChannelAsync(server.LogLevelAnnouncements);
            var userExpObj = server.ServerExp.FirstOrDefault(x => x.UserId == user.Id);

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
            await ServerQueries.UpdateServerExp(expObject);

            double newLevel = ReturnLevel(server, user);
            await ConsoleLogger.Log($"[Server Exp]: User has received {exp} exp. [Guild: [Name: {context.Guild.Name} | ID: {server.Id}] " +
                                    $"User: [ID: {user.Id}] | New EXP: {expObject.Exp:N0}]", LogLevel.DEBUG);

            double oldLevel = ReturnLevel(server, user);
            if (HasLeveledUp((int)oldLevel, (int)newLevel))
            {
                await ConsoleLogger.Log($"[Server Exp]: User has leveled up! [ID: {user.Id} | Level: {newLevel} | Experience: {user.Experience}]",
                    LogLevel.INFO);

                if (levelAnnouncementChannel != null && levelAnnouncementChannel is IMessageChannel textChannel)
                {
                    await textChannel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
                    return;
                }

                await context.Channel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
            }
        }

        // ReSharper disable once PossibleNullReferenceException
        private static async Task<bool> CanGetExperience(Server server, User user)
        {
            try
            {
                return DateTime.Now.AddSeconds(-120).ToOADate() >=
                       server.ServerExp.FirstOrDefault(x => x.UserId == user.Id).LatestExp;
            }
            catch (NullReferenceException)
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
            return exp != null ? Math.Sqrt((double) exp / 8 + -8) : 0;
        }

        private static bool HasLeveledUp(int oldLevel, int newLevel)
        {
            return oldLevel < newLevel;
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
