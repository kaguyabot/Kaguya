using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience
{
    public static class ExperienceHandler
    {
        public static async void AddExp(User user, ICommandContext context)
        {
            Server server = ServerQueries.GetServer(context.Guild.Id);

            // If the user can receive exp, give them between 5 and 8.
            if (!CanGetExperience(user))
            {
                return;
            }
            var levelAnnouncementChannel = await context.Guild.GetChannelAsync(server.LogLevelAnnouncements);
            double oldLevel = ReturnLevel(user);

            Random r = new Random();
            int exp = r.Next(5, 8);

            user.Experience += exp;
            user.LatestExp = DateTime.Now.ToOADate();
            UserQueries.UpdateUser(user);

            double newLevel = ReturnLevel(user);
            await ConsoleLogger.Log($"[Global Exp]: User has received {exp} exp. [ID: {user.Id} | New EXP: {user.Experience:N0}]",
                DataStorage.JsonStorage.LogLevel.DEBUG);

            if (!HasLeveledUp(oldLevel, newLevel))
            {
                return;
            }
            await ConsoleLogger.Log($"[Global Exp]: User has leveled up! [ID: {user.Id} | Level: {newLevel} | Experience: {user.Experience}]",
                DataStorage.JsonStorage.LogLevel.INFO);
            if (levelAnnouncementChannel != null && levelAnnouncementChannel is IMessageChannel textChannel)
            {
                await textChannel.SendMessageAsync(embed: LevelUpEmbed(user, context));
                return;
            }

            //await context.Channel.SendMessageAsync(embed: LevelUpEmbed(user, context));
        }

        private static bool CanGetExperience(User user)
        {
            return DateTime.Now.AddSeconds(-120).ToOADate() >= user.LatestExp;
        }

        private static double ReturnLevel(User user)
        {
            return Math.Sqrt(user.Experience / 8 + -8);
        }

        private static bool HasLeveledUp(double oldLevel, double newLevel)
        {
            if (Math.Floor(oldLevel) < Math.Floor(newLevel))
            {
                return true;
            }
            return false;
        }

        public static Embed LevelUpEmbed(User user, ICommandContext context)
        {
            var allUsers = UserQueries.GetAllUsers().ToList();
            var rankIndex = allUsers.OrderByDescending(x => x.Experience).ToList().FindIndex(x => x.Id == user.Id) + 1;

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = "Level Up!",
                Description = $"{context.User.Username} just leveled up! \n" +
                              $"[Level: {ReturnLevel(user)} | Experience Points: {user.Experience:N0}]\n" +
                              $"Rank: `#{rankIndex}/{allUsers.Count:N0}`",
                ThumbnailUrl = ConfigProperties.client.GetUser(user.Id).GetAvatarUrl()
            };

            return embed.Build();
        }
    }
}
