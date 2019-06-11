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
using Kaguya.Core.Attributes;

namespace Kaguya.Modules
{
    [KaguyaModule("EXP")]
    public class EXP : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("expadd")] //exp
        [Alias("addexp")]
        [RequireOwner]
        public async Task ExpAdd(int exp)
        {
            var account = UserAccounts.GetAccount(Context.User);

            if (exp > 0)
                account.EXP += (uint)exp;
            else if (exp < 0)
                account.EXP -= (uint)exp;

            UserAccounts.SaveAccounts();
            embed.WithTitle("Adding Experience Points");
            embed.WithDescription($"{Context.User.Mention} has gained {exp} EXP.");
            await BE();
        }

        [Command("expadd")] //exp
        [Alias("addexp")]
        [RequireOwner]
        public async Task ExpAdd(int exp, [Remainder]IGuildUser user)
        {
            var account = UserAccounts.GetAccount(user as SocketUser);

            if (exp > 0)
                account.EXP += (uint)exp;
            else if (exp < 0)
                account.EXP -= (uint)exp;

            UserAccounts.SaveAccounts();
            embed.WithTitle("Adding Experience Points");
            embed.WithDescription($"{Context.User.Mention} has gained {exp} EXP.");
            await BE();
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

            UserAccounts.SaveAccounts();
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
            UserAccounts.SaveAccounts();
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
                embed.WithDescription($"**{Context.User.Mention} you must wait {(int)(24 - difference.TotalHours)}h {(int)(60 - difference.TotalMinutes)}m {(int)(60 - difference.Seconds)}s " +
                    $"before you can give rep again!**");
                embed.SetColor(EmbedColor.RED);
                await BE();
                // logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, $"User must wait {(int)(24 - difference.TotalHours)} more hours before awarding more reputation."); return;
            }
            else
            {
                userAccount.Rep++;
                commandUserAcc.LastGivenRep = DateTime.Now;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"{Context.User.Username}#{Context.User.Discriminator} has given +1 rep to {userAccount.Username}");
                embed.WithTitle("+Rep Author");
                embed.SetColor(EmbedColor.RED);
                embed.WithDescription("**Successfully gave +1 rep to my creator** uwu.");
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
                embed.WithDescription($"**{Context.User.Mention} you must wait {(int)(24 - difference.TotalHours)}h {(int)(60 - difference.Minutes)}m {(int)(60 - difference.Seconds)} " +
                    $"before you can give rep again!**");
                embed.SetColor(EmbedColor.RED);
                await BE();
                // logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, $"User must wait {(int)(24 - difference.TotalHours)} more hours before awarding more reputation."); return;
            }
            if (userAccount == targetAccount)
            {
                embed.WithTitle("Rep");
                embed.WithDescription($"**{Context.User.Mention} You may not rep yourself!**");
                embed.SetColor(EmbedColor.RED);
                await BE();
                //logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User attempted to rep themselves."); return;
            }
            else
            {
                targetAccount.Rep++;
                userAccount.LastGivenRep = DateTime.Now;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Rep");
                embed.WithDescription($"**{Context.User.Mention} Successfully gave rep to {user.Mention}!**");
                await BE();
                // logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
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

            var account = UserAccounts.GetAccount(user as SocketUser);
            embed.WithTitle("Experience Points");
            embed.WithDescription($"{Context.User.Mention} has `{account.EXP.ToString("N0")}` EXP.");
            await BE();
        }

        [Command("level")] //exp
        public async Task Level()
        {
            var account = UserAccounts.GetAccount(Context.User);
            embed.WithTitle("Level");
            embed.WithDescription($"{Context.User.Mention} you are level: {account.LevelNumber}");
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
