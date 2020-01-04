using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience
{
    public static class ExperienceHandler
    {
        public static async Task AddExp(User user, ICommandContext context)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);

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
            await UserQueries.UpdateUserAsync(user);

            double newLevel = ReturnLevel(user);
            await ConsoleLogger.LogAsync($"[Global Exp]: User {user.Id} has received {exp} exp. [New Total: {user.Experience:N0} Exp]", 
                LogLvl.DEBUG);

            if (!HasLeveledUp(oldLevel, newLevel))
            {
                return;
            }
            await ConsoleLogger.LogAsync($"[Global Exp]: User {user.Id} has leveled up! [Level: {newLevel} | EXP: {user.Experience:N0}]",
                DataStorage.JsonStorage.LogLvl.INFO);
            if (levelAnnouncementChannel != null && levelAnnouncementChannel is IMessageChannel textChannel)
            {
                await textChannel.SendMessageAsync(embed: await LevelUpEmbed(user, context));
                return;
            }

            await context.Channel.SendMessageAsync(embed: await LevelUpEmbed(user, context));
        }

        private static bool CanGetExperience(User user)
        {
            double twoMinutesAgo = DateTime.Now.AddSeconds(-120).ToOADate();
            return twoMinutesAgo >= user.LatestExp;
        }

        private static double ReturnLevel(User user)
        {
            return Math.Sqrt(user.Experience / 8 + -8);
        }

        private static bool HasLeveledUp(double oldLevel, double newLevel)
        {
            return Math.Floor(oldLevel) < Math.Floor(newLevel);
        }

        private static async Task<Embed> LevelUpEmbed(User user, ICommandContext context)
        {
            int count = await UtilityQueries.GetCountOfUsersAsync();
            var rankNum = UserQueries.GetGlobalExpRankIndex(user) + 1;

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = "Level Up!",
                Description = $"`{context.User.Username}` just leveled up! \n" +
                              $"Level: `{(int)ReturnLevel(user)}` | EXP: `{user.Experience:N0}`\n" +
                              $"Rank: `#{rankNum}/{count:N0}`",
                ThumbnailUrl = ConfigProperties.Client.GetUser(user.Id).GetAvatarUrl()
            };

            return embed.Build();
        }
    }
}
