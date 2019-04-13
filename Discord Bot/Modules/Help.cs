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
using Discord_Bot.Core.Server_Files;
using Discord_Bot.Core.Commands;

#pragma warning disable

namespace Discord_Bot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
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

        [Command("h")] //The BIG fish, Help
        [Alias("help")]
        public async Task HelpCommand([Remainder]string command)
        {
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            switch (command.ToLower())
            {
                case "h":
                case "help":
                    embed.WithTitle($"Help: Help!! | `{cmdPrefix}h` / `{cmdPrefix}help`");
                    embed.WithDescription($"Shows the command list. If typed with the name of a command (Ex: `{cmdPrefix}help <command>`), the response will instead contain helpful information on the specified " +
                        $"command, including how to use it.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "helpdm":
                case "hdm":
                    embed.WithTitle($"Help: HelpDM | `{cmdPrefix}helpdm`");
                    embed.WithDescription($"{Context.User.Mention} Sends a DM with helpful information, including a link to add the bot to your own server, and a link to the Kaguya Github page!");
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
                    embed.WithTitle($"Help: Adding Experience Points | `{cmdPrefix}expadd` / `{cmdPrefix}addexp");
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
                case "addpoints":
                    embed.WithTitle($"Help: Adding Points | `{cmdPrefix}pointsadd`");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\n{Context.User.Mention} Syntax: `{cmdPrefix}pointsadd <number of points to add>`. The number of points you are adding must be a positive whole number.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "level":
                    embed.WithTitle($"Help: Level | `{cmdPrefix}level`");
                    embed.WithDescription($"{Context.User.Mention} Displays your current Kaguya level!");
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
                case "commands":
                case "cmds":
                    embed.WithTitle($"Help: Commands | `{cmdPrefix}commands` / `{cmdPrefix}cmds`");
                    embed.WithDescription($"{Context.User.Mention} Displays a list of commands for the specified module. Use {cmdPrefix}modules " +
                        $"for the list of modules you may pick from. Modules are essentially command groups seperated by category." +
                        $"\nSyntax: `{cmdPrefix}cmds <module>`, `{cmdPrefix}cmds administrator`");
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
                    embed.WithTitle($"Help: Pick | `{cmdPrefix}pick`");
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
                        "\nThese points are added to your Kaguya account." +
                        "\nIf you are in a server with a self-hosted version of Kaguya, these values may be different." +
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
                case "massban":
                    embed.WithTitle($"Help: Mass Banning of Users | `{cmdPrefix}massban`");
                    embed.WithDescription($"**{Context.User.Mention} Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nTakes a list of mentioned users and permanently bans them simultaneously." +
                        $"\nSyntax: `{cmdPrefix}massban @mentioneduser#0001 @otheruser#0002 @smellysushi#2623 [...]`");
                    embed.WithColor(Pink);
                    BE(); break;
                case "masskick":
                    embed.WithTitle($"Help: Mass Kicking of Users | `{cmdPrefix}masskick`");
                    embed.WithDescription($"**{Context.User.Mention} Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nTakes a list of mentioned users and kicks them simultaneously." +
                        $"\nSyntax: `{cmdPrefix}masskick @bullyHunter#0001 @stinkysushi#0002 @smellysushi#2623 [...]`");
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
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nDeletes a role from the server (and in the process, removes said role from everyone who had it). " +
                        $"If multiple matches of the same role are found, the bot will delete all occurrences of said role." +
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
                        $" the bot will use the osu! username that was specified to the command executor's Kaguya account (through {cmdPrefix}osuset).\n" +
                        $"As of right now, no response will be given for an invalid username.\n");
                    embed.WithColor(Pink);
                    BE(); break;
                case "osuset":
                    string name = Context.User.Username;
                    embed.WithTitle($"Help: osuset | `{cmdPrefix}osuset`");
                    embed.WithDescription($"{Context.User.Mention} Adds an osu! username to your Kaguya account! Setting your osu! username allows you to use all osu! related commands without any additional " +
                        $"parameters. For example, instead of typing `{cmdPrefix}osutop {name}`, you can now just type `{cmdPrefix}osutop` to get your most recent osu! plays. Same thing for `{cmdPrefix}r` / `{cmdPrefix}recent`!");
                    embed.WithFooter("Ensure your username is spelled properly, otherwise all osu! related commands will not work for you!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "massblacklist":
                    embed.WithTitle($"Help: Mass Blacklist | `{cmdPrefix}massblacklist`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner, Administrator**" +
                        $"\n" +
                        $"\nA bot owner may execute this command on a list of users they deem unworthy of being able to ever use Kaguya again. These users are permanently banned from the server this command is executed in." +
                        $"These users will have all of their EXP and Points reset to zero, and will be permanently filtered from receiving EXP and executing Kaguya commands." +
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
                case "groll":
                case "gr":
                    embed.WithTitle($"Help: Gambling | `{cmdPrefix}gamble` / `{cmdPrefix}g`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to roll the dice and gamble your points!" +
                        $"\nA roll between `0-66` will result in a loss of your bet." +
                        $"\nA roll between `67-78` will return your bet back to you with a multiplier of `1.25x`" +
                        $"\nRolls between `79-89`, `90-95`, `96-99`, and `100` will yield multipliers of `1.75x`, `2.25x`, `3x`, and `5x` respectively." +
                        $"\nThe maximum amount of points you can gamble at one time is set to `25,000`.");
                    BE(); break;
                case "kaguyagtfo":
                    embed.WithTitle($"Help: Kaguya, gtfo! | `{cmdPrefix}kaguyagtfo`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAdministrator only command that forces Kaguya to leave the current server.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "prefix":
                    embed.WithTitle($"Help: Prefix Alteration | `{cmdPrefix}prefix`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to change the bot's command prefix. Typically, this is one or two symbols `(!, $, %, >, etc.)`." +
                        $"\nTo reset the command prefix, type {cmdPrefix}prefix, or tag me and type `prefix`! The bot will always display the last known command prefix " +
                        $"and the new prefix when using this command.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "serverexplb":
                case "explb":
                    embed.WithTitle($"Help: Server EXP Leaderboard | `{cmdPrefix}serverexplb` / `{cmdPrefix}explb`");
                    embed.WithDescription($"{Context.User.Mention} Displays the 10 top EXP holders in the server. This command " +
                        $"also displays their level.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "globalexplb":
                case "gexplb":
                    embed.WithTitle($"Help: Global EXP Leaderboard | `{cmdPrefix}globalexplb` / `{cmdPrefix}gexplb`");
                    embed.WithDescription($"{Context.User.Mention} Displays the 10 top EXP holders in the entire Kaguya database! This command " +
                        $"also displays their level.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "scrapeserver":
                    embed.WithTitle($"Help: Server Scraping | `{cmdPrefix}scrapeserver`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\nOrders the bot to create user accounts for every individual in the server, even if they have never typed " +
                        $"in chat. This function is automatically called when using `{cmdPrefix}massblacklist` to ensure that " +
                        $"there is no question on whether they will be able to be banned/unbanned. Creating a user account allows for name " +
                        $"and ID logging, the latter is necessary if a bot owner wishes to unblacklist a user.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "scrapedatabase":
                    embed.WithTitle($"Help: Database Scraping | `{cmdPrefix}scrapedatabase`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\nCreates an account for every user in every server that Kaguya is connected to. This command will not create accounts " +
                        $"for other bots or users in servers with over `3,500` members. This command primarily exists for stability reasons (occasionally, if a " +
                        $"user doesn't have an account, a bot function may not work for said user [such as with `$ctr`]).");
                    embed.WithColor(Pink);
                    BE(); break;
                case "rep":
                    embed.WithTitle($"Help: Rep | `{cmdPrefix}rep`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user in the server to add one reputation point to another member." +
                        $"\nThis can be done once every 24 hours, and can not be used on yourself. This rep will show on your Kaguya profile!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "rep author":
                case "repauthor":
                    embed.WithTitle($"Help: +Rep Author | `{cmdPrefix}repauthor` / `{cmdPrefix}rep author`");
                    embed.WithDescription($"{Context.User.Mention} Gives my creator your daily +rep point!");
                    embed.WithFooter($"We appreciate your generosity uwu | To give rep to another user, use $rep!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "author":
                    embed.WithTitle($"Help: Author | `{cmdPrefix}author`");
                    embed.WithDescription($"{Context.User.Mention} Displays information about my creator!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "timelyreset":
                    embed.WithTitle($"Help: Timely Reset | `{cmdPrefix}timelyreset`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAllows a bot owner to reset the {cmdPrefix}timely cooldown for every user in the Kaguya database.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "filteradd":
                case "fa":
                    embed.WithTitle($"Help: Filter Adding | `{cmdPrefix}filteradd` / `{cmdPrefix}fa`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to add a word or phrase to the list of filtered words for the server." +
                        $"\nSpaces may be used when adding a phrase to the filter. The filter is not case sensitive." +
                        $"\nExamples: `{cmdPrefix}fa Smelly Sushi`, `{cmdPrefix}fa frogs`");
                    embed.WithColor(Pink);
                    BE(); break;
                case "filterremove":
                case "fr":
                    embed.WithTitle($"Help: Filter Removing | `{cmdPrefix}filterremove` / `{cmdPrefix}fr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to remove a word or phrase from the list of filtered words for the server." +
                        $"\nSpaces may be used when removing a phrase from the filter. The filter is not case sensitive." +
                        $"\nExamples: `{cmdPrefix}fr Smelly Sashimi`, `{cmdPrefix}fr caterpillars`");
                    embed.WithColor(Pink);
                    BE(); break;
                case "filterview":
                case "fv":
                    embed.WithTitle($"Help: Viewing Filtered Words | `{cmdPrefix}filterview` / `{cmdPrefix}fv`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Messages**" +
                        $"\n" +
                        $"\nAllows viewing of all filtered words and phrases in the server. Ideally this would be used in a private \"Moderator\" channel.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "filterclear":
                case "clearfilter":
                    embed.WithTitle($"Help: Filter Clearing | `{cmdPrefix}filterclear` / `{cmdPrefix}clearfilter`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to remove ALL words and phrases from the server's list of filtered words/phrases." +
                        $"\nThis command does not take any parameters." +
                        $"\nExamples: `{cmdPrefix}filterclear`, `{cmdPrefix}clearfilter`");
                    embed.WithFooter("This action is dangerous and irreversible!");
                    embed.WithColor(Pink);
                    BE(); break;
                case "setlogchannel":
                case "log":
                    embed.WithTitle($"Help: Set Logging Channel | `{cmdPrefix}setlogchannel` / `{cmdPrefix}log`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to set the logging channel for a given log type. For example, if I want logs for " +
                        $"`DeletedMessages` to be sent in channel `message-logs`, I would type `{cmdPrefix}setlogchannel deletedmessages <#message-logs>`. " +
                        $"The channel for logs must be properly linked (where it shows up blue and you can click on the channel to be directed to it). " +
                        $"To set the logging channel for **all** log types, the proper usage would be `{cmdPrefix}setlogchannel all <#logging-channel>`" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}setlogchannel <logtype> <#logging-channel>`");
                    embed.WithFooter("To see all available log types, and to see what channel the log types are being sent to, use $logtypes");
                    embed.WithColor(Pink);
                    BE(); break;
                case "resetlogchannel":
                case "rlog":
                    embed.WithTitle($"Help: Resetting Logging Channels | `{cmdPrefix}resetlogchannel`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to reset (disable) the logging channel for a given log type." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}resetlogchannel <logtype>`" +
                        $"\nExample: `{cmdPrefix}resetlogchannel all` would disable all logging in the server.");
                    embed.WithFooter("To see all available log types, and to see what channel the log types are being sent to, use $logtypes");
                    embed.WithColor(Pink);
                    BE(); break;
                case "logtypes":
                case "loglist":
                    embed.WithTitle($"Help: Log Types | `{cmdPrefix}logtypes`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to see a list of all available log types. In addition to this, the channels that are " +
                        $"currently occupied by the specified logtype will be displayed. If the log type is not logging at all, it will not " +
                        $"show any channels after it.");
                    embed.WithFooter("Note to Server Admins: This command will put out the log list in the chat channel you call this command from.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "awardeveryone":
                case "awardall":
                    embed.WithTitle($"Help: Awarding Points | `{cmdPrefix}awardeveryone` / `{cmdPrefix}awardall`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAllows a bot owner to award a specified number of points to **all** users in their Kaguya database." +
                        $"\nThis can be a negative number, however it can not send a user's points below zero.");
                    embed.WithColor(Pink);
                    BE(); break;
                case "masspointsdistribute":
                    embed.WithTitle($"Help: Mass Distributing Points | `{cmdPrefix}masspointsdistribute`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user to mass redistribute all of their Kaguya Points evenly to the rest of the server. " +
                        $"Upon using this command, your points will be set to zero and they will have been evenly divided amongst everyone in the server. If you do not " +
                        $"have at least one point for every member in the server, the command will not be executed.");
                    embed.WithFooter("What a generous act!");
                    embed.WithColor(Pink);
                    BE(); break;


                default:
                    embed.WithDescription($"**{Context.User.Mention} \"{command}\" is not a valid command.**");
                    embed.WithColor(Pink);
                    BE(); break;
            }
        }

        [Command("help")] //help
        [Alias("h")]
        public async Task HelpCommand()
        {
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            embed.WithTitle("Commands List");
            embed.WithDescription($"All Kaguya commands separated by category. To use the command, have \nthe `{cmdPrefix}` symbol appended before the phrase. For more information on a specific command, " +
                $"type `{cmdPrefix}h <command>`");
            embed.AddField("Administration", "`kick [k]` \n`ban [b]` \n`masskick` \n`massban` \n`massblacklist` \n`unblacklist` \n`removeallroles [rar]` \n`createrole [cr]` \n`deleterole [dr]`" +
                "\n`clear [c] [purge]` \n`kaguyaexit` \n`scrapeserver` \n`filteradd [fa]` \n`filterremove [fr]` \n`filterview [fv]` \n`filterclear [clearfilter]` \n`setlogchannel [log]` \n`resetlogchannel [rlog]`" +
                "\n`logtypes [loglist]`", true);
            embed.AddField("Currency", "`points` \n`pointsadd [addpoints]` \n`timely [t]` \n`timelyreset` \n`groll [gr]` \n`awardeveryone [awardall]` \n`masspointsdistribute`", true);
            embed.AddField("EXP", "`exp` \n`expadd [addexp]` \n`level` \n`rep` \n`repauthor [rep author]` \n`serverexplb [explb]` \n`globalexplb [gexplb]`", true);
            embed.AddField("Fun", "`echo` \n`pick`", true);
            embed.AddField("Help", "`help [h]` \n`helpdm [hdm]`", true);
            embed.AddField("osu!", "`createteamrole [ctr]` \n`delteams` \n`osutop` \n`recent [r]` \n`osuset`", true);
            embed.AddField("Utility", "`modules [mdls]` \n`createtextchannel [ctc]` \n`deletetextchannel [dtc]` \n`createvoicechannel [cvc]` \n`deletevoicechannel [dvc]` \n`prefix` \n`author` \n`commands [cmds]`", true);
            embed.WithColor(Pink);
            embed.WithFooter($"For more information, including a link to add this bot to your server and a link to the Kaguya Support Discord, type {cmdPrefix}hdm!");
            BE();
        }

        [Command("helpdm")] //help
        [Alias("hdm")]
        public async Task HelpDM()
        {
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            BE();
            Context.User.SendMessageAsync($"Need help with a specific command? Type `{cmdPrefix}mdls` to see a list of categories the commands are listed under." +
                $"\nType `{cmdPrefix}commands <module name>` to see all commands listed under that module." +
                $"\nType `{cmdPrefix}h <command name>` for more how to use the command and a detailed description of what it does." +
                $"\nAdd me to your server with this link!: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=2146958847" +
                $"\nWant to keep track of all the changes or feel like self hosting? Feel free to check out the Kaguya Github page!: https://github.com/stageosu/Kaguya" +
                $"\nKaguya Support Server: https://discord.gg/yhcNC97");
        }
    }
}
