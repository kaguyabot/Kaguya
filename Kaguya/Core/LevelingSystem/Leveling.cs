using Discord;
using Discord.WebSocket;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;

namespace Kaguya.Core.LevelingSystem
{
    internal static class Leveling
    {

        internal static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel)
        {
            Color Pink = new Color(252, 132, 255);
            try
            {
                UserAccount userAccount = UserAccounts.UserAccounts.GetAccount(user);
                Random RNGTimeout = new Random();
                Logger logger = new Logger();
                if (!CanReceiveExperience(userAccount, RNGTimeout.Next(110, 130)))
                    return;
                uint oldLevel = userAccount.LevelNumber;
                Random random = new Random();
                uint newExp = (uint)random.Next(5, 8);
                userAccount.EXP += newExp;
                userAccount.LastReceivedEXP = DateTime.Now;
                UserAccounts.UserAccounts.SaveAccounts();
                uint newLevel = userAccount.LevelNumber;
                Server guild = Servers.GetServer(channel.Guild);
                if (oldLevel != userAccount.LevelNumber && guild.MessageAnnouncements == true)
                {
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithDescription($"**{user.Nickname} [{user.Username}#{user.Discriminator}] just leveled up!**" +
                        $"\nLevel: {userAccount.LevelNumber.ToString("N0")} | EXP: {userAccount.EXP.ToString("N0")}");
                    embed.WithColor(Pink);
                    if(guild.LogLevelUpAnnouncements == 0)
                    await channel.SendMessageAsync("", false, embed.Build());
                    else if(guild.LogLevelUpAnnouncements != 0) //If the server has a specified channel for level up announcements, send it there instead of in the chat the user leveled up in.
                    {
                        var textChannel = Global.Client.GetChannel(guild.LogLevelUpAnnouncements);
                        await (textChannel as ITextChannel).SendMessageAsync("", false, embed.Build());
                    }
                    logger.ConsoleGuildAdvisory(channel.Guild, $"User {user.Username}#{user.Discriminator} leveled up to level {userAccount.LevelNumber}.");
                }
                else return;
            }
            catch (Discord.Net.HttpException)
            {
                Console.WriteLine($"Failed to embed message (Leveling.cs) Guild: {channel.Guild.Name} Channel: {channel.Name}. Possibly attempted to send leveling message to " +
                    "a channel that the bot cannot access.");
            }
            catch (Exception)
            {
                return;
            }
        }

        internal static bool CanReceiveExperience(UserAccount user, int timeout)
        {
            var difference = DateTime.Now - user.LastReceivedEXP;
            return difference.TotalSeconds > timeout;
        }
    }
}
