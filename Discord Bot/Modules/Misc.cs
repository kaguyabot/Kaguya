using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core.UserAccounts;

#pragma warning disable

namespace Discord_Bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();

        public Color Pink = new Color(252, 132, 255);

        public string version = Utilities.GetAlert("VERSION");

        public string cmdPrefix = Config.bot.cmdPrefix;

        public async Task BE()
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("h")] //The BIG fish
        public async Task Help([Remainder]string command)
        {
            switch(command.ToLower())
            {
                    
                case "expadd":
                    embed.WithTitle($"Help: Adding experience points");
                    embed.WithDescription($"**Permissions Required: Administrator**" +
                        $"\n" +
                        $"\n{Context.User.Mention} Syntax: `{cmdPrefix}expadd <number of experience points to add>`. The number of exp you are adding must be an integer.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "points":
                    embed.WithTitle($"Help: Points");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}points`." +
                        $"\nReturns the value of points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "exp":
                    embed.WithTitle("Help: EXP");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}exp`." +
                        $"\nReturns the value of experience points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
            }
        }

        [Command("help")]
        public async Task Help()
        {
            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync("u thought");
        }

        [Command("modules")]
        public async Task ModulesList()
        {
            Context.Channel.SendMessageAsync
                ("```css" +
                $"\nStageBot Modules for {version}:" +
                $"\nType {cmdPrefix}cmds <module> for a list of commands in said module." +
                $"\n" +
                $"\n{cmdPrefix}administration" +
                $"\n{cmdPrefix}exp" +
                $"\n{cmdPrefix}currency" +
                $"\n{cmdPrefix}utility" +
                "\n```");
        }

        [Command("cmds")]
        public async Task ModulesList([Remainder]string category)
        { 
            if (category == "administration".ToLower() || category == "admin".ToLower())
            {
                embed.WithTitle("Module: Administration");
                embed.WithDescription("```css" +
                        "\nAll commands in category: Administration" +
                        "\n" +
                        $"\n{cmdPrefix} Commands coming soon." +
                        $"\n" +
                        $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                        "\n```");
                embed.WithColor(Pink);
                BE();
            }
            else if (category == "exp".ToLower())
            {
                embed.WithTitle("Module: EXP");
                embed.WithDescription
                    ("```css" +
                    "\nAll commands in category: Experience Points" +
                    "\n" +
                    $"\n{cmdPrefix}exp" +
                    $"\n{cmdPrefix}expadd" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    "\n```");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("createtextchannel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GuildCreateTextChannel([Remainder]string name)
        {
            var channel = await Context.Guild.CreateTextChannelAsync(name);
            embed.WithDescription($"{Context.User.Mention} has successfully created the channel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("deletetextchannel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GuildDeleteTextChannel([Remainder]string name)
        {
            foreach (var Channel in Context.Guild.TextChannels) { if (Channel.Name == (name.ToLower())) { await Channel.DeleteAsync(); } }
            embed.WithDescription($"{Context.User.Mention} has successfully deleted the text channel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("timely")]
        public async Task DailyPoints()
        {
            uint bonus = EditableCommands.bot.timelyPoints;
            uint hours = EditableCommands.bot.timelyHours;
            var account = UserAccounts.GetAccount(Context.User);
            embed.WithTitle("Timely Points");
            embed.WithDescription($"{Context.User.Mention} has received {bonus} points! Claim again in {hours}h.");
            embed.WithColor(Pink);
            account.Points += bonus;
            UserAccounts.SaveAccounts();
            BE();
        }

        [Command("exp")]
        public async Task EXP()
        {
            var account = UserAccounts.GetAccount(Context.User);
            embed.WithTitle("Experience Points");
            embed.WithDescription($"You have {account.EXP} EXP.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("points")]
        public async Task Points([Remainder]string arg = "")
        {
            SocketUser target = null;
            var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = mentionedUser ?? Context.User;
            var account = UserAccounts.GetAccount(target);
            embed.WithTitle("Points");
            embed.WithDescription($"{target.Mention} has {account.Points} points.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("pointsadd")]
        [RequireUserPermission(GuildPermission.Administrator)]

        public async Task PointsAdd([Remainder]uint points)
        {
            if (!UserIsAdmin((SocketGuildUser)Context.User))
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription("Required Permission Missing: `Administrator`.");
                embed.WithColor(252, 132, 255);
                await Context.Channel.SendMessageAsync(Context.User.Mention + ":x: You are not an administrator.");
                return;
            }
            var account = UserAccounts.GetAccount(Context.User);
            account.Points += points;
            UserAccounts.SaveAccounts();
            embed.WithTitle("Adding Points");
            embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("expadd")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ExpAdd([Remainder]uint exp)
        {
            if (!UserIsAdmin((SocketGuildUser)Context.User))
            {
                embed.WithTitle("Adding Experience Points");
                embed.WithDescription("Required Permission Missing: `Administrator`.");
                embed.WithColor(252, 132, 255);
                await Context.Channel.SendMessageAsync(Context.User.Mention + ":x: You are not an administrator.");
                return;
            }
            var account = UserAccounts.GetAccount(Context.User);
            account.EXP += exp;
            UserAccounts.SaveAccounts();
            embed.WithTitle("Adding Experience Points");
            embed.WithDescription($"{Context.User.Mention} has gained {exp} EXP.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("echo")]
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Echoed message");
            embed.WithDescription(message);
            embed.WithColor(Pink);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("pick")]
        public async Task PickOne([Remainder]string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            var embed = new EmbedBuilder();
            embed.WithTitle("Choice for " + Context.User.Username);
            embed.WithDescription(selection);
            embed.WithColor(Pink);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("secret")]
        public async Task RevealSecret([Remainder]string arg = "")
        {
            if (!UserIsLolicon((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ":x: You have insufficient permissions to execute this command.");
                return;
            }
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(Utilities.GetAlert("SECRET"));
        }
        
        private bool UserIsLolicon(SocketGuildUser user)
        {
            string targetRoleName = "Lolicon";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
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
