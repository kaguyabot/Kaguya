using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Commands.EXP;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Images.ExpLevelUp;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience
{
    public static class ExperienceHandler
    {
        public static async Task TryAddExp(User user, Server server, ICommandContext context)
        {
            Stopwatch sw = Stopwatch.StartNew();
            await Task.Run(async () =>
            {
                // If the user can receive exp, give them between 5 and 8.
                if (!CanGetExperience(user))
                    return;

                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} confirmed to be able to earn global exp.");

                // Don't give exp to any user who is blacklisted.
                // Members in blacklisted servers also cannot earn exp.
                if (user.IsBlacklisted || server.IsBlacklisted)
                    return;

                SocketTextChannel levelAnnouncementChannel = null;
                if (server.LogLevelAnnouncements != 0)
                {
                    levelAnnouncementChannel = await context.Guild.GetTextChannelAsync(server.LogLevelAnnouncements) as SocketTextChannel;
                }

                if (levelAnnouncementChannel == null)
                    levelAnnouncementChannel = (SocketTextChannel) context.Channel;

                double oldLevel = ReturnLevel(user);
                
                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} old level identified.");

                var r = new Random();
                int exp = r.Next(5, 8);
                int points = r.Next(1, 4);

                user.Experience += exp;
                user.Points += points;
                user.LastGivenExp = DateTime.Now.ToOADate();
                await DatabaseQueries.UpdateAsync(user);
                
                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} updated in the database.");

                double newLevel = ReturnLevel(user);
                await ConsoleLogger.LogAsync($"[Global Exp]: User {user.UserId} has received {exp} exp and {points} points. " +
                                             $"[New Total: {user.Experience:N0} Exp]", LogLvl.DEBUG);

                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} new level identified.");
                
                if (!HasLeveledUp(oldLevel, newLevel))
                    return;

                await ConsoleLogger.LogAsync($"[Global Exp]: User {user.UserId} has leveled up! " +
                                             $"[Level: {Math.Floor(newLevel):0} | EXP: {user.Experience:N0}]", LogLvl.INFO);

                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} has leveled up.");

                
                // Don't send announcement if the channel is blacklisted, but only if it's not a level-announcements log channel.
                if (server.BlackListedChannels.Any(x => x.ChannelId == context.Channel.Id && x.ChannelId != server.LogLevelAnnouncements))
                    return;
                
                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} iterated through blacklisted channels.");

                if (!server.LevelAnnouncementsEnabled)
                    return;

                var xp = new XpImage();
                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} xp image created.");

                if (user.ExpChatNotificationType == ExpType.GLOBAL || user.ExpChatNotificationType == ExpType.BOTH)
                {
                    if (levelAnnouncementChannel != null)
                    {
                        Stream xpStream = await xp.GenerateXpImageStream(user, (SocketGuildUser) context.User);
                        await levelAnnouncementChannel.SendFileAsync(xpStream, $"Kaguya_Xp_LevelUp.png", "");
                    }
                }
                
                if (user.ExpDmNotificationType == ExpType.GLOBAL || user.ExpDmNotificationType == ExpType.BOTH)
                {
                    Stream xpStream = await xp.GenerateXpImageStream(user, (SocketGuildUser) context.User);
                    await context.User.SendFileAsync(xpStream, $"Kaguya_Xp_LevelUp.png", "");
                }
                
                Console.WriteLine($"{sw.ElapsedMilliseconds} User {context.User} sent level up notification.");
            });
        }

        private static bool CanGetExperience(User user)
        {
            double twoMinutesAgo = DateTime.Now.AddSeconds(-120).ToOADate();

            return twoMinutesAgo >= user.LastGivenExp;
        }

        private static double ReturnLevel(User user) => GlobalProperties.CalculateLevelFromExp(user.Experience);
        private static bool HasLeveledUp(double oldLevel, double newLevel) => Math.Floor(oldLevel) < Math.Floor(newLevel);
    }
}