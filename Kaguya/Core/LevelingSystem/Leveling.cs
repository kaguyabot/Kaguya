using Discord;
using Discord.WebSocket;
using Kaguya.Core.CommandHandler;
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
                uint newExp = (uint)random.Next(7, 11);
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
                    await channel.SendMessageAsync("", false, embed.Build());
                    logger.ConsoleGuildAdvisory(channel.Guild, $"User {user.Username}#{user.Discriminator} leveled up to level {userAccount.LevelNumber}.");
                }
                else return;
            }
            catch (Discord.Net.HttpException)
            {
                Console.WriteLine($"Failed to embed message (Leveling.cs) Guild: {channel.Guild.Name} Channel: {channel.Name}. Possibly attempted to send leveling message to " +
                    "a channel that the bot cannot access.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await EmbedHandler.CreateErrorEmbed("Leveling Exception", "Exception thrown when processing a leveling command." + //Sends response to user
                    $"\nException: `{e.Message}` \n`Leveling.cs Line 51`", "This has been automatically reported as a bug and will be fixed as soon as possible.");
                await EmbedHandler.CreateAutomaticBugReport("Leveling Exception: Leveling.cs Line 51", "Exception thrown when processing a leveling command." + //Sends bug to Kaguya Support Server.
                    $"\nException Message: {e.Message}");
            }
        }

        internal static bool CanReceiveExperience(UserAccount user, int timeout)
        {
            var difference = DateTime.Now - user.LastReceivedEXP;
            return difference.TotalSeconds > timeout;
        }
    }
}
