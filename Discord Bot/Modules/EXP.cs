﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core.UserAccounts;
using System.Net;
using System.Timers;
using Discord_Bot.Core.Server_Files;
using Discord_Bot.Core.Commands;

#pragma warning disable

namespace Discord_Bot.Modules
{
    public class EXP : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();

        public Color Pink = new Color(252, 132, 255);

        public Color Red = new Color(255, 0, 0);

        public Color Gold = new Color(255, 223, 0);

        public BotConfig bot = new BotConfig();

        public string version = Utilities.GetAlert("VERSION");

        public string botToken = Config.bot.token;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("expadd")] //exp
        [Alias("addexp")]
        [RequireOwner]
        public async Task ExpAdd([Remainder]uint exp)
        {
            var account = UserAccounts.GetAccount(Context.User);
            account.EXP += exp;
            UserAccounts.SaveAccounts();
            embed.WithTitle("Adding Experience Points");
            embed.WithDescription($"{Context.User.Mention} has gained {exp} EXP.");
            embed.WithColor(Pink);
            BE();
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
            embed.WithColor(Pink);
            BE();
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
            embed.WithColor(Pink);
            BE();
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
                embed.WithColor(Red);
                BE(); return;
            }
            else
            {
                userAccount.Rep++;
                commandUserAcc.LastGivenRep = DateTime.Now;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"{Context.User.Username}#{Context.User.Discriminator} has given +1 rep to {userAccount.Username}");
                embed.WithTitle("+Rep Author");
                embed.WithDescription("**Successfully gave +1 rep to my creator** uwu.");
                embed.WithFooter("Thank you for showing your support <3");
                embed.WithColor(Pink);
                BE(); return;
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
                embed.WithColor(Red);
                BE(); return;
            }
            if (userAccount == targetAccount)
            {
                embed.WithTitle("Rep");
                embed.WithDescription($"**{Context.User.Mention} You may not rep yourself!**");
                embed.WithColor(Red);
                BE(); return;
            }
            else
            {
                targetAccount.Rep++;
                userAccount.LastGivenRep = DateTime.Now;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Rep");
                embed.WithDescription($"**{Context.User.Mention} Successfully gave rep to {user.Mention}!**");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("exp")] //exp
        public async Task EXPCommand()
        {
            var account = UserAccounts.GetAccount(Context.User);
            embed.WithTitle("Experience Points");
            embed.WithDescription($"{Context.User.Mention} has {account.EXP} EXP.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("level")] //exp
        public async Task Level()
        {
            var account = UserAccounts.GetAccount(Context.User);
            embed.WithTitle("Level");
            embed.WithDescription($"{Context.User.Mention} you have {account.LevelNumber} levels.");
            embed.WithColor(Pink);
            BE();
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
