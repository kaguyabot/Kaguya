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
using System.Net;
using System.Timers;

#pragma warning disable

namespace Discord_Bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();

        public Color Pink = new Color(252, 132, 255);

        public Color Red = new Color(255, 0, 0);

        public string version = Utilities.GetAlert("VERSION");

        public string cmdPrefix = Config.bot.cmdPrefix;

        public string botToken = Config.bot.token;

        public async Task BE() //Method to build an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("modules")]
        [Alias("mdls")]
        public async Task ModulesList()
        {
            Context.Channel.SendMessageAsync
                ("```css" +
                $"\nStageBot Modules for {version}:" +
                $"\nType {cmdPrefix}cmds <module> for a list of commands in said module." +
                $"\n" +
                $"\nadministration" +
                $"\ncurrency" +
                $"\nexp" +
                $"\nfun" +
                $"\nosu" +
                $"\nutility" +
                "\n```");
        }

        [Command("cmds")]
        [Alias("commands")]
        public async Task ModulesList([Remainder]string category)
        {
            if (category.ToLower() == "administration" || category.ToLower() == "admin")
            {
                embed.WithTitle("Module: Administration");
                embed.WithDescription("```css" +
                        "\nAll commands in category: Administration" +
                        "\n" +
                        $"\n{cmdPrefix}kick [k]" +
                        $"\n{cmdPrefix}ban [b]" +
                        $"\n{cmdPrefix}masskick" +
                        $"\n{cmdPrefix}massban" +
                        $"\n{cmdPrefix}massblacklist" +
                        $"\n{cmdPrefix}unblacklist" +
                        $"\n{cmdPrefix}removeallroles [rar]" +
                        $"\n{cmdPrefix}createrole [cr]" +
                        $"\n{cmdPrefix}deleterole [dr]" +
                        $"\n{cmdPrefix}clear [c] [purge]" +
                        $"\n{cmdPrefix}stagebotgtfo" +
                        $"\n" +
                        $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                        "\n```");
                embed.WithColor(Pink);
                BE();
            }
            else if (category.ToLower() == "exp")
            {
                embed.WithTitle("Module: EXP");
                embed.WithDescription
                    ("```css" +
                    "\nAll commands in category: Experience Points" +
                    "\n" +
                    $"\n{cmdPrefix}exp" +
                    $"\n{cmdPrefix}expadd [addexp]" +
                    $"\n{cmdPrefix}level" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    "\n```");
                embed.WithColor(Pink);
                BE();
            }
            else if (category.ToLower() == "currency")
            {
                embed.WithTitle("Module: Currency");
                embed.WithDescription
                ("```css" +
                "\nAll commands in category: Currency" +
                "\n" +
                $"\n{cmdPrefix}points" +
                $"\n{cmdPrefix}pointsadd" +
                $"\n{cmdPrefix}timely [t]" +
                $"\b{cmdPrefix}gamble [g]" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```"
                );
            }
            else if (category.ToLower() == "utility")
            {
                embed.WithTitle("Module: Utility");
                embed.WithDescription
                ("```css" +
                "\nAll commands in category: Utility" +
                "\n" +
                $"\n{cmdPrefix}help [h]" +
                $"\n{cmdPrefix}helpdm [hdm]" +
                $"\n{cmdPrefix}createtextchannel [ctc]" +
                $"\n{cmdPrefix}deletetextchannel [dtc]" +
                $"\n{cmdPrefix}createvoicechannel [cvc]" +
                $"\n{cmdPrefix}deletevoicechannel [dvc]" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```");
                embed.WithColor(Pink);
                BE();
            }
            else if (category.ToLower() == "fun")
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
            else if (category.ToLower() == "osu")
            {
                embed.WithTitle("Module: osu!");
                embed.WithDescription("```css" +
                    "\n" +
                    $"\n{cmdPrefix}createteamrole [ctr]" +
                    $"\n{cmdPrefix}delteams" +
                    $"\n{cmdPrefix}osutop" +
                    $"\n{cmdPrefix}recent [r]" +
                    $"\n{cmdPrefix}osuset" +
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
                case "help":
                    embed.WithTitle($"Help: Help!! | `{cmdPrefix}h` / `{cmdPrefix}help`");
                    embed.WithDescription($"Shows the command list. If typed with the name of a command (Ex: `{cmdPrefix}help <command>`), the response will instead contain helpful information on the specified " +
                        $"command, including how to use it.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "helpdm":
                case "hdm":
                    embed.WithTitle($"Help: HelpDM | `{cmdPrefix}helpdm`");
                    embed.WithDescription($"{Context.User.Mention} Sends a DM with helpful information, including a link to add the bot to your own server, and a link to the StageBot Github page!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "exp":
                    embed.WithTitle($"Help: EXP | `{cmdPrefix}exp`");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}exp`." +
                        $"\nReturns the value of experience points a user has in their account.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "expadd":
                case "addexp":
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
                case "t":
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
                        $"\nSyntax: `{cmdPrefix}deleterole <role name>`");
                    BE(); break;
                case "createteamrole":
                case "ctr":
                    embed.WithTitle($"Help: Create Team Roles | `{cmdPrefix}createteamrole`, `{cmdPrefix}ctr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nCreates a role, then applies it to all mentioned users." +
                        $"\nThis is very ideal for managing many groups of people (such as teams in a tournament, hence the name)." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}createteamrole <role name> <mentioned users>`" +
                        $"\nExample: `{cmdPrefix}createteamrole \"Smelly Sushi\" @user1#0000 @smellyfish#2100 @smellysushilover#9999`.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "osutop":
                    embed.WithTitle($"Help: osu! Top | `{cmdPrefix}osutop`");
                    embed.WithDescription($"\n" +
                        $"\n{Context.User.Mention} Displays the specified amount of top osu! plays for a given player with other relevant information." +
                        $"\nThe number of requested plays to display may not be more than 10." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}osutop 5 Stage` | `{cmdPrefix}osutop 8 \"Smelly sushi\"`");
                    embed.WithColor(Pink);
                    BE(); break;
                case "delteams":
                    embed.WithTitle($"Help: Deleting Teams | `{cmdPrefix}delteams`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: `Manage Roles`, `Administrator`, `Bot Owner`**" +
                        $"\n" +
                        $"\nDeletes all team roles. A team role is any role that has the word \"Team: \" inside of it (with the space)." +
                        $"\nThis command will delete ALL team roles upon execution, making this command dangerous and irreversable.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "recent":
                case "r":
                    embed.WithTitle($"Help: osu! Recent | `{cmdPrefix}r` / `{cmdPrefix}recent`");
                    embed.WithDescription($"{Context.User.Mention} Displays the most recent osu! play for the given user. If there is no user specified," +
                        $" the bot will use the osu! username that was specified to the command executor's StageBot account (through {cmdPrefix}osuset).\n" +
                        $"As of right now, no response will be given for an invalid username.\n");
                    embed.WithColor(Pink);
                    BE(); break;
                case "osuset":
                    string name = Context.User.Username;
                    embed.WithTitle($"Help: osuset | `{cmdPrefix}osuset`");
                    embed.WithDescription($"{Context.User.Mention} Adds an osu! username to your StageBot account! Setting your osu! username allows you to use all osu! related commands without any additional " +
                        $"parameters. For example, instead of typing `{cmdPrefix}osutop {name}`, you can now just type `{cmdPrefix}osutop` to get your most recent osu! plays. Same thing for `{cmdPrefix}r` / `{cmdPrefix}recent`!");
                    embed.WithFooter("Ensure your username is spelled properly, otherwise all osu! related commands will not work for you!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "massblacklist":
                    embed.WithTitle($"Help: Mass Blacklist | `{cmdPrefix}massblacklist`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner, Administrator**" +
                        $"\n" +
                        $"\nA bot owner may execute this command on a list of users they deem unworthy of being able to ever use StageBot again. These users are permanently banned from the server this command is executed in." +
                        $"These users will have all of their EXP and Points reset to zero, and will be permanently filtered from receiving EXP and executing StageBot commands." +
                        $"\nSyntax: `{cmdPrefix}massblacklist @username#123` | `{cmdPrefix}massblacklist @username#123 @ToxicPlayer123#7777 @SuckySmellySushi#1234`");
                    embed.WithFooter("Bot owners: This command is EXTREMELY DANGEROUS. The only way to unblacklist someone is to edit your accounts.json file!!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "unblacklist":
                    embed.WithTitle($"Help: Unblacklisting Users | `{cmdPrefix}unblacklist <UserID>`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nUnblacklists the specified userID." +
                        $"\nSelf-Hosters: If you do not know the ID of the person to unblacklist, look through accounts.json.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "gamble":
                case "g":
                    embed.WithTitle($"Help: Gambling | `{cmdPrefix}gamble` / `{cmdPrefix}g`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to roll the dice and gamble your points!" +
                        $"\nA roll between `0-66` will result in a loss of your bet." +
                        $"\nA roll between `67-78` will return your bet back to you with a multiplier of `1.25x`" +
                        $"\nRolls between `79-89`, `90-95`, `96-99`, and `100` will yield multipliers of `1.75x`, `2.25x`, `3x`, and `5x` respectively." +
                        $"\nThe maximum amount of points you can gamble at one time is set to `25,000`.");
                    BE(); break;
                case "stagebotgtfo":
                    embed.WithTitle($"Help: StageBot, gtfo! | `{cmdPrefix}stagebotgtfo`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAdministrator only command that forces StageBot to leave the current server.");
                    embed.WithColor(Pink);
                    BE(); break;


                default:
                    embed.WithDescription($"**{Context.User.Mention} \"{command}\" is not a valid command.**");
                    embed.WithColor(Pink);
                    BE(); break;
            }
        }

        [Command("helpt")] //utility, pinned
        [Alias("ht")]
        public async Task Help()
        {
            embed.WithTitle("Commands List");
            embed.WithDescription($"All StageBot commands separated by category. To use the command, have \nthe `{cmdPrefix}` symbol appended before the phrase. For more information on a specific command, " +
                $"type `{cmdPrefix}h <command>`");
            embed.AddField("Administration", "`kick [k]` \n`ban [b]` \n`masskick` \n`massban` \n`massblacklist` \n`unblacklist` \n`removeallroles [rar]` \n`createrole [cr]` \n`deleterole [dr]` \n`clear [c] [purge]` \n`stagebotgtfo`", true);
            embed.AddField("Currency", "`points` \n`pointsadd` \n`timely [t]` \n`gamble [g]`", true);
            embed.AddField("EXP", "`exp` \n`expadd [addexp]` \n`level`", true);
            embed.AddField("Fun", "`echo` \n`pick`", true);
            embed.AddField("osu!", "`createteamrole [ctr]` \n`delteams` \n`osutop` \n`recent [r]` \n`osuset`", true);
            embed.AddField("Utility", "`help [h]` \n`helpdm [hdm]` \n`createtextchannel [ctc]` \n`deletetextchannel [dtc]` \n`createvoicechannel [cvc]` \n`deletevoicechannel [dvc]`", true);
            embed.WithColor(Pink);
            BE();
        }

        [Command("helpdm")] //utility, pinned
        [Alias("hdm")]
        public async Task HelpDM()
        {
            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync($"Need help with a specific command? Type `{cmdPrefix}mdls` to see a list of categories the commands are listed under." +
                $"\nType `{cmdPrefix}<module name>` to see all commands listed under that module." +
                $"\nType `{cmdPrefix}h <command name>` for more how to use the command and a detailed description of what it does." +
                $"\nAdd me to your server with this link!: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=2146958847" +
                $"\nWant to keep track of all the changes or feel like self hosting? Feel free to check out the StageBot Github page!: https://github.com/stageosu/StageBot" +
                $"\nStill need help? Feel free to join the StageBot Development server and ask for help there!: https://discord.gg/yhcNC97");
        }



        [Command("Unblacklist")] //administration
        [RequireOwner]
        public async Task Unblacklist(SocketUser id)
        {
            var userAccount = UserAccounts.GetAccount(id);
            userAccount.Blacklisted = 0;
            UserAccounts.SaveAccounts();

            embed.WithTitle("User Unblacklisted");
            embed.WithDescription($"User `{userAccount.Username}` with ID `{userAccount.ID}` has been Unblacklisted from StageBot functionality.");
            embed.WithFooter("Please note that all Points and EXP are not able to be restored.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("massblacklist")] //administration
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task MassBlacklist(List<SocketGuildUser> users)
        {
            foreach (var user in users)
            {
                var serverID = Context.Guild.Id;
                var serverName = Context.Guild.Name;
                var userAccount = UserAccounts.GetAccount(user);
                userAccount.EXP = 0;
                userAccount.Points = 0;
                userAccount.Blacklisted = 1;
                UserAccounts.SaveAccounts();

                await user.SendMessageAsync($"You have been permanently banned from `{serverName}` with ID `{serverID}`." +
                    $"\nYour new StageBot EXP amount is `{userAccount.EXP}`. Your new StageBot currency amount is `{userAccount.Points}`." +
                    $"\nUser `{userAccount.Username}` with ID `{userAccount.ID}` has been permanently blacklisted from all StageBot functions, and can no longer execute any of the StageBot commands." +
                    $"\nIf you wish to appeal this blacklist, message `Stage#0001` on Discord.");
               // await user.BanAsync();
                await ReplyAsync($"**{user.Mention} has been permanently `banned` and `blacklisted`.**");
            }
        }

        [Command("osuset")] //osu
        public async Task osuSet([Remainder]string username)
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            string oldUsername = userAccount.OsuUsername;
            if (oldUsername == null)
                oldUsername = "Null";
            userAccount.OsuUsername = username;
            UserAccounts.SaveAccounts();

            embed.WithTitle("osu! Username Set");
            embed.WithDescription($"{Context.User.Mention} **Your new username has been set! Changed from `{oldUsername}` to `{username}`.**");
            embed.WithFooter("Ensure your username is spelled properly, otherwise all osu! related commands will not work for you!");
            embed.WithColor(Pink);
            BE();
        }

        [Command("recent")] //osu
        [Alias("r")]
        public async Task osuRecent(string player = null, int mode = 0)
        {
            if(player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
            }

            string osuapikey = Config.bot.osuapikey;

            string jsonRecent = "";
            using (WebClient client = new WebClient())
            {
                jsonRecent = client.DownloadString($"https://osu.ppy.sh/api/get_user_recent?k={osuapikey}&u=" + player);
            }
            if (jsonRecent == "[]")
            {
                string NormalUserName = "";
                using (WebClient client = new WebClient())
                {
                    NormalUserName = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u=" + player);
                }

                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(NormalUserName)[0];
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("" + mapUserNameObject.username + " hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + mapUserNameObject.user_id);
                });
                embed.WithColor(Pink);
                BE();
            }
            else
            {
                var playerRecentObject = JsonConvert.DeserializeObject<dynamic>(jsonRecent)[0];
                string mapID = playerRecentObject.beatmap_id;

                string mapRecent = "";
                using (WebClient client = new WebClient())
                {
                    mapRecent = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={osuapikey}&b={mapID}");
                }
                var mapRecentObject = JsonConvert.DeserializeObject<dynamic>(mapRecent)[0];

                string mapTitle = mapRecentObject.title;
                string difficulty = mapRecentObject.version;
                string score = playerRecentObject.score;
                string maxCombo = playerRecentObject.maxcombo;
                string artist = mapRecentObject.artist;
                double count50 = playerRecentObject.count50;
                double count100 = playerRecentObject.count100;
                double count300 = playerRecentObject.count300;
                double countMiss = playerRecentObject.countmiss;
                string fullCombo = playerRecentObject.perfect;
                if (fullCombo == "1")
                    fullCombo = " **Full Combo!**"; else fullCombo = null;
                string mods = playerRecentObject.enabled_mods;
                mods = EnabledModsList(mods);
                string maxPossibleCombo = mapRecentObject.max_combo;
                mods = EnabledModsList(mods);
                string date = playerRecentObject.date;
                double starRating = mapRecentObject.difficultyrating;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                string grade = playerRecentObject.rank;
                switch (grade)
                {
                    case "XH":
                        grade = "<:XH:553119188089176074>"; break;
                    case "X":
                        grade = "<:X_:553119217109565470>"; break;
                    case "SH":
                        grade = "<:SH:553119233463025691>"; break;
                    case "S":
                        grade = "<:S_:553119252329267240>"; break;
                    case "A":
                        grade = "<:A_:553119274256826406>"; break;
                    case "B":
                        grade = "<:B_:553119304925577228>"; break;
                    case "C":
                        grade = "<:C_:553119325565878272>"; break;
                    case "D":
                        grade = "<:D_:553119338035675138>"; break;
                    case "F":
                        grade = "<:F_:557297028263051288>"; break;
                }

                string NormalUserName = "";
                using (WebClient client = new WebClient())
                {
                    NormalUserName = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u=" + player);
                }
                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(NormalUserName)[0];

                string playerRecentString = $"▸ {grade}{mods} ▸ **[{mapTitle} [{difficulty}]](https://osu.ppy.sh/b/{mapID})** by ***{artist}***\n" +
                    $"▸ **☆{starRating.ToString("F")}** ▸ **{accuracy.ToString("F")}%**\n" +
                    $"▸ [Combo: {maxCombo}x / Max: {maxPossibleCombo}] {fullCombo}\n" +
                    $"▸ [{count300} / {count100} / {count50} / {countMiss}]";

                var difference = DateTime.UtcNow - (DateTime)playerRecentObject.date;

                string footer = $"{mapUserNameObject.username} preformed this play {difference.Days} days {difference.Hours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.";

                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + mapUserNameObject.username)
                        .WithIconUrl("https://a.ppy.sh/" + playerRecentObject.user_id);
                });
                embed.WithDescription($"{playerRecentString}");
                embed.WithFooter(footer);
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("osutop")] //osu
        public async Task osuTop(string player = null, int num = 5)
    {
            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
            }

            string osuapikey = Config.bot.osuapikey;

            if (num > 10)
            {
                embed.WithDescription($"{Context.User.Mention} You may not request more than 10 top plays.");
                return;
            }
            string jsonTop = "";
            using (WebClient client = new WebClient())
            {
                jsonTop = client.DownloadString($"https://osu.ppy.sh/api/get_user_best?k={osuapikey}&u=" + player + "&limit=" + num);
            }
            PlayData[] PlayDataArray = new PlayData[num];

            for (var i = 0; i < num; i++)
            {
                var playerTopObject = JsonConvert.DeserializeObject<dynamic>(jsonTop)[i];
                string jsonMap = "";

                string mapID = playerTopObject.beatmap_id.ToString();
                using (WebClient client = new WebClient())
                {
                    jsonMap = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={osuapikey}&b=" + mapID);
                }
                var mapObject = JsonConvert.DeserializeObject<dynamic>(jsonMap)[0];
                double pp = playerTopObject.pp;
                string mapTitle = mapObject.title;
                double difficultyRating = mapObject.difficultyrating;
                string version = mapObject.version;
                string country = playerTopObject.country;
                double count300 = playerTopObject.count300;
                double count100 = playerTopObject.count100;
                double count50 = playerTopObject.count50;
                double countMiss = playerTopObject.countmiss;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                double playerMaxCombo = playerTopObject.maxcombo;
                double mapMaxCombo = mapObject.max_combo;
                string grade = playerTopObject.rank;
                switch (grade)
                {
                    case "XH":
                        grade = "<:XH:553119188089176074>"; break;
                    case "X":
                        grade = "<:X_:553119217109565470>"; break;
                    case "SH":
                        grade = "<:SH:553119233463025691>"; break;
                    case "S":
                        grade = "<:S_:553119252329267240>"; break;
                    case "A":
                        grade = "<:A_:553119274256826406>"; break;
                    case "B":
                        grade = "<:B_:553119304925577228>"; break;
                    case "C":
                        grade = "<:C_:553119325565878272>"; break;
                    case "D":
                        grade = "<:D_:553119338035675138>"; break;
                }

                string mods = playerTopObject.enabled_mods;
                mods = EnabledModsList(mods);
                PlayData PlayData = new PlayData(mapTitle, mapID, pp, difficultyRating, version, country, count300, count100, count50, countMiss, accuracy, grade, playerMaxCombo, mapMaxCombo, mods);
                PlayDataArray[i] = PlayData;
            }

            string jsonPlayer = "";
                using (WebClient client = new WebClient())
                {
                    jsonPlayer = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}");
                }

            var playerObject = JsonConvert.DeserializeObject<dynamic>(jsonPlayer)[0];

            string TopPlayString = ""; //Country images to come later.
            for (var j = 0; j < num; j++)
            {
                TopPlayString = TopPlayString + $"\n{j + 1}: ▸ {PlayDataArray[j].grade}{PlayDataArray[j].mods} ▸ {PlayDataArray[j].mapID} ▸ **[{PlayDataArray[j].mapTitle} [{PlayDataArray[j].version}]](https://osu.ppy.sh/b/{PlayDataArray[j].mapID})** " +
                    $"\n▸ **☆{PlayDataArray[j].difficultyRating.ToString("F")}** ▸ **{PlayDataArray[j].accuracy.ToString("F")}%** for **{PlayDataArray[j].pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {PlayDataArray[j].playerMaxCombo}x / Max: {PlayDataArray[j].mapMaxCombo}]\n";
            }
            embed.WithAuthor($"{player}'s Top osu! Standard Plays");
            embed.WithTitle($"**Top {num} osu! standard plays for {player}:**");
            embed.WithUrl($"https://www.osu.ppy.sh/u/{player}/");
            embed.WithDescription($"osu! Stats for player **{player}**:\n" + TopPlayString);
            embed.WithColor(Pink);
            BE();
        }

        public static string EnabledModsList(string mods)
        {
            switch (mods)
            {
                case "0":
                    mods = ""; break;
                case "1":
                    mods = "**+NF**"; break;
                case "2":
                    mods = "**+EZ**"; break;
                case "3":
                    mods = "**+NFEZ**"; break;
                case "8":
                    mods = "**+HD**"; break;
                case "16":
                    mods = "**+HR**"; break;
                case "24":
                    mods = "**+HDHR**"; break;
                case "25":
                    mods = "**+HDHRNF**"; break;
                case "27":
                    mods = "**+HDHRNFEZ**"; break;
                case "32":
                    mods = "**+SD**"; break;
                case "34":
                    mods = "**+EZSD**"; break;
                case "40":
                    mods = "**+HDSD**"; break;
                case "48":
                    mods = "**+HRSD**"; break;
                case "50":
                    mods = "**+EZHRSD**"; break;
                case "56":
                    mods = "**+HDHRSD**"; break;
                case "58":
                    mods = "**+EZHDHRSD**"; break;
                case "64":
                    mods = "**+DT**"; break;
                case "65":
                    mods = "**+NFDT**"; break;
                case "66":
                    mods = "**+EZDT**"; break;
                case "67":
                    mods = "**+EZDTNF**"; break;
                case "72":
                    mods = "**+HDDT**"; break;
                case "73":
                    mods = "**+HDDTNF**"; break;
                case "74":
                    mods = "**+EZHDDT**"; break;
                case "75":
                    mods = "**+EZNFHDDT**"; break;
                case "80":
                    mods = "**+HRDT**"; break;
                case "88":
                    mods = "**+HDHRDT**"; break;
                case "89":
                    mods = "**+NFHDHRDT**"; break;
                case "90":
                    mods = "**+EZHDHRDT**"; break;
                case "91":
                    mods = "**+EZNFHDHRDT**"; break;
                case "96":
                    mods = "**+DTSD**"; break;
                case "98":
                    mods = "**+EZDTSD**"; break;
                case "104":
                    mods = "**+HDDTSD**"; break;
                case "106":
                    mods = "**+EZHDDTSD**"; break;
                case "112":
                    mods = "**+HRDTSD**"; break;
                case "114":
                    mods = "**+EZHRDTSD**"; break;
                case "120":
                    mods = "**+HDHRDTSD**"; break;
                case "122":
                    mods = "**+EZHDHRDTSD**"; break;
                case "256":
                    mods = "**+HT**"; break;
                case "257":
                    mods = "**+NFHT**"; break;
                case "258":
                    mods = "**+HTEZ**"; break;
                case "259":
                    mods = "**+NFEZHT**"; break;
                case "264":
                    mods = "**+HDHT**"; break;
                case "265":
                    mods = "**+NFHDHT**"; break;
                case "267":
                    mods = "**+NFEZHDHT**"; break;
                case "272":
                    mods = "**+HTHR**"; break;
                case "273":
                    mods = "**+NFHTHR**"; break;
                case "275":
                    mods = "**+EZNFHTHR**"; break;
                case "280":
                    mods = "**HDHRHT**"; break;
                case "281":
                    mods = "**+NFHDHRHT**"; break;
                case "283":
                    mods = "**+EZNFHDHRHT**"; break;
                case "288":
                    mods = "**+HTSD**"; break;
                case "290":
                    mods = "**+EZHTSD**"; break;
                case "296":
                    mods = "**+HDHTSD**"; break;
                case "298":
                    mods = "**+EZHDHTSD**"; break;
                case "304":
                    mods = "**+HRHTSD**"; break;
                case "306":
                    mods = "**+EZHRHTSD**"; break;
                case "312":
                    mods = "**+HDHRHTSD**"; break;
                case "314":
                    mods = "**+EZHDHRHTSD**"; break;
                case "1024":
                    mods = "**+FL**"; break;
                case "1025":
                    mods = "**+NFFL**"; break;
                case "1026":
                    mods = "**+EZFL**"; break;
                case "1027":
                    mods = "**+NFEZFL**"; break;
                case "1032":
                    mods = "**+HDFL**"; break;
                case "1040":
                    mods = "**+HRFL**"; break;
                case "1048":
                    mods = "**+HDHRFL**"; break;
                case "1049":
                    mods = "**+HDHRFLNF**"; break;
                case "1051":
                    mods = "**+HDHRFLNFEZ**"; break;
                case "1056":
                    mods = "**+FLSD**"; break;
                case "1058":
                    mods = "**+EZFLSD**"; break;
                case "1064":
                    mods = "**+HDFLSD**"; break;
                case "1072":
                    mods = "**+HRFLSD**"; break;
                case "1074":
                    mods = "**+EZHRFLSD**"; break;
                case "1080":
                    mods = "**+HDHRFLSD**"; break;
                case "1082":
                    mods = "**+EZHDHRFLSD**"; break;
                case "1088":
                    mods = "**+DTFL**"; break;
                case "1089":
                    mods = "**+NFDTFL**"; break;
                case "1090":
                    mods = "**+EZDTFL**"; break;
                case "1091":
                    mods = "**+EZDTNFFL**"; break;
                case "1096":
                    mods = "**+HDDTFL**"; break;
                case "1097":
                    mods = "**+HDDTNFFL**"; break;
                case "1098":
                    mods = "**+EZHDDTFL**"; break;
                case "1099":
                    mods = "**+EZNFHDDTFL**"; break;
                case "1104":
                    mods = "**+HRDTFL**"; break;
                case "1112":
                    mods = "**+HDHRDTFL**"; break;
                case "1113":
                    mods = "**+NFHDHRDTFL**"; break;
                case "1114":
                    mods = "**+EZHDHRDTFL**"; break;
                case "1115":
                    mods = "**+EZNFHDHRDTFL**"; break;
                case "1120":
                    mods = "**+DTFLSD**"; break;
                case "1122":
                    mods = "**+EZDTFLSD**"; break;
                case "1130":
                    mods = "**+HDDTFLSD**"; break;
                case "1132":
                    mods = "**+EZHDDTFLSD**"; break;
                case "1138":
                    mods = "**+HRDTFLSD**"; break;
                case "1140":
                    mods = "**+EZHRDTFLSD**"; break;
                case "1144":
                    mods = "**+HDHRDTFLSD**"; break;
                case "1146":
                    mods = "**+EZHDHRDTFLSD**"; break;
                case "1280":
                    mods = "**+HTFL**"; break;
                case "1281":
                    mods = "**+NFHTFL**"; break;
                case "1282":
                    mods = "**+HTEZFL**"; break;
                case "1283":
                    mods = "**+NFEZHTFL**"; break;
                case "1288":
                    mods = "**+HDHTFL**"; break;
                case "1289":
                    mods = "**+NFHDHTFL**"; break;
                case "1291":
                    mods = "**+NFEZHDHTFL**"; break;
                case "1292":
                    mods = "**+HTHRFL**"; break;
                case "1293":
                    mods = "**+NFHTHRFL**"; break;
                case "1295":
                    mods = "**+EZNFHTHRFL**"; break;
                case "1300":
                    mods = "**HDHRHTFL**"; break;
                case "1301":
                    mods = "**+NFHDHRHTFL**"; break;
                case "1303":
                    mods = "**+EZNFHDHRHTFL**"; break;
                case "1312":
                    mods = "**+HTFLSD**"; break;
                case "1314":
                    mods = "**+EZHTFLSD**"; break;
                case "1320":
                    mods = "**+HDHTFLSD**"; break;
                case "1322":
                    mods = "**+EZHDHTFLSD**"; break;
                case "1328":
                    mods = "**+HRHTFLSD**"; break;
                case "1330":
                    mods = "**+EZHRHTFLSD**"; break;
                case "1336":
                    mods = "**+HDHRHTFLSD**"; break;
                case "1338":
                    mods = "**+EZHDHRHTFLSD**"; break;
            }
            return mods;
        }

        [Command("createteamrole")] //osu
        [Alias("ctr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task CreateTeamRoles(string teamName, [Remainder]List<SocketGuildUser> users)
        {
            var participantRole = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "participant" || x.Name == "Participant");
            var roleName = "Team: " + teamName;
            var teamRole = await Context.Guild.CreateRoleAsync(roleName);
            foreach (var user in users)
            {
                await user.AddRoleAsync(teamRole);
                await user.AddRoleAsync(participantRole);
                embed.WithDescription($"**{user}** has been added to {teamRole.Mention} and {participantRole.Mention}.");
                embed.WithColor(Pink);
            }
        }

        [Command("delteams")] //osu
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task DeleteTeams()
        {
            var roles = Context.Guild.Roles;
            foreach (IRole role in roles)
            {
                if (role.Name.Contains("Team: "))
                {
                    role.DeleteAsync();
                    embed.WithTitle("Teams Deleted");
                    embed.WithDescription("The following teams have been deleted: " +
                        $"\n{role} ");
                    embed.WithColor(Pink);
                    BE();
                }
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

        [Command("createrole")] //admin
        [Alias("cr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task CreateRole(string role)
        {
            await Context.Guild.CreateRoleAsync(role);
            embed.WithTitle("Role Created");
            embed.WithDescription($"{Context.User.Mention} role **{role}** has been created.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("stagebotgtfo")] //admin
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task leave()
        {
            embed.WithTitle("Leaving Server");
            embed.WithDescription($"Administrator {Context.User.Mention} has directed me to leave. Goodbye!");
            embed.WithColor(Red);
            Context.Guild.LeaveAsync();
        }

        /*[Command("mute")] //admin
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task Mute(IGuildUser user, string time = "")
        {
            
        }
        */

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
        public async Task ClearMessages([Remainder]int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            const int delay = 5000;
            var m = await this.ReplyAsync($"Clearing of messages completed. This message will be deleted in {delay / 1000} seconds.");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

       /* [Command("createtimedannouncement")]
        [Alias("ctda")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task CreateTimedAnnouncement(string message, )
        {
            Console.WriteLine(message);
        }
        */

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
        [Alias("t")]
        public async Task DailyPoints(uint timeout = 24)
        {
            uint bonus = EditableCommands.bot.timelyPoints;
            var userAccount = UserAccounts.GetAccount(Context.User);
            if(!CanReceiveTimelyPoints(userAccount, (int)timeout))
            {
                var difference = DateTime.Now - userAccount.LastReceivedTimelyPoints;
                var formattedTime = $"{difference.Hours}h {difference.Minutes}m {difference.Seconds}s";
                embed.WithTitle("Timely Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}timely`!" +
                    $" Please wait until `{timeout} hours` have passed to receive more timely points.");
                embed.WithColor(Pink);
                BE();
                return;
            }
            userAccount.Points += bonus;
            userAccount.LastReceivedTimelyPoints = DateTime.Now;
            UserAccounts.SaveAccounts();
            embed.WithTitle("Timely Points");
            embed.WithDescription($"{Context.User.Mention} has received {bonus} points! Claim again in {timeout}h.");
            embed.WithColor(Pink);
            BE();
        }

        internal static bool CanReceiveTimelyPoints(UserAccount user, int timeout)
        {
            var difference = DateTime.Now - user.LastReceivedTimelyPoints;
            return difference.TotalHours > timeout;
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

        [Command("level")] //exp
        public async Task Level()
        {
            var account = UserAccounts.GetAccount(Context.User);
            embed.WithTitle("Level");
            embed.WithDescription($"{Context.User.Mention} you have {account.LevelNumber} levels.");
            embed.WithColor(Pink);
            BE();
        }

        [Command("gamble")] //currency
        [Alias("g")]
        public async Task GamblePoints(int points)
        {
            var user = Context.User;
            var userAccount = UserAccounts.GetAccount((SocketGuildUser)Context.User);
            if (points > userAccount.Points)
            {
                embed.WithTitle("Gambling: Insufficient Points!");
                embed.WithDescription($"{user.Mention} you have an insufficient amount of points!" +
                    $"\nThe maximum amount you may gamble is {userAccount.Points}.");
                embed.WithColor(Red);
                BE();
                return;
            }
            if(points > 25000)
            {
                embed.WithTitle("Gambling: Too Many Points!");
                embed.WithDescription($"**{user.Mention} you are attempting to gamble too many points!" +
                    $"\nThe maximum amount you may gamble is `25,000` points.**");
                embed.WithColor(Red);
                BE();
                return;
            }

            Random rand = new Random();
            var roll = rand.Next(0, 100);


            if (roll <= 66)
            {
                userAccount.Points = userAccount.Points - (uint)points;
                userAccount.LifetimeGambleLosses++;
                userAccount.LifetimeGambles++;

                string[] sadEmotes = { "<:PepeHands:431853568669253632>", "<:FeelsBadMan:431647398071107584>", "<:FeelsWeirdMan:431148381449224192>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);
                Console.WriteLine(num);
                embed.WithTitle($"Gambling: Loser! {sadEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and lost their bet of `{points.ToString("N0")}`! Better luck next time!** <:SagiriBlush:498009810692734977>");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();
                Console.WriteLine(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles);

                UserAccounts.SaveAccounts();
                return;
            }
            else if (67 <= roll && roll <= 78)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes1 = { "<:peepoHappy:479314678699524116>", "<:EZ:431149816127553547>", "<a:pats:432262215018741780>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 1.25;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling: Winner! {happyEmotes1[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (79 <= roll && roll <= 89)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes2 = { "<:Pog:484960397946912768>", "<:PogChamp:433109653501640715>", "<:nepWink:432745215217106955>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 1.75;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling Winner: High Roll! {happyEmotes2[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (90 <= roll && roll <= 95)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string[] eliteEmotes = { "<:PogU:509194017368702987>", "<a:Banger:506288311829135386>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 2.25;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling Winner: Elite Roll! {eliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (96 <= roll && roll <= 99)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string[] superEliteEmotes = { "<:YES:462371445864136732>", "<:smug:453259470815100941>", "<:Woww:442687161871892502>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 3.00;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling Winner: Super Elite Roll! {superEliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (roll == 100)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string sirenEmote = "<a:siren:429784681316220939>";

                var multiplier = 5.00;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"{sirenEmote} Gambling Winner: Perfect Roll! {sirenEmote}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(255, 223, 0);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
        }

        [Command("points")] //currency
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
        [Alias("addpoints")]
        [RequireOwner]
        public async Task PointsAdd(uint points, IGuildUser user = null)
        {
            if (!UserIsAdmin((SocketGuildUser)Context.User))
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription("Required Permission Missing: `Administrator`.");
                embed.WithColor(252, 132, 255);
                await Context.Channel.SendMessageAsync(Context.User.Mention + ":x: You are not an administrator.");
                return;
            }
            if(user == null)
            {
                var userAccount = UserAccounts.GetAccount(Context.User);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
                embed.WithColor(Pink);
                BE();
            }
            else if(user is IGuildUser)
            {
                var userAccount = UserAccounts.GetAccount((SocketUser)user);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
                embed.WithColor(Pink);
                BE();
            }
            else
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} Unable to add points to {user}! Make sure they exist and try again!");
                embed.WithColor(Pink);
                BE();
            }
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
