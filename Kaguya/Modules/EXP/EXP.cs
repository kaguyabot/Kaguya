using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.UserAccounts;
using Kaguya.Core;
using System.Diagnostics;
using Kaguya.Core.Embed;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules
{
    public class EXP : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("serverexplb")] //exp
        [Alias("explb")]
        public async Task ServerEXPLeaderboard()
        {
            List<UserAccount> userAccounts = new List<UserAccount>();
            var users = Context.Guild.Users;

            foreach (var user in users)
            {
                var allAccs = UserAccounts.GetAccount(user);
                userAccounts.Add(allAccs);
            }

            var users10 = userAccounts.OrderByDescending(u => u.EXP).Take(10);
            embed.WithTitle("Server EXP Leaderboard");
            int i = 1;
            foreach (var user in users10)
            {
                embed.AddField($"#{i++} {user.Username}", $"Level: {user.LevelNumber} - EXP: {user.EXP}");
            }
            await BE();
        }

        [Command("globalexplb")] //exp
        [Alias("gexplb")]
        public async Task GlobalEXPLeaderboard()
        {
            var users = UserAccounts.GetAllAccounts();
            var users10 = users.OrderByDescending(u => u.EXP).Take(10);
            embed.WithTitle("Kaguya Global EXP Leaderboard");
            int i = 1;
            foreach (var user in users10)
            {
                embed.AddField($"#{i++} {user.Username}", $"Level: {user.LevelNumber} - EXP: {user.EXP}");
            }
            await BE();
        }

        [Command("repauthor")] //exp
        [Alias("rep author")]
        public async Task RepAuthor(int timeout = 24)
        {
            var userAccount = UserAccounts.GetAuthor();
            var commandUserAcc = UserAccounts.GetAccount(Context.User);
            var difference = DateTime.Now - commandUserAcc.LastGivenRep;

            if (difference.TotalHours < timeout)
            {
                embed.WithTitle("Rep");
                embed.WithDescription($"{Context.User.Mention} you must wait `{(int)(23 - difference.Hours)}h " +
                    $"{(int)(59 - difference.Minutes)}m {(int)(59 - difference.Seconds)}s` " +
                    $"before you can give rep again!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }
            else
            {
                userAccount.Rep++;
                commandUserAcc.LastGivenRep = DateTime.Now;
                Console.WriteLine($"{Context.User.Username}#{Context.User.Discriminator} has given +1 rep to {userAccount.Username}");
                embed.WithTitle("+Rep Author");
                embed.WithDescription("Successfully gave +1 rep to my creator uwu.");
                await BE();
            }
        }

        [Command("rep")] //exp
        public async Task Rep(IGuildUser user, int timeout = 24)
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            var targetAccount = UserAccounts.GetAccount((SocketUser)user);
            var difference = DateTime.Now - userAccount.LastGivenRep;
            if (difference.TotalHours < timeout)
            {
                embed.WithTitle("Rep");
                embed.WithDescription($"{Context.User.Mention} you must wait `{(int)(23 - difference.Hours)}h {(int)(59 - difference.Minutes)}m {(int)(60 - difference.Seconds)}s` " +
                    $"before you can give rep again!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }
            if (userAccount == targetAccount)
            {
                embed.WithTitle("Rep");
                embed.WithDescription($"**{Context.User.Mention} You may not rep yourself!**");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }
            else
            {
                targetAccount.Rep++;
                userAccount.LastGivenRep = DateTime.Now;
                embed.WithTitle("Rep");
                embed.WithDescription($"**{Context.User.Mention} Successfully gave rep to {user.Mention}!**");
                await BE();
                return;
            }
        }

        [Command("rep")]
        public async Task Rep()
        {
            var user = Context.User;
            var userAccount = UserAccounts.GetAccount(user);

            embed.WithDescription($"{user.Mention} You have `{userAccount.Rep}` rep!");
            await BE();
        }

        [Command("exp")] //exp
        public async Task EXPCommand(IGuildUser user = null)
        {
            if(user is null)
                user = Context.User as IGuildUser;

            var account = UserAccounts.GetAccount((SocketUser)user);
            var futureEXP = 8 * (Math.Pow(account.LevelNumber + 1, 2) + 8);
            var expRemaining = 8 * (Math.Pow(account.LevelNumber + 1, 2) + 8) - account.EXP;

            TimeSpan time = TimeSpan.FromSeconds(expRemaining * 120 / 6.5);

            embed.WithTitle("Experience Points");
            embed.WithDescription($"{user.Mention} is level `{account.LevelNumber}` and has " +
                $"`{account.EXP.ToString("N0")} / {futureEXP.ToString("N0")}` EXP." +
                $"\nEXP Needed for `Level {account.LevelNumber + 1}: " +
                $"{expRemaining.ToString("N0")}`" +
                $"\nETA: `{time.ToString(@"hh\h\ mm\m\ ss\s")} of active chatting`");
            await BE();
        }

        private bool UserIsAdmin(SocketGuildUser user)
        {
            string targetRoleName = "Administrator";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
    }
}
