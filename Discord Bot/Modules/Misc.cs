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
                $"\n{cmdPrefix}osu" +
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
                        $"\n{cmdPrefix}kick [k]" +
                        $"\n{cmdPrefix}ban [b]" +
                        $"\n{cmdPrefix}masskick" +
                        $"\n{cmdPrefix}massban" +
                        $"\n{cmdPrefix}removeallroles [rar]" +
                        $"\n{cmdPrefix}deleterole [dr]" +
                        $"\n{cmdPrefix}clear [c] [purge]" +
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
                $"\n{cmdPrefix}help [h]" +
                $"\n{cmdPrefix}createtextchannel [ctc]" +
                $"\n{cmdPrefix}deletetextchannel [dtc]" +
                $"\n{cmdPrefix}createvoicechannel [cvc]" +
                $"\n{cmdPrefix}deletevoicechannel [dvc]" +
                $"\n{cmdPrefix}changelog" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
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
            else if (category == "osu".ToLower())
            {
                embed.WithTitle("Module: osu!");
                embed.WithDescription("```css" +
                    "\n" +
                    $"\n{cmdPrefix}createteamrole [ctr]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n```");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("h")] //The BIG fish
        [Alias("help")]
        public async Task Help([Remainder]string command)
        {
            switch (command.ToLower())
            {
                case "exp":
                    embed.WithTitle($"Help: EXP | `{cmdPrefix}exp`");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}exp`." +
                        $"\nReturns the value of experience points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "expadd":
                    embed.WithTitle($"Help: Adding Experience Points | `{cmdPrefix}expadd`");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\n{Context.User.Mention} Syntax: `{cmdPrefix}expadd <number of experience points to add>`. The number of exp you are adding must be a positive whole number.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "points":
                    embed.WithTitle($"Help: Points | `{cmdPrefix}points`");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}points`." +
                        $"\nReturns the value of points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "pointsadd":
                    embed.WithTitle($"Help: Adding Points | `{cmdPrefix}pointsadd`");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\n{Context.User.Mention} Syntax: `{cmdPrefix}pointsadd <number of points to add>`. The number of points you are adding must be a positive whole number.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "createtextchannel":
                case "ctc":
                    embed.WithTitle($"Help: Creating Text Channels | `{cmdPrefix}createtextchannel`, `{cmdPrefix}ctc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a text channel with the speficied name. " +
                        $"\nSyntax: `{cmdPrefix}createtextchannel <channel name>`. " +
                        $"\nThis name can have spaces. Example: `{cmdPrefix}createtextchannel testing 123`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "deletetextchannel":
                case "dtc":
                    embed.WithTitle($"Help: Deleting Text Channels | `{cmdPrefix}deletetextchannel`, `{cmdPrefix}dtc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a text channel with the speficied name. " +
                        $"\nThis name can **not** have spaces. Type the text channel exactly as displayed; If the text channel contains a `-`, type that in." +
                        $"\nSyntax: `{cmdPrefix}deletetextchannel <channel name>`." +
                        $"Example: `{cmdPrefix}deletetextchannel super-long-name-with-lots-of-spaces`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "createvoicechannel":
                case "cvc":
                    embed.WithTitle($"Help: Creating Voice Channels | `{cmdPrefix}createvoicechannel`, `{cmdPrefix}cvc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a voice channel with the speficied name. Syntax: `{cmdPrefix}createvoicechannel <channel name>`. " +
                        $"\nThis name can have spaces." +
                        $"\nSyntax: `{cmdPrefix}createvoicechannel <channel name>`." +
                        $"\nExample: `{cmdPrefix}cvc testing 123`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "deletevoicechannel":
                case "dvc":
                    embed.WithTitle($"Help: Deleting Voice Channels | `{cmdPrefix}deletevoicechannel`, `{cmdPrefix}dvc");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a voice channel with the speficied name. " +
                        $"\nThis name can **not** have spaces. Type the text channel exactly as displayed; If the text channel contains a `-`, type that in." +
                        $"\nSyntax: `{cmdPrefix}deletevoicechannel <channel name>`." +
                        $"Example: `{cmdPrefix}deletevoicechannel super-long-name-with-lots-of-spaces`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "echo":
                    embed.WithTitle($"Help: Echoed Messages | `{cmdPrefix}echo`");
                    embed.WithDescription($"{Context.User.Mention} Makes the bot repeat anything you say!" +
                        $"\nSyntax: `{cmdPrefix}echo <message>`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "pick":
                    embed.WithTitle($"Help: Pick | `{cmdPrefix}pick");
                    embed.WithDescription($"{Context.User.Mention} Tells the bot to pick between any amount of options, randomly." +
                        $"\nSyntax: `{cmdPrefix}pick option1|option2|option3|option4`...etc." +
                        $"\nYou may have as many \"Options\" as you'd like!" +
                        $"\nThe bot will always pick with totally random odds.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "timely":
                    embed.WithTitle($"Help: Timely Points | `{cmdPrefix}timely");
                    embed.WithDescription($"{Context.User.Mention} The timely command allows any user to claim free points every certain amount hours." +
                        "\nThese points are added to your StageBot account." +
                        "\nIf you are in a server with a self-hosted version of StageBot, these values may be different." +
                        $"\nSyntax: `{cmdPrefix}timely`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "clear":
                case "purge":
                case "c":
                    embed.WithTitle($"Help: Clearing Messages | `{cmdPrefix}clear`, `{cmdPrefix}purge`, `{cmdPrefix}c`");
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
                case "k":
                    embed.WithTitle($"Help: Kicking Users | `{cmdPrefix}kick`, `{cmdPrefix}k`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Kick Members**" +
                        $"\n" +
                        $"\nKicks an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}kick @User#0000`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "ban":
                case "b":
                    embed.WithTitle($"Help: Banning Users | `{cmdPrefix}ban`, `{cmdPrefix}b`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nBans an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}ban @User#0000`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "removeallroles":
                case "rar":
                    embed.WithTitle($"Help: Removing All Roles | `{cmdPrefix}removeallroles`, `{cmdPrefix}rar`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        "\n" +
                        "\nRemoves all roles from the specified user." +
                        $"\nSyntax: `{cmdPrefix}removeallroles @User#0000`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "deleterole":
                case "dr":
                    embed.WithTitle($"Help: Deleting Roles | `{cmdPrefix}deleterole`, `{cmdPrefix}dr`");
                    embed.WithDescription($"{ Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nDeletes a role from the server (and in the process, removes said role from everyone who had it)." +
                        $"\nSyntax: `{cmdPrefix}deleterole/dr <role name>`");
                    BE(); break;
                case "createteamrole":
                case "ctr":
                    embed.WithTitle($"Help: Create Team Roles | `{cmdPrefix}createteamrole`, `{cmdPrefix}ctr`");
                    embed.WithDescription($"{Context.User.Mention}**Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nCreates a role, then applies it to all mentioned users." +
                        $"\nThis is very ideal for managing many groups of people (such as teams in a tournament, hence the name)." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}createteamrole <role name> <mentioned users>`" +
                        $"\nExample: `{cmdPrefix}createteamrole Smelly Sushi @user1#0000 @smellyfish#2100 @smellysushilover#9999`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "changelog":
                    embed.WithTitle($"Help: Changelog | `{cmdPrefix}changelog`");
                    embed.WithDescription($"{Context.User.Mention} Displays the most recent StageBot changelog. Find out what's new!");
                    embed.WithColor(Pink);
                    BE(); break;

                default:
                    embed.WithDescription($"**{Context.User.Mention} \"{command}\" is not a valid command.**");
                    embed.WithColor(Pink);
                    BE(); break;
            }
        }

        [Command("help")] //utility, pinned
        [Alias("h")]
        public async Task Help()
        {
            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync("Need help with a specific command? Type `{cmdPrefix}h <command name>` for more information on said command." +
                $"\nAdd me to your server with this link!: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8" +
                $"\nWant to keep track of all the changes or feel like self hosting? Feel free to check out the StageBot Github page!: https://github.com/stageosu/StageBot" +
                $"\nStill need help? Feel free to join the StageBot Development server and ask for help there!: https://discord.gg/yhcNC97");
        }

        [Command("changelog")] //utility, pinned
        public async Task ChangeLog()
        {
            await Context.User.SendMessageAsync($"Changelog for StageBot {version}:" +
                $"\n" +
                $"\n");
        }

        [Command("createteamrole")] //osu
        [Alias("ctr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task CreateTeamRoles(string teamName, [Remainder]List<SocketGuildUser> users)
        {
            var teamRole = await Context.Guild.CreateRoleAsync(teamName);
            foreach (var user in users)
            {
                await user.AddRoleAsync(teamRole);
                await ReplyAsync($"{user} has been added to {teamRole.Mention}.");
            }
        }

        [Command("removeallroles")] //admin
        [Alias("rar")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveAllRoles(IGuildUser user)
        {
            await RemoveAllRoles(user);
            embed.WithTitle("Remove All Roles");
            embed.WithDescription($"All roles have been removed from `{user}`.");
            embed.WithColor(Pink);
        }

        [Command("deleterole")] //admin
        [Alias("dr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DeleteRole(IRole role)
        {
            await role.DeleteAsync();
            embed.WithTitle("Role Deleted");
            embed.WithDescription($"{Context.User.Mention} role **{role}** has been deleted.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("kick")] //admin
        [Alias("k")]
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

        [Command("ban")] //admin
        [Alias("b")]
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

        [Command("massban")] //admin
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task MassBan([Remainder]List<SocketGuildUser> users)
        {
            foreach (var user in users)
            {
                await user.BanAsync();
                await ReplyAsync($"**{user} has been permanently banned by {Context.User.Mention}.**");
            }
        }

        [Command("masskick")] //admin
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task MassKick([Remainder]List<SocketGuildUser> users)
        {
            foreach (var user in users)
            {
                await user.BanAsync();
                await ReplyAsync($"**{user} has been kicked by {Context.User.Mention}.**");
            }
        }

        [Command("clear")] //admin
        [Alias("c", "purge")]
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

        [Command("createtextchannel")] //utility
        [Alias("ctc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateTextChannel([Remainder]string name)
        {
            var channel = await Context.Guild.CreateTextChannelAsync(name);
            embed.WithDescription($"{Context.User.Mention} has successfully created the channel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("deletetextchannel")] //utility
        [Alias("dtc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildDeleteTextChannel([Remainder]string name)
        {
            foreach (var Channel in Context.Guild.TextChannels) { if (Channel.Name == (name.ToLower())) { await Channel.DeleteAsync(); } }
            embed.WithDescription($"{Context.User.Mention} has successfully deleted the text channel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("createvoicechannel")] //utility
        [Alias("cvc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateVoiceChannel([Remainder]string name)
        {
            var channel = await Context.Guild.CreateVoiceChannelAsync(name);
            embed.WithDescription($"{Context.User.Mention} has successfully created the voicechannel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("deletevoicechannel")] //utility
        [Alias("dvc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildDeleteVoiceChannel([Remainder]string name)
        {
            foreach (var Channel in Context.Guild.VoiceChannels) { if (Channel.Name == (name.ToLower())) { await Channel.DeleteAsync(); } }
            embed.WithDescription($"{Context.User.Mention} has successfully deleted the voice channel #{name}.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("timely")] //currency
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

        [Command("exp")] //exp
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

        [Command("pointsadd")] //currency
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

        [Command("expadd")] //exp
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

        [Command("echo")] //fun
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Echo");
            embed.WithDescription(message);
            embed.WithColor(Pink);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("pick")] //fun
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
