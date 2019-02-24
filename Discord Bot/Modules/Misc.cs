using System;
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

#pragma warning disable

namespace Discord_Bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();

        public Color Pink = new Color(252, 132, 255);

        public string version = Utilities.GetAlert("VERSION");

        public string cmdPrefix = Config.bot.cmdPrefix;

        public string botToken = Config.bot.token;

        public async Task BE() //Method to build an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
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
                $"\n{cmdPrefix}fun" +
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
                        $"\n{cmdPrefix}kick" +
                        $"\n{cmdPrefix}ban" +
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
            else if (category == "currency".ToLower())
            {
                embed.WithTitle("Module: Currency");
                embed.WithDescription
                ("```css" +
                "\nAll commands in category: Currency" +
                "\n" +
                $"\n{cmdPrefix}points" +
                $"\n{cmdPrefix}pointsadd" +
                $"\n{cmdPrefix}timely" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```"
                );
            }
            else if (category == "utility".ToLower())
            {
                embed.WithTitle("Module: Utility");
                embed.WithDescription
                ("```css" +
                "\nAll commands in category: Utility" +
                "\n" +
                $"\n{cmdPrefix}createtextchannel" +
                $"\n{cmdPrefix}deletetextchannel" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n" +
                $"\n```");
                embed.WithColor(Pink);
                BE();
            }
            else if (category == "fun".ToLower())
            {
                embed.WithTitle("Module: Fun");
                embed.WithDescription("```css" +
                    "\n" +
                    $"\n{cmdPrefix}echo" +
                    $"\n{cmdPrefix}pick" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n" +
                    $"\n```");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("h")] //The BIG fish
        public async Task Help([Remainder]string command)
        {
            switch (command.ToLower())
            {
                case "exp":
                    embed.WithTitle("Help: EXP");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}exp`." +
                        $"\nReturns the value of experience points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "expadd":
                    embed.WithTitle("Help: Adding experience points");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\n{Context.User.Mention} Syntax: `{cmdPrefix}expadd <number of experience points to add>`. The number of exp you are adding must be a positive whole number.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "points":
                    embed.WithTitle("Help: Points");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}points`." +
                        $"\nReturns the value of points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "pointsadd":
                    embed.WithTitle("Help: Adding Points");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\n{Context.User.Mention} Syntax: `{cmdPrefix}pointsadd <number of points to add>`. The number of points you are adding must be a positive whole number.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "createtextchannel":
                    embed.WithTitle("Help: Creating Text Channels");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a text channel with the speficied name. Syntax: `{cmdPrefix}createtextchannel <channel name>`. " +
                        $"\nThis name can have spaces. Example: `{cmdPrefix}createtextchannel testing 123`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "deletetextchannel":
                    embed.WithTitle("Help: Deleting Text Channels");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a text channel with the speficied name. Syntax: `{cmdPrefix}deletetextchannel <channel name>`. " +
                        $"\nThis name can **not** have spaces. Type the text channel exactly as displayed; If the text channel contains a `-`, type that in." +
                        $"Example: `{cmdPrefix}deletetextchannel super-long-name-with-lots-of-spaces`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "echo":
                    embed.WithTitle("Help: Echoed Messages");
                    embed.WithDescription($"{Context.User.Mention} Makes the bot repeat anything you say!" +
                        $"\nSyntax: `{cmdPrefix}echo <message>`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "pick":
                    embed.WithTitle("Help: Pick");
                    embed.WithDescription($"{Context.User.Mention} Tells the bot to pick between any amount of options, randomly." +
                        $"\nSyntax: `{cmdPrefix}pick option1|option2|option3|option4`...etc." +
                        $"\nYou may have as many \"Options\" as you'd like!" +
                        $"\nThe bot will always pick with totally random odds.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "timely":
                    embed.WithTitle("Help: Timely Points");
                    embed.WithDescription($"{Context.User.Mention} The timely command allows any user to claim 500 free points every 24 hours." +
                        "\nThese points are added to your StageBot account." +
                        "\nIf you are in a server with a self-hosted version of StageBot, these values may be different." +
                        $"\nSyntax: `{cmdPrefix}timely`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "clear":
                case "purge":
                    embed.WithTitle("Help: Clearing Messages");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Messages**" +
                        $"\n" +
                        $"\nDeletes a specified number of messages in a given channel." +
                        $"\nSyntax: `{cmdPrefix}clear 25`" +
                        $"\nSyntax: `{cmdPrefix}prune 25`" +
                        $"\nThis number may not exceed `100`." +
                        $"\nMessages older than two weeks will need to be deleted manually.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "kick":
                    embed.WithTitle("Help: Kicking users");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Kick Members**" +
                        $"\n" +
                        $"\nKicks an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}kick @User#0000`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "ban":
                    embed.WithTitle("Help: Banning users");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nBans an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}ban @User#0000`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "removeallroles":
                case "rar":
                    embed.WithTitle("Help: Removing All Roles");
                    embed.WithDescription($"{Context.User.Mention}**Permissions Required: Manage Roles**" +
                        "\n" +
                        "\nRemoves all roles from the specified user." +
                        $"\nSyntax: `{cmdPrefix}removeallroles @User#0000`.");
                    embed.WithColor(Pink);
                    BE(); break;
            }
        }

        [Command("h")]
        public async Task Help()
        {
            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync("Add me to your server with this link!: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8" +
                $"\nNeed help with a specific command? Type `{cmdPrefix}h <command name>` for more information on said command." +
                $"\nAre you a self hoster? Search for the \"README.txt\" file inside of the bot's files." +
                $"\nStill need help? Feel free to join the StageBot Development server and ask for help there!: https://discord.gg/yhcNC97");
        }

        [Command("h")] //isolated help command, "h". Same thing as below.
        public async Task HelpDMISO()
        {
            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync("Add me to your server with this link!: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8" +
                $"\nNeed help with a specific command? Type `{cmdPrefix}h <command name>` for more information on said command." +
                $"\nAre you a self hoster? Search for the \"README.txt\" file inside of the bot's files." +
                $"\nStill need help? Feel free to join the StageBot Development server and ask for help there!: https://discord.gg/yhcNC97");
        }

        [Command("help")] //same as above just uses "help" instead
        public async Task HelpDM()
        {
            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync("Add me to your server with this link!: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8" +
                $"\nNeed help with a specific command? Type `{cmdPrefix}h <command name>` for more information on said command." +
                $"\nAre you a self hoster? Search for the \"README.txt\" file inside of the bot's files." +
                $"\nStill need help? Feel free to join the StageBot Development server and ask for help there!: https://discord.gg/yhcNC97");
        }

        [Command("removeallroles")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveAllRoles(IGuildUser user)
        {
            await RemoveAllRoles(user);
            embed.WithTitle("Remove All Roles");
            embed.WithDescription($"All roles have been removed from `{user}`.");
            embed.WithColor(Pink);
        }

        [Command("rar")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RAR(IGuildUser user)
        {
            await RemoveAllRoles(user);
            embed.WithTitle("Remove All Roles");
            embed.WithDescription($"All roles have been removed from `{user}`.");
            embed.WithColor(Pink);
        }

        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, string reason = "No reason provided.")
        {
            if (reason != "No reason provided.")
            {
                await user.KickAsync(reason);
                embed.WithTitle($"User Kicked");
                embed.WithDescription($"`{Context.User.Mention}` has kicked `{user}` with reason: \"{reason}\"");
                embed.WithColor(Pink);
                BE();
            }
            else
            {
                await user.KickAsync(reason);
                embed.WithTitle($"User Kicked");
                embed.WithDescription($"`{Context.User.Mention}` has kicked `{user}` without a specified reason.");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, string reason = "No reason provided.")
        {
            if (reason != "No reason provided.")
            {
                await user.BanAsync(0, reason);
                embed.WithTitle($"User Banned");
                embed.WithDescription($"{Context.User.Mention} has banned `{user}` with reason: \"{reason}\"");
                embed.WithColor(Pink);
                BE();
            }
            else
            {
                await user.BanAsync(0, reason);
                embed.WithTitle($"User Banned");
                embed.WithDescription($"{Context.User.Mention} has banned `{user}` without a specified reason.");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("clear")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessages([Remainder]uint amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync(); //defualt is 100
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            const int delay = 5000;
            var m = await this.ReplyAsync($"Clearing of messages completed. This message will be deleted in {delay / 1000} seconds.");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeMessages([Remainder]uint amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync(); //defualt is 100
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            const int delay = 5000;
            var m = await this.ReplyAsync($"Clearing of messages completed. This message will be deleted in {delay / 1000} seconds.");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

        [Command("createtextchannel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateTextChannel([Remainder]string name)
        {
            var channel = await Context.Guild.CreateTextChannelAsync(name);
            embed.WithDescription($"{Context.User.Mention} has successfully created the channel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("deletetextchannel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
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
        [RequireOwner]
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
        [RequireOwner]
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
            embed.WithTitle("Echo");
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
