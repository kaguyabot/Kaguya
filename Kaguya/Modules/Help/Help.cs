﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EmbedType = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Help
{
    public class Help : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("h")] //The BIG fish
        [Alias("help")]
        public async Task HelpCommand([Remainder]string command)
        {

            var cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            switch (command.ToLower())
            {
                case "h":
                case "help":
                    embed.WithTitle($"Help: Help!! | `{cmdPrefix}h` / `{cmdPrefix}help`");
                    embed.WithDescription($"Shows the command list. If typed with the name of a command (Ex: `{cmdPrefix}help <command>`), the response will instead contain helpful information on the specified " +
                        $"command, including how to use it.");
                    await BE(); break;
                case "helpdm":
                case "hdm":
                    embed.WithTitle($"Help: HelpDM | `{cmdPrefix}helpdm`, `{cmdPrefix}hdm`");
                    embed.WithDescription($"{Context.User.Mention}Sends a DM with helpful information, including a link to add the bot to your own server, and a link to the Kaguya Github page!");
                    await BE(); break;
                case "warnset":
                case "ws":
                    embed.WithTitle($"Help: Warnset | `{cmdPrefix}warnset`, `{cmdPrefix}ws`");
                    embed.WithDescription($"{Context.User.Mention} Permissions Required: **Administrator**" +
                        $"\n" +
                        $"\nConfigures the server's warning punishments. You may `mute`, `kick`, `shadowban`, and `ban` users on the specified amount of warnings." +
                        $"\nFor example, if you use `{cmdPrefix}warnset 4 mute`, users who are warned for the fourth time will be muted. Same goes for the rest of the warning types." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}warnset <num> <warning punishment>`");
                    embed.WithFooter($"If you forget the warning punishments, you may use {cmdPrefix}warnoptions to see the list.");
                    await BE(); break;
                case "warn":
                case "w":
                    embed.WithTitle($"Help: Warn | `{cmdPrefix}warn`, `{cmdPrefix}w`");
                    embed.WithDescription($"{Context.User.Mention} Permissions Required: **Kick Members**" +
                        $"\n" +
                        $"\nWarns a user. This will add a (currently non-revocable) warning to a user (typically for a rule violation of some sort)." +
                        $"\nThe user will know they have been warned when this command is executed (they will be DM'd with who warned them)." +
                        $"\nIf a user reaches a certain number of warnings required to trigger a \"punishment\", they will be punished according to the server's `{cmdPrefix}warnset` configuration." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}warn <user {{ID, Name, Mention}}>`");
                    await BE(); break;
                case "warnoptions":
                case "wo":
                    embed.WithTitle($"Help: Warning Options | `{cmdPrefix}warnoptions`");
                    embed.WithDescription($"{Context.User.Mention} Displays available ways to punish users through my warning system." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}warnoptions`");
                    await BE(); break;
                case "warnpunishments":
                case "wp":
                    embed.WithTitle($"Help: Warning Configuration | `{cmdPrefix}warnpunishments`, `{cmdPrefix}wp`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Kick Members**" +
                        $"\n" +
                        $"\nDisplays what punishments have been set for the server and at how many warnings they will be triggered." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}warnpunishments`");
                    await BE(); break;
                case "inspect":
                    embed.WithTitle($"Help: User Inspection | `{cmdPrefix}inspect`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nDisplays all previous punishments for a user. This includes warns, kicks, and bans, as well as their " +
                        $"reason for being punished, who punished them, and the time at which they were punished. If a user is banned/shadowbanned/etc. " +
                        $"as a result of being warned a certain number of times, this will not be displayed. Kicks and bans are also only displayed " +
                        $"if the user was punished through my commands." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}inspect <user>`");
                    embed.WithFooter("This command only displays punishments made after Kaguya V1.32");
                    await BE(); break;
                case "toggleannouncements":
                    embed.WithTitle($"Help: Toggle Announcements | `{cmdPrefix}toggleannouncements`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nToggles the server's preference for in-chat level announcements." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}toggleannouncements`");
                    await BE(); break;
                case "bugreport":
                    embed.WithTitle($"Help: Bug Report | `{cmdPrefix}bug`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to report a bug directly to the support server's `#bugs` channel so that my creator can take a look at what's wrong (and hopefully fix it)! " +
                        $"Please use this feature whenever you feel something is wrong with Kaguya, but don't overdo it! Messages that are spammy, violate the Discord TOS, or are abusive/deragotory in nature will " +
                        $"result in a `permanent blacklist` from all of Kaguya. A bug report that leads to something getting fixed will result in `+2000 Kaguya points` added to your account on the next patch as a thank you!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}bugreport <message>`");
                    await BE(); break;
                case "kaguyawarn":
                    embed.WithTitle($"Help: Global Warnings | `{cmdPrefix}kaguyawarn`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAdds a Global Kaguya Warning to a user. Upon 3 warnings, the target will be blacklisted. This is generally for abusing Kaguya's features, such as the $bug report feature " +
                        $"or exploiting a Kaguya system (think EXP, points, etc.)" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}kaguyawarn <ID> <Reason>`");
                    embed.WithFooter($"Blacklists applied from warnings are not removed.");
                    await BE(); break;
                case "vote":
                    embed.WithTitle($"Help: Voting | `{cmdPrefix}vote`");
                    embed.WithDescription($"{Context.User.Mention} I will reply with a link to my discordbots.org page. If you wish to support me and want more people to have the ability to use me, " +
                        $"give me an upvote! My creator and I greatly appreciate it uwu. After voting, use `{cmdPrefix}voteclaim` to get some rewards for your support!");
                    embed.WithFooter($"Use \"{cmdPrefix}h voteclaim\" to find out what the rewards for upvoting are!");
                    await BE(); break;
                case "voteclaim":
                case "vc":
                    embed.WithTitle($"Help: Claiming Voting Rewards | `{cmdPrefix}voteclaim`");
                    embed.WithDescription($"{Context.User.Mention} Use this command after voting (see `{cmdPrefix}h vote`) to have some rewards applied to your Kaguya account!" +
                        $"\nRewards: `2x critical hit chance for 12 hours` and `500 Kaguya Points`! You may ask \"well, what's a critical?\" I have a help command for that! Use `{cmdPrefix}h critical` to find out more!");
                    await BE(); break;
                case "critical":
                    embed.WithTitle($"Help: Critical Hits");
                    embed.WithDescription($"{Context.User.Mention} No matter what currency related command you use (roll, timely, weekly, etc.), there is a chance that reward can be a \"critical\"." +
                        $"\nThe critical factor will greatly multiply your point rewards." +
                        $"\n" +
                        $"\nCritical Rewards:" +
                        $"\n" +
                        $"\nRolls: `5% chance that the multiplier of your bet is multiplied by 2.5x`" +
                        $"\nQuickdraw: `2% chance that you will receive ~2.89x your bet.`" +
                        $"\nTimely: `14% chance that the value of your reward is multiplied by 3.5x`" +
                        $"\nWeekly: `8% chance that the value of your reward will be multiplied by 3.5x`" +
                        $"\n" +
                        $"\nIf you have successfully used `{cmdPrefix}voteclaim` within the last 12 hours, these percentages are doubled.");
                    await BE(); break;
                case "exp":
                    embed.WithTitle($"Help: EXP | `{cmdPrefix}exp`");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}exp`." +
                        $"\nReturns the value of experience points the user has in their account, as well as their level. " +
                        $"Use this command with no parameters to return one's own information." +
                        $"\nSyntax: `{cmdPrefix}exp`, `{cmdPrefix}exp <user>`");
                    await BE(); break;
                case "expadd":
                case "addexp":
                    embed.WithTitle($"Help: Adding Experience Points | `{cmdPrefix}expadd` / `{cmdPrefix}addexp`");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n{Context.User.Mention} Adds EXP points to the specified user. The number of exp you are adding must be a positive whole number." +
                        $"\nSyntax: `{cmdPrefix}expadd <number of experience points to add> <User {{ID, Name, Mention}}>`.");
                    await BE(); break;
                case "points":
                    embed.WithTitle($"Help: Kaguya Points Balance | `{cmdPrefix}points`");
                    embed.WithDescription($"Returns the value of points a user has in their account." +
                        $"\nSyntax: `{cmdPrefix}points {{This returns the amount of points you have in your account}}`" +
                        $"\nSyntax: `{cmdPrefix}points <user> {{This returns the amount of points someone else has in their account.}}`");
                    await BE(); break;
                case "profile":
                case "p":
                    embed.WithTitle($"Help: Profile | `{cmdPrefix}profile`, `{cmdPrefix}p`");
                    embed.WithDescription($"\n{Context.User.Mention} Shows you all of your Kaguya stats at once!" +
                        $"\nSyntax: `{cmdPrefix}profile`");
                    await BE(); break;
                case "pointsadd":
                case "addpoints":
                    embed.WithTitle($"Help: Adding Points | `{cmdPrefix}pointsadd`, `{cmdPrefix}addpoints`");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n{Context.User.Mention} Adds points to the specified user's kaguya account. The number of points you are adding must be a positive whole number." +
                        $"\nSyntax: `{cmdPrefix}pointsadd <number of points to add> <User {{ID, Name, Mention}}>`.");
                    await BE(); break;
                case "stats":
                    embed.WithTitle($"Help: Stats | `{cmdPrefix}stats`");
                    embed.WithDescription($"{Context.User.Mention} Displays some interesting Kaguya Statistics!");
                    await BE(); break;
                case "ping":
                    embed.WithTitle($"Help: Ping | `{cmdPrefix}ping`");
                    embed.WithDescription($"{Context.User.Mention} Displays Discord's latency in real time.");
                    await BE(); break;
                case "createtextchannel":
                case "ctc":
                    embed.WithTitle($"Help: Creating Text Channels | `{cmdPrefix}createtextchannel`, `{cmdPrefix}ctc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a text channel with the speficied name. " +
                        $"\nSyntax: `{cmdPrefix}createtextchannel <channel name>`. " +
                        $"\nThis name can have spaces. Example: `{cmdPrefix}createtextchannel testing 123`.");
                    await BE(); break;
                case "deletetextchannel":
                case "dtc":
                    embed.WithTitle($"Help: Deleting Text Channels | `{cmdPrefix}deletetextchannel`, `{cmdPrefix}dtc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a text channel with the speficied name. " +
                        $"\nThis name can **not** have spaces. Type the text channel exactly as displayed; If the text channel contains a `-`, type that in." +
                        $"\nSyntax: `{cmdPrefix}deletetextchannel <channel name>`." +
                        $"Example: `{cmdPrefix}deletetextchannel super-long-name-with-lots-of-spaces`.");   
                    await BE(); break;
                case "createvoicechannel":
                case "cvc":
                    embed.WithTitle($"Help: Creating Voice Channels | `{cmdPrefix}createvoicechannel`, `{cmdPrefix}cvc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a voice channel with the speficied name." +
                        $"\nThis name can have spaces." +
                        $"\nSyntax: `{cmdPrefix}createvoicechannel <channel name>`.");
                    await BE(); break;
                case "deletevoicechannel":
                case "dvc":
                    embed.WithTitle($"Help: Deleting Voice Channels | `{cmdPrefix}deletevoicechannel`, `{cmdPrefix}dvc");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a voice channel with the speficied name. " +
                        $"\nThis name can have spaces. Replace the `-` symbols with spaces." +
                        $"\nSyntax: `{cmdPrefix}deletevoicechannel <channel name>`." +
                        $"Example: `{cmdPrefix}deletevoicechannel super long name with lots of spaces`.");
                    await BE(); break;
                case "createrole":
                case "cr":
                    embed.WithTitle($"Help: Creating Roles | `{cmdPrefix}createrole`, `{cmdPrefix}cr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nCreates a role with the specified name. The role will have no special permissions or colors." +
                        $"\nSyntax: `{cmdPrefix}createrole <name>`, `{cmdPrefix}cr <name>`.");
                    await BE(); break;
                case "inrole":
                    embed.WithTitle($"Help: Inrole | `{cmdPrefix}inrole`");
                    embed.WithDescription($"{Context.User.Mention} Shows a list of up to 70 members with the role specified (in alphabetical order)." +
                        $"\nSyntax: `{cmdPrefix}inrole <roleName>`");
                    await BE(); break;
                case "antiraid":
                    embed.WithTitle($"Help: Anti-Raid Service | `{cmdPrefix}antiraid`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nBegins an interactive setup of the Kaguya Anti-Raid service." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}antiraid`");
                    await BE(); break;
                case "autoassign":
                case "aa":
                    embed.WithTitle($"Help: Auto Assign Roles | `{cmdPrefix}autoassign`, `{cmdPrefix}aa`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAdds a role to a list of auto-assignable roles. " +
                        $"An auto assigned role is a role that is given to a new user whenever they join the server." +
                        $"\nSyntax: `{cmdPrefix}aa <role>`.");
                    await BE(); break;
                case "autoassignremove":
                case "aar":
                    embed.WithTitle($"Help: Auto Assign Roles | `{cmdPrefix}autoassignremove`, `{cmdPrefix}aar`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nRemoves a role from the list of auto-assignable roles. " +
                        $"An auto assigned role is a role that is given to a new user whenever they join the server." +
                        $"\nSyntax: `{cmdPrefix}aar <role>`.");
                    await BE(); break;
                case "autoassignclear":
                case "aac":
                    embed.WithTitle($"Help: Auto Assign Roles | `{cmdPrefix}autoassignclear`, `{cmdPrefix}aac`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nRemoves all roles from the list of auto-assignable roles. " +
                        $"An auto assigned role is a role that is given to a new user whenever they join the server." +
                        $"\nSyntax: `{cmdPrefix}aac`.");
                    await BE(); break;
                case "autoassignview":
                case "aav":
                    embed.WithTitle($"Help: Auto Assign Roles | `{cmdPrefix}autoassignview`, `{cmdPrefix}aav`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nDisplays the list of auto assigned roles for the server. " +
                        $"\nSyntax: `{cmdPrefix}aav`.");
                    await BE(); break;
                case "echo":
                    embed.WithTitle($"Help: Echoed Messages | `{cmdPrefix}echo`");
                    embed.WithDescription($"{Context.User.Mention} Makes the bot repeat anything you say!" +
                        $"\nSyntax: `{cmdPrefix}echo <message>`.");
                    await BE(); break;
                case "pick":
                    embed.WithTitle($"Help: Pick | `{cmdPrefix}pick`");
                    embed.WithDescription($"{Context.User.Mention} Tells the bot to pick between any amount of options, randomly." +
                        $"\nSyntax: `{cmdPrefix}pick option1|option2|option3|option4`...etc." +
                        $"\nYou may have as many \"Options\" as you'd like!" +
                        $"\nThe bot will always pick with totally random odds.");
                    await BE(); break;
                case "tictactoe":
                case "ttt":
                    embed.WithTitle($"Help: Tic Tac Toe! | `{cmdPrefix}tictactoe`, `{cmdPrefix}ttt`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to play Tic Tac Toe with a friend!" +
                        $"\n" +
                        $"\nInitiate a Tic Tac Toe game by typing \n`{cmdPrefix}ttt {{@User#0000 / User ID / Username}}`." +
                        $"\nCurrently, the person who initiates the game will go first." +
                        $"\n" +
                        $"\nHow to play:" +
                        $"\n" +
                        $"\nThe first person to get three in a row (vertically, horizontally, or diagonally) wins! " +
                        $"To make your move, simply reply in chat with your move. An example move could be `3A`, `2B`, or `1C`. " +
                        $"Moves always begin with a number between 1 and 3, and end with a letter between A and C. You do not type " +
                        $"the command prefix `{cmdPrefix}` when typing your move." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}ttt <{{@User#0000 / User ID / Username}}>`" +
                        $"\nSyntax: `{cmdPrefix}tictactoe <{{@User#0000 / User ID / Username}}>`");
                    await BE(); break;
                case "8ball":
                    embed.WithTitle($"Help: Magic 8Ball | `{cmdPrefix}8ball`");
                    embed.WithDescription($"{Context.User.Mention} Ask Kaguya a question and she will use her divine powers to answer you extremely accurately!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}8ball <question>`");
                    await BE(); break;
                case "slap":
                    embed.WithTitle($"Help: Slapping! | `{cmdPrefix}slap`");
                    embed.WithDescription($"{Context.User.Mention} Slap someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}slap <word>`");
                    await BE(); break;
                case "hug":
                    embed.WithTitle($"Help: Hugging! | `{cmdPrefix}hug`");
                    embed.WithDescription($"{Context.User.Mention} hug someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}hug <word>`");
                    await BE(); break;
                case "kiss":
                    embed.WithTitle($"Help: Kissing! | `{cmdPrefix}kiss`");
                    embed.WithDescription($"{Context.User.Mention} Hug someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}kiss <word>`");
                    await BE(); break;
                case "pat":
                    embed.WithTitle($"Help: Patting! | `{cmdPrefix}pat`");
                    embed.WithDescription($"{Context.User.Mention} pat someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}pat <word>`");
                    await BE(); break;
                case "poke":
                    embed.WithTitle($"Help: Poking! | `{cmdPrefix}poke`");
                    embed.WithDescription($"{Context.User.Mention} Poke someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}poke <word>`");
                    await BE(); break;
                case "tickle":
                    embed.WithTitle($"Help: Tickling! | `{cmdPrefix}tickle`");
                    embed.WithDescription($"{Context.User.Mention} Tickle someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}tickle <word>`");
                    await BE(); break;
                case "baka":
                    embed.WithTitle($"Help: Baka | `{cmdPrefix}baka`");
                    embed.WithDescription($"{Context.User.Mention} Someone said something stupid? Show them how much of a baka they are with the baka command! An emotionally-accurate gif will be posted in chat." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}baka`");
                    await BE(); break;
                case "nekoavatar":
                    embed.WithTitle($"Help: Neko Avatar | `{cmdPrefix}nekoavatar`");
                    embed.WithDescription($"{Context.User.Mention} Generates a Neko Avatar for you!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}nekoavatar`");
                    await BE(); break;
                case "smug":
                    embed.WithTitle($"Help: Smug | `{cmdPrefix}smug`");
                    embed.WithDescription($"{Context.User.Mention} Posts a \"smug\" gif in chat." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}smug`");
                    await BE(); break;
                case "waifu":
                    embed.WithTitle($"Help: Waifu! | `{cmdPrefix}waifu`");
                    embed.WithDescription($"{Context.User.Mention} Posts an image of a waifu in chat." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}waifu`");
                    await BE(); break;
                case "wallpaper":
                    embed.WithTitle($"Help: Wallpaper! | `{cmdPrefix}wallpaper`");
                    embed.WithDescription($"{Context.User.Mention} An anime wallpaper will be posted in chat!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}wallpaper`");
                    await BE(); break;
                case "timely":
                case "t":
                    embed.WithTitle($"Help: Timely Points | `{cmdPrefix}timely`, `{cmdPrefix}t`");
                    embed.WithDescription($"{Context.User.Mention} The timely command allows any user to claim 500 free points every 24 hours." +
                        "\nThese points are added to your Kaguya account. The timely command has a `6%` chance of landing a critical hit, " +
                        "multiplying your reward by `3.50x`." +
                        $"\nSyntax: `{cmdPrefix}timely`");
                    await BE(); break;
                case "weekly":
                    embed.WithTitle($"Help: Weekly Points | `{cmdPrefix}weekly`");
                    embed.WithDescription($"{Context.User.Mention} The weekly command allows any user to claim 5,000 points every week." +
                        "\nThese points are automatically added to your Kaguya account. The weekly command has a `8%` chance to land a critical hit, multiplying " +
                        "your reward by `3.50x`." +
                        $"\nSyntax: `{cmdPrefix}weekly`");
                    await BE(); break;
                case "weeklyreset":
                    embed.WithTitle($"Help: Weekly Reset | `{cmdPrefix}weeklyreset`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nResets the weekly cooldown for every user in the Kaguya database." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}weeklyreset`");
                    await BE(); break;
                case "clear":
                case "purge":
                case "c":
                    embed.WithTitle($"Help: Clearing Messages | `{cmdPrefix}clear`, `{cmdPrefix}purge`, `{cmdPrefix}c`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Messages**" +
                        $"\n" +
                        $"\nDeletes a specified number of messages in a given channel. This number may not exceed `100`. Messages older than two weeks will need to be deleted manually." +
                        $"\nSyntax: `{cmdPrefix}clear <num>`, `{cmdPrefix}purge <num>`, {cmdPrefix}c <num>");
                    await BE(); break;
                case "kick":
                case "k":
                    embed.WithTitle($"Help: Kicking Users | `{cmdPrefix}kick`, `{cmdPrefix}k`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Kick Members**" +
                        $"\n" +
                        $"\nKicks an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}kick @User#0000`.");
                    await BE(); break;
                case "mute":
                    embed.WithTitle($"Help: Muting Users | `{cmdPrefix}mute`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Mute Members, Manage Roles**" +
                        $"\n" +
                        $"\nMutes an individual user or a list of users from the server. This command may also have a time associated with it " +
                        $"so that you may \"mass mute\" members for a length of time, or indefinitely. This time may go as far as thousands of days. " +
                        $"Examples of time formats: `12s10m1h3d` for 3 days, 1 hour, 10 minutes, and one second. `1m30s`, `24h`, `15m23s18h` are all valid time formats, " +
                        $"they can be in any order! Muting a user will apply the `kaguya-mute` role to them. The mute command will create this role automatically on first time use. " +
                        $"The `kaguya-mute` role will deny the \"Add Reactions\" and \"Send Messages\" permissions to any users that have the role." +
                        $"\n" +
                        $"\nSyntax `<Required parameter>, [Optional parameter]`: " +
                        $"\n`{cmdPrefix}mute [time {{<Num>s<Num>m<Num>h<Num>d}}] <list of users {{IDs, Username, or Mention}}>`");
                    await BE(); break;
                case "unmute":
                    embed.WithTitle($"Help: Un-Muting Users | `{cmdPrefix}unmute`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Mute Members, Manage Roles**" +
                        $"\n" +
                        $"\nUnmutes a user. This removes the \"kaguya-mute\" role from the user, if they have it." +
                        $"\nSyntax: `{cmdPrefix}unmute <user>`.");
                    await BE(); break;
                case "shadowban":
                    embed.WithTitle($"Help: Shadowbanning Users | `{cmdPrefix}shadowban`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nShadowbans an individual member from the server, blocking all access to all channels. All permissions " +
                        $"for this user, in every channel, will be denied. Their roles will remain, however." +
                        $"\nSyntax: `{cmdPrefix}shadowban @User#0000`.");
                    await BE(); break;
                case "unshadowban":
                    embed.WithTitle($"Help: Un-Shadowbanning Users | `{cmdPrefix}unshadowban`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nUn-Shadowbans an individual member from the server, reinstating all access to all channels. All permissions " +
                        $"for this user, in every channel, will be set to default (the user is neither allowed or denied any explicit permissions)." +
                        $"\nSyntax: `{cmdPrefix}unshadowban @User#0000`.");
                    await BE(); break;
                case "ban":
                case "b":
                    embed.WithTitle($"Help: Banning Users | `{cmdPrefix}ban`, `{cmdPrefix}b`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nBans an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}ban @User#0000`.");
                    await BE(); break;
                case "massban":
                    embed.WithTitle($"Help: Mass Banning of Users | `{cmdPrefix}massban`");
                    embed.WithDescription($"**{Context.User.Mention} Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nTakes a list of mentioned users and permanently bans them simultaneously." +
                        $"\nSyntax: `{cmdPrefix}massban @mentioneduser#0001 @otheruser#0002 @smellysushi#2623 [...]`");
                    await BE(); break;
                case "masskick":
                    embed.WithTitle($"Help: Mass Kicking of Users | `{cmdPrefix}masskick`");
                    embed.WithDescription($"**{Context.User.Mention} Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nTakes a list of mentioned users and kicks them simultaneously." +
                        $"\nSyntax: `{cmdPrefix}masskick @bullyHunter#0001 @stinkysushi#0002 @smellysushi#2623 [...]`");
                    await BE(); break;
                case "removeallroles":
                case "rar":
                    embed.WithTitle($"Help: Removing All Roles | `{cmdPrefix}removeallroles`, `{cmdPrefix}rar`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        "\n" +
                        "\nRemoves all roles from the specified user." +
                        $"\nSyntax: `{cmdPrefix}removeallroles @User#0000`.");
                    await BE(); break;
                case "removerole":
                case "rr":
                    embed.WithTitle($"Help: Removing Roles | `{cmdPrefix}removerole`, `{cmdPrefix}rr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        "\n" +
                        "\nRemoves the role from the specified user(s). This may be a list of users!" +
                        $"\nSyntax: `{cmdPrefix}removerole <role> <@User#0000>`." +
                        $"\nSyntax: `{cmdPrefix}rr <role> <Name / ID / @User#0000> [any subsequent users]`");
                    await BE(); break;
                case "addrole":
                case "ar":
                    embed.WithTitle($"Help: Adding Roles | `{cmdPrefix}addrole`, `{cmdPrefix}ar`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        "\n" +
                        "\nAdds the role to the specified user(s)." +
                        $"\nSyntax: `{cmdPrefix}addrole @User#0000`." +
                        $"\nSyntax: `{cmdPrefix}ar <Name> <ID> <@Name#0000> <20945832042384>`");
                    await BE(); break;
                case "deleterole":
                case "dr":
                    embed.WithTitle($"Help: Deleting Roles | `{cmdPrefix}deleterole`, `{cmdPrefix}dr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nDeletes a role from the server (and in the process, removes said role from everyone who had it). " +
                        $"If multiple matches of the same role are found, the bot will delete all occurrences of said role." +
                        $"\nSyntax: `{cmdPrefix}deleterole <role name>`");
                    await BE(); break;
                case "osu":
                    embed.WithTitle($"Help: osu! | `{cmdPrefix}osu`");
                    embed.WithDescription($"{Context.User.Mention} Presents lots of statistics from the given osu! profile name. If your `{cmdPrefix}osuset` username " +
                        $"is set, you may use `{cmdPrefix}osu` by itself. Otherwise, you must specify a name afterward." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}osu`, `{cmdPrefix}osu <username>`");
                    await BE(); break;
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
                    await BE(); break;
                case "osutop":
                    embed.WithTitle($"Help: osu! Top | `{cmdPrefix}osutop`");
                    embed.WithDescription($"\n" +
                        $"\n{Context.User.Mention} Displays the specified amount of top osu! plays for a given player with other relevant information." +
                        $"\nThe number of requested plays to display may not be more than 7." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}osutop 5 Stage` | `{cmdPrefix}osutop 7 Smelly sushi`");
                    await BE(); break;
                case "osutop -n":
                    embed.WithTitle($"Help: osu! Top Extension: -n | `{cmdPrefix}osutop -n`");
                    embed.WithDescription($"{Context.User.Mention} Displays the specified top play for a given player. If the player variable is left blank, " +
                        $"I will use the username specified when you used `{cmdPrefix}osuset`. The number for the play you want to search for must be between 1 and 100, " +
                        $"because there are only 100 top plays on someone's profile." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}osutop -n <num> [player]`" +
                        $"\nExamples: `{cmdPrefix}osutop -n 75`, `{cmdPrefix}osutop -n 90 nathan on osu`");
                    await BE(); break;
                case "delteams":
                    embed.WithTitle($"Help: Deleting Teams | `{cmdPrefix}delteams`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: `Manage Roles`, `Administrator`, `Bot Owner`**" +
                        $"\n" +
                        $"\nDeletes all team roles. A team role is any role that has the word \"Team: \" inside of it (with the space)." +
                        $"\nThis command will delete ALL team roles upon execution, making this command dangerous and irreversable.");
                    await BE(); break;
                case "recent":
                case "r":
                    embed.WithTitle($"Help: osu! Recent | `{cmdPrefix}r` / `{cmdPrefix}recent`");
                    embed.WithDescription($"{Context.User.Mention} Displays the most recent osu! play for the given user. If there is no user specified," +
                        $" the bot will use the osu! username that was specified to the command executor's Kaguya account (through {cmdPrefix}osuset).\n" +
                        $"As of right now, no response will be given for an invalid username.\n");
                    await BE(); break;
                case "osuset":
                    string name = Context.User.Username;
                    embed.WithTitle($"Help: osuset | `{cmdPrefix}osuset`");
                    embed.WithDescription($"{Context.User.Mention} Adds an osu! username to your Kaguya account! Setting your osu! username allows you to use all osu! related commands without any additional " +
                        $"parameters. For example, instead of typing `{cmdPrefix}osutop {name}`, you can now just type `{cmdPrefix}osutop` to get your most recent osu! plays. Same thing for `{cmdPrefix}r` / `{cmdPrefix}recent`!");
                    embed.WithFooter("Ensure your username is spelled properly, otherwise all osu! related commands will not work for you!");
                    await BE(); break;
                case "userblacklist":
                case "ubl":
                    embed.WithTitle($"Help: User Mass Blacklist | `{cmdPrefix}userblacklist`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nA bot owner may execute this command on a list of users they deem unworthy of being able to ever use Kaguya again." +
                        $"These users will have all of their EXP and Points reset to zero, and will be permanently filtered from receiving EXP and executing Kaguya commands." +
                        $"\nSyntax: `{cmdPrefix}ubl <list of user IDs>`");
                    await BE(); break;
                case "userunblacklist":
                case "uubl":
                    embed.WithTitle($"Help: Unblacklisting Users | `{cmdPrefix}userunblacklist`, `{cmdPrefix}ubl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nUnblacklists the specified user IDs." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}uubl <params ulong[] IDs>`");
                    await BE(); break;
                case "reloadconfig":
                    embed.WithTitle($"Help: Reload Config | `{cmdPrefix}reloadconfig`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nReloads all user accounts and servers into memory. This should be used if these files are " +
                        $"altered and you don't want to restart the bot." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}reloadconfig`");
                    await BE(); break;
                case "roll":
                case "gr":
                    embed.WithTitle($"Help: Betting | `{cmdPrefix}roll` / `{cmdPrefix}gr`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to roll the dice and gamble your points!" +
                        $"\n" +
                        $"\nA roll between `0-66` will result in a loss of your bet. " +
                        $"A roll between `67-78` will return your bet back to you with a multiplier of `1.7x`" +
                        $"\nRolls between `79-89`, `90-95`, `96-99`, and `100` will yield multipliers of `2.5x`, `3x`, `4.25x`, and `6x` respectively." +
                        $"\n" +
                        $"\nThe maximum amount of points you can gamble at one time is set to `25,000`." +
                        $"\n" +
                        $"\nIn addition, all rolls have a `5%` chance of landing a critical hit, multiplying the `multiplier` of the roll by `2.50x` (except for a 100 roll). " +
                        $"The best possible roll is a `critical 100`, multiplying your bet by `30x` (The odds of this are `1 / 2,000` or `0.05%`.)" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}roll <points>`");
                    await BE(); break;
                case "history":
                case "gh":
                    embed.WithTitle($"Help: Gambling History | `{cmdPrefix}history`, `{cmdPrefix}gh`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user to see their 10 most recent Kaguya gambles.");
                    await BE(); break;
                case "quickdraw":
                case "qd":
                    embed.WithTitle($"Help: Quickdraw Game | `{cmdPrefix}quickdraw`, `{cmdPrefix}qd`");
                    embed.WithDescription($"{Context.User.Mention} Quickdraw is a game of speed: " +
                        $"you and I will face off in a quickdraw battle to see who can pull out their pistol the " +
                        $"fastest, over a wager of course." +
                        $"\n" +
                        $"\nThe winner is whoever was fastest, all times are recorded down to the one-thousandth of a second." +
                        $"\n" +
                        $"\nIf you lose, you lose `100%` of your points." +
                        $"\nIf you win, you win `1.80x` of whatever you bet." +
                        $"\nIn the rare event that you strike a critical hit on a win, you will be awarded `~4.05x` your bet." +
                        $"\nThis game's `critical hit rate` is `8%`." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}qd <points>`");
                    await BE(); break;
                case "kaguyaexit":
                    embed.WithTitle($"Help: Kaguya Exit! | `{cmdPrefix}kaguyaexit`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAdministrator only command that forces Kaguya to leave the current server.");
                    await BE(); break;
                case "prefix":
                    embed.WithTitle($"Help: Prefix Alteration | `{cmdPrefix}prefix`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to change the bot's command prefix. Typically, this is one or two symbols `(!, $, %, >, etc.)`." +
                        $"\nTo reset the command prefix, type {cmdPrefix}prefix, or tag me and type `prefix`! The bot will always display the last known command prefix " +
                        $"and the new prefix when using this command.");
                    await BE(); break;
                case "channelwhitelist":
                case "cwl":
                    embed.WithTitle($"Help: Channel Whitelisting | `{cmdPrefix}channelwhitelist`, `{cmdPrefix}cwl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to whitelist a channel (or multiple, subsequent channels). Whitelisted " +
                        $"channels are the only channels that Kaguya will respond to commands from. Using this command clears " +
                        $"any blacklisted channels this server has specified, so keep that in mind. Administrators are immune to whitelists." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}channelwhitelist #<channel>`" +
                        $"\nSyntax: `{cmdPrefix}cwl <channel-name/ID>`");
                    await BE(); break;
                case "channelblacklist":
                case "cbl":
                    embed.WithTitle($"Help: Channel Blacklisting | `{cmdPrefix}channelblacklist`, `{cmdPrefix}cbl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to blacklist a channel (or multiple, subsequent channels). Blacklisted " +
                        $"channels are channels that Kaguya will not respond to. Use of this command clears any whitelisted channels " +
                        $"in the server. Administrators are immune to blacklists." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}channelblacklist #<channel>`" +
                        $"\nSyntax: `{cmdPrefix}cbl <channel-name/ID>`");
                    await BE(); break;
                case "channelunblacklist":
                case "cubl":
                    embed.WithTitle($"Help: Channel Un-Blacklisting | `{cmdPrefix}channelunblacklist`, `{cmdPrefix}cubl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to un-blacklist a channel (or multiple, subsequent channels). Blacklisted " +
                        $"channels are channels that Kaguya will not respond to. Use this command to re-allow command execution in the specified channel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}channelunblacklist #<channel>`" +
                        $"\nSyntax: `{cmdPrefix}cubl <channel-name/ID>`");
                    await BE(); break;
                case "channelunwhitelist":
                case "cuwl":
                    embed.WithTitle($"Help: Channel Un-Whitelisting | `{cmdPrefix}channelunwhitelist`, `{cmdPrefix}cuwl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to un-whitelist a channel (or multiple, subsequent channels). Whitelisted " +
                        $"channels are channels that Kaguya will isolate for command responses. Use of this command revokes Kaguya's " +
                        $"ability to send messages in the specified channel. " +
                        $"\n" +
                        $"\nNote: This is not the same as blacklisting a channel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}channelunwhitelist #<channel>`" +
                        $"\nSyntax: `{cmdPrefix}cuwl <channel-name/ID>`");
                    await BE(); break;
                case "whitelist":
                case "wl":
                    embed.WithTitle($"Help: Server Whitelist | `{cmdPrefix}whitelist`, `{cmdPrefix}wl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nDisplays any whitelisted channels for the server.");
                    await BE(); break;
                case "blacklist":
                case "bl":
                    embed.WithTitle($"Help: Server Blacklist | `{cmdPrefix}blacklist`, `{cmdPrefix}bl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nDisplays any blacklisted channels for the server.");
                    await BE(); break;
                case "serverblacklist":
                case "sbl":
                    embed.WithTitle($"Help: Owner Guild Blacklist | `{cmdPrefix}serverblacklist`, `{cmdPrefix}sbl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nBlacklists the specified server ID." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}sbl <params ulong[] IDs>`");
                    await BE(); break;
                case "serverunblacklist":
                case "subl":
                    embed.WithTitle($"Help: Owner Guild Un-Blacklist | `{cmdPrefix}serverunblacklist`, `{cmdPrefix}subl`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nUn-Blacklists the specified server ID." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}subl <params ulong[] IDs>`");
                    await BE(); break;
                case "serverexplb":
                case "explb":
                    embed.WithTitle($"Help: Server EXP Leaderboard | `{cmdPrefix}serverexplb` / `{cmdPrefix}explb`");
                    embed.WithDescription($"{Context.User.Mention} Displays the 10 top EXP holders in the server. This command " +
                        $"also displays their level.");
                    await BE(); break;
                case "globalexplb":
                case "gexplb":
                    embed.WithTitle($"Help: Global EXP Leaderboard | `{cmdPrefix}globalexplb` / `{cmdPrefix}gexplb`");
                    embed.WithDescription($"{Context.User.Mention} Displays the 10 top EXP holders in the entire Kaguya database! This command " +
                        $"also displays their level.");
                    await BE(); break;
                case "fact":
                    embed.WithTitle($"Help: Random Facts | `{cmdPrefix}fact`");
                    embed.WithDescription($"{Context.User.Mention} Displays a random fact in chat!");
                    await BE(); break;
                case "bugaward":
                    embed.WithTitle($"Help: Bug Rewards | `{cmdPrefix}bugaward`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nDMs the target and adds 2,000 Kaguya Points to their account. This is the reward for a `$bug` report that directly led to a patch/fix.");
                    await BE(); break;
                case "rep":
                    embed.WithTitle($"Help: Rep | `{cmdPrefix}rep`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user in the server to add one reputation point to another member." +
                        $"\nThis can be done once every 24 hours, and can not be used on yourself. This rep will show on your Kaguya profile!");
                    await BE(); break;
                case "rep author":
                case "repauthor":
                    embed.WithTitle($"Help: +Rep Author | `{cmdPrefix}repauthor` / `{cmdPrefix}rep author`");
                    embed.WithDescription($"{Context.User.Mention} Gives my creator your daily +rep point!");
                    embed.WithFooter($"We appreciate your generosity uwu | To give rep to another user, use $rep!");
                    await BE(); break;
                case "author":
                    embed.WithTitle($"Help: Author | `{cmdPrefix}author`");
                    embed.WithDescription($"{Context.User.Mention} Displays information about my creator!");
                    await BE(); break;
                case "timelyreset":
                    embed.WithTitle($"Help: Timely Reset | `{cmdPrefix}timelyreset`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAllows a bot owner to reset the {cmdPrefix}timely cooldown for every user in the Kaguya database." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}timelyreset`");
                    await BE(); break;
                case "filteradd":
                case "fa":
                    embed.WithTitle($"Help: Filter Adding | `{cmdPrefix}filteradd` / `{cmdPrefix}fa`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to add a word or phrase to the list of filtered words for the server." +
                        $"\nSpaces may be used when adding a phrase to the filter. The filter is not case sensitive." +
                        $"\n" +
                        $"\nNote: **Kaguya's filter is also a wildcard filter, which means any message that contains what's filtered will be deleted. " +
                        $"Example: if you filter \"`https://www.twitch.tv/\", this will delete all twitch links.**" +
                        $"\nExamples: `{cmdPrefix}fa Smelly Sushi`, `{cmdPrefix}fa frogs`");
                    await BE(); break;
                case "filterremove":
                case "fr":
                    embed.WithTitle($"Help: Filter Removing | `{cmdPrefix}filterremove` / `{cmdPrefix}fr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to remove a word or phrase from the list of filtered words for the server." +
                        $"\nSpaces may be used when removing a phrase from the filter. The filter is not case sensitive." +
                        $"\nExamples: `{cmdPrefix}fr Smelly Sashimi`, `{cmdPrefix}fr caterpillars`");
                    await BE(); break;
                case "filterview":
                case "fv":
                    embed.WithTitle($"Help: Viewing Filtered Words | `{cmdPrefix}filterview` / `{cmdPrefix}fv`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Messages**" +
                        $"\n" +
                        $"\nAllows viewing of all filtered words and phrases in the server. Ideally this would be used in a private \"Moderator\" channel.");
                    await BE(); break;
                case "filterclear":
                case "clearfilter":
                    embed.WithTitle($"Help: Filter Clearing | `{cmdPrefix}filterclear` / `{cmdPrefix}clearfilter`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to remove ALL words and phrases from the server's list of filtered words/phrases." +
                        $"\nThis command does not take any parameters." +
                        $"\nExamples: `{cmdPrefix}filterclear`, `{cmdPrefix}clearfilter`");
                    embed.WithFooter("This action is dangerous and irreversible!");
                    await BE(); break;
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
                    await BE(); break;
                case "resetlogchannel":
                case "rlog":
                    embed.WithTitle($"Help: Resetting Logging Channels | `{cmdPrefix}resetlogchannel`, `{cmdPrefix}rlog`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to reset (disable) the logging channel for a given log type." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}rlog <logtype>`" +
                        $"\nExample: `{cmdPrefix}resetlogchannel all` would disable all logging in the server.");
                    embed.WithFooter("To see all available log types, and to see what channel the log types are being sent to, use $logtypes");
                    await BE(); break;
                case "logtypes":
                case "loglist":
                    embed.WithTitle($"Help: Log Types | `{cmdPrefix}logtypes`, `{cmdPrefix}loglist`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to see a list of all available log types. In addition to this, the channels that are " +
                        $"currently occupied by the specified logtype will be displayed. If the log type is not logging at all, it will not " +
                        $"show any channels after it." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}logtypes`" +
                        $"\nSyntax: `{cmdPrefix}loglist`");
                    embed.WithFooter("Note to Server Admins: This command will put out the log list in the chat channel you call this command from.");
                    await BE(); break;
                case "awardeveryone":
                case "awardall":
                    embed.WithTitle($"Help: Awarding Points | `{cmdPrefix}awardeveryone` / `{cmdPrefix}awardall`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAllows a bot owner to award a specified number of points to **all** users in their Kaguya database." +
                        $"\nThis can be a negative number, however it can not send a user's points below zero.");
                    await BE(); break;
                case "masspointsdistribute":
                    embed.WithTitle($"Help: Mass Distributing Points | `{cmdPrefix}masspointsdistribute`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user to mass redistribute all of their Kaguya Points evenly to the rest of the server. " +
                        $"Upon using this command, your points will be set to zero and they will have been evenly divided amongst everyone in the server. If you do not " +
                        $"have at least one point for every member in the server, the command will not be executed.");
                    embed.WithFooter("What a generous act!");
                    await BE(); break;
                case "n":
                    embed.WithTitle($"Help: NSFW | `{cmdPrefix}n`");
                    embed.WithDescription($"{Context.User.Mention} The `{cmdPrefix}n` command will post a 2D image (no real people) in an NSFW channel with the specified tag." +
                        $"\nWhen using the `{cmdPrefix}n` command, you may also append an optional modifier to the end like so: `{cmdPrefix}n <modifier>`." +
                        $"\nNSFW Modifiers: `{cmdPrefix}n <bomb (Sends 5 images at once), gif>` (Select one).");
                    embed.WithFooter($"{cmdPrefix}n bomb usage is limited to 5 uses per hour for non-supporters.");
                    await BE(); break;
                case "m":
                    embed.WithTitle($"Help: Music Commands | `{cmdPrefix}m <modifier>`");
                    embed.WithDescription($"{Context.User.Mention} The `{cmdPrefix}m`command group is for all Kaguya Music commands. They are described in detail below:" +
                        $"\n" +
                        $"\n**Play/Pause:** Plays or pauses the music player. Use this command first if Kaguya is not in the voice channel yet. `{cmdPrefix}m play <song name>`, `{cmdPrefix}m pause`" +
                        $"\n**Join:** Makes Kaguya join the voice channel the user is currently in, only if there is no current player already active. `{cmdPrefix}m join`" +
                        $"\n**Leave:** Makes Kaguya leave the voice channel she is currently in and closes any active players. `{cmdPrefix}m leave`" +
                        $"\n**Queue:** Displays Kaguya's playlist. Add more songs to the queue with the play command. `{cmdPrefix}m queue`" +
                        $"\n**Resume:** If Kaguya's music player is paused, she will resume playing music. `{cmdPrefix}m resume`" +
                        $"\n**Skip:** Skips the current song. `{cmdPrefix}m skip`" +
                        $"\n**Volume:** Sets the volume to a value between 0-200. This " +
                        $"also allows for a volume adjustment: `{cmdPrefix}m volume <0-200>` `{cmdPrefix}m volume <+/- {{number}}>`" +
                        $"\n**Jump:** Jump to a specific position in the queue, skipping all songs before it in one go.`{cmdPrefix}m jump <jumpNum>`");
                    await BE(); break;
                case "invite":
                    embed.WithTitle($"Help: Kaguya Invites | `{cmdPrefix}invite`");
                    embed.WithDescription($"{Context.User.Mention} Use of this command will result in you being sent a DM with a link to join my support Discord server as well as a link to add me to your server!");
                    await BE(); break;
                case "supporter":
                    embed.WithTitle($"Help: Kaguya Supporter Tags | `{cmdPrefix}supporter`");
                    embed.WithDescription($"{Context.User.Mention} Displays information on Kaguya's Supporter Tag feature.");
                    await BE(); break;
                case "redeem":
                    embed.WithTitle($"Help: Supporter Key Redemption | `{cmdPrefix}redeem`");
                    embed.WithDescription($"{Context.User.Mention} Allows a user to redeem a Kaguya Supporter Key. Keys are one time use and are purchased " +
                        $"through the **[Kaguya Supporter Store](https://stageosu.selly.store/)**. Keys are safe to redeem in public Discord channels." +
                        $"\n" +
                        $"\nAfter purchasing a supporter key, check your E-Mail because that's where your key will be!");
                    await BE(); break;
                case "diamonds":
                    embed.WithTitle($"Help: Kaguya Diamonds | `{cmdPrefix}diamonds`");
                    embed.WithDescription($"{Context.User.Mention} Displays how many `Kaguya Diamonds` you have. Diamonds are able to be earned through being a supporter. " +
                        $"For more information, check out `{cmdPrefix}supporter`!");
                    await BE(); break;
                case "diamondconvert":
                case "dc":
                    embed.WithTitle($"Help: Diamond Convert | `{cmdPrefix}diamondconvert`, `{cmdPrefix}dc`");
                    embed.WithDescription($"{Context.User.Mention} Converts your diamonds into points. This may be any amount of diamonds, so long as you have them." +
                        $"\n10 <a:KaguyaDiamonds:581562698228301876> = 1,000 points." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}dc <diamonds to convert>`");
                    await BE(); break;
                case "sync":
                    embed.WithTitle($"Help: Supporter Sync | `{cmdPrefix}sync`");
                    embed.WithDescription($"{Context.User.Mention} This is a supporter only command that will automatically give you the \"Supporter\" role in the Kaguya Support Server. " +
                        $"Note: You must use this command in the support server!");
                    embed.WithFooter($"Use the {cmdPrefix}invite command for a link to the support server if you need one!");
                    await BE(); break;
                case "cooldowns":
                    embed.WithTitle($"Help: Kaguya Cooldowns | `{cmdPrefix}cooldowns`");
                    embed.WithDescription($"{Context.User.Mention} Displays all concurrent cooldowns for Kaguya and whether or not they are off cooldown.");
                    await BE(); break;
                case "changelog":
                    embed.WithTitle($"Help: Kaguya Changelog | `{cmdPrefix}changelog`");
                    embed.WithDescription($"{Context.User.Mention} This command displays the most recent version's patch notes.");
                    await BE(); break;
                case "owner":
                    embed.WithTitle($"Help: Owner Command List | `{cmdPrefix}owner`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nOwner only command that displays all other owner only commands.");
                    await BE(); break;
                case "kill":
                    embed.WithTitle($"Help: Kill | `{cmdPrefix}kill`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nShuts down the bot and terminates the Kaguya process, taking it down for all users." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}kill`");
                    await BE(); break;
                case "restart":
                    embed.WithTitle($"Help: Restart | `{cmdPrefix}restart`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nRemotely restarts the bot. The Kaguya process opens another instance of itself right before " +
                        $"terminating the current instance." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}restart`");
                    await BE(); break;
                case "setgame":
                    embed.WithTitle($"Help: Setgame | `{cmdPrefix}setgame`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nChanges Kaguya's \"Currently Playing\" game to whatever is specified. This will last until " +
                        $"the next \"game\" in the rotation takes presedence (~15 minutes).");
                    await BE(); break;
                default:
                    embed.WithDescription($"**{Context.User.Mention} \"{cmdPrefix}{command}\" is not a valid command.**");
                    await BE();
                    break;
            }
        }

        [Command("help")] //help
        [Alias("h")]
        public async Task HelpCommand()
        {

            var cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            PaginatedMessage message = new PaginatedMessage();

            string administration = "```css" +
                        "\nAll commands in category: Administration" +
                        "\n" +
                        $"\n{cmdPrefix}addrole [ar]" +
                        $"\n{cmdPrefix}antiraid" +
                        $"\n{cmdPrefix}antiraidoff" +
                        $"\n{cmdPrefix}ban [b]" +
                        $"\n{cmdPrefix}channelblacklist [cbl]" +
                        $"\n{cmdPrefix}channelunblacklist [cubl]" +
                        $"\n{cmdPrefix}channelwhitelist [cwl]" +
                        $"\n{cmdPrefix}channelunwhitelist [cuwl]" +
                        $"\n{cmdPrefix}clear [c] [purge]" +
                        $"\n{cmdPrefix}createrole [cr]" +
                        $"\n{cmdPrefix}deleterole [dr]" +
                        $"\n{cmdPrefix}filteradd [fa]" +
                        $"\n{cmdPrefix}filterclear [clearfilter]" +
                        $"\n{cmdPrefix}filterremove [fr]" +
                        $"\n{cmdPrefix}filterview [fv]" +
                        $"\n{cmdPrefix}inspect" +
                        $"\n{cmdPrefix}kaguyaexit" +
                        $"\n{cmdPrefix}kick [k]" +
                        $"\n{cmdPrefix}logtypes [loglist]" +
                        $"\n{cmdPrefix}massban" +
                        $"\n{cmdPrefix}masskick" +
                        $"\n{cmdPrefix}mute" +
                        $"\n{cmdPrefix}removeallroles [rar]" +
                        $"\n{cmdPrefix}removerole [rr]" +
                        $"\n{cmdPrefix}resetlogchannel [rlog]" +
                        $"\n{cmdPrefix}setlogchannel [log]" +
                        $"\n{cmdPrefix}shadowban" +
                        $"\n{cmdPrefix}unblacklist" +
                        $"\n{cmdPrefix}unmute" +
                        $"\n{cmdPrefix}unshadowban" +
                        $"\n{cmdPrefix}warn [w]" +
                        $"\n{cmdPrefix}warnset [ws]" +
                        $"\n{cmdPrefix}warnoptions [wo]" +
                        $"\n{cmdPrefix}warnpunishments [wp]" +
                        $"\n{cmdPrefix}whitelist [wl]" +
                        $"\n" +
                        $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                        "\n```";
            string exp = "```css" +
                    "\nAll commands in category: Experience Points" +
                    "\n" +
                    $"\n{cmdPrefix}exp" +
                    $"\n{cmdPrefix}globalexplb [gexplb]" +
                    $"\n{cmdPrefix}rep" +
                    $"\n{cmdPrefix}repauthor [rep author]" +
                    $"\n{cmdPrefix}serverexplb [explb]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    "\n```";
            string currency = "```css" +
                "\nAll commands in category: Currency" +
                "\n" +
                $"\n{cmdPrefix}awardeveryone [awardall]" +
                $"\n{cmdPrefix}diamonds" +
                $"\n{cmdPrefix}diamondconvert [dc]" +
                $"\n{cmdPrefix}history [gh]" +
                $"\n{cmdPrefix}masspointsdistribute" +
                $"\n{cmdPrefix}points" +
                $"\n{cmdPrefix}quickdraw [qd]" +
                $"\n{cmdPrefix}roll [gr]" +
                $"\n{cmdPrefix}timely [t]" +
                $"\n{cmdPrefix}weekly" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```";

            string utility = "```css" +
                "\nAll commands in category: Utility" +
                "\n" +
                $"\n{cmdPrefix}author" +
                $"\n{cmdPrefix}autoassign [aa]" +
                $"\n{cmdPrefix}autoassignremove [aar]" +
                $"\n{cmdPrefix}autoassignclear [aac]" +
                $"\n{cmdPrefix}autoassignview [aav]" +
                $"\n{cmdPrefix}changelog" +
                $"\n{cmdPrefix}createtextchannel [ctc]" +
                $"\n{cmdPrefix}createvoicechannel [cvc]" +
                $"\n{cmdPrefix}deletetextchannel [dtc]" +
                $"\n{cmdPrefix}deletevoicechannel [dvc]" +
                $"\n{cmdPrefix}inrole" +
                $"\n{cmdPrefix}prefix" +
                $"\n{cmdPrefix}stats" +
                $"\n{cmdPrefix}toggleannouncements" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```";

            string fun = "```css" +
                    "\nAll commands in category: Fun" +
                    "\n" +
                    $"\n{cmdPrefix}8ball" +
                    $"\n{cmdPrefix}baka " +
                    $"\n{cmdPrefix}echo" +
                    $"\n{cmdPrefix}fact" +
                    $"\n{cmdPrefix}hug " +
                    $"\n{cmdPrefix}kiss " +
                    $"\n{cmdPrefix}nekoavatar " +
                    $"\n{cmdPrefix}pat " +
                    $"\n{cmdPrefix}pick" +
                    $"\n{cmdPrefix}poke " +
                    $"\n{cmdPrefix}slap " +
                    $"\n{cmdPrefix}smug " +
                    $"\n{cmdPrefix}tickle " +
                    $"\n{cmdPrefix}waifu " +
                    $"\n{cmdPrefix}wallpaper " +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n" +
                    $"\n```";

            string music = "```css" +
                    "\nAll commands in category: Music!" +
                    "\n" +
                    $"\n{cmdPrefix}m play" +
                    $"\n{cmdPrefix}m pause" +
                    $"\n{cmdPrefix}m join" +
                    $"\n{cmdPrefix}m resume" +
                    $"\n{cmdPrefix}m leave" +
                    $"\n{cmdPrefix}m queue" +
                    $"\n{cmdPrefix}m skip" +
                    $"\n{cmdPrefix}m volume" +
                    $"\n{cmdPrefix}m jump" +
                    $"\n" +
                    $"\nType \"{cmdPrefix}h m\" for more information on Kaguya's Music Module." +
                    $"\n```";

            string osu = "```css" +
                    "\nAll commands in category: osu!" +
                    "\n" +
                    $"\n{cmdPrefix}osu" +
                    $"\n{cmdPrefix}osuset" +
                    $"\n{cmdPrefix}osutop" +
                    $"\n{cmdPrefix}osutop -n" +
                    $"\n{cmdPrefix}recent [r]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n```";

            string help = "```css" +
                    "\nAll commands in category: Help" +
                    "\n" +
                    $"\n{cmdPrefix}bugreport" +
                    $"\n{cmdPrefix}cooldowns" +
                    $"\n{cmdPrefix}help [h]" +
                    $"\n{cmdPrefix}helpdm [hdm]" +
                    $"\n{cmdPrefix}invite" +
                    $"\n{cmdPrefix}profile [p]" +
                    $"\n{cmdPrefix}redeem" +
                    $"\n{cmdPrefix}supporter" +
                    $"\n{cmdPrefix}vote" +
                    $"\n{cmdPrefix}voteclaim [vc]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n```";

            string nsfw = "```css" +
                    "\nAll commands in category: NSFW" +
                    "\nNote: ALL NSFW images are 2D!" +
                    "\n" +
                    $"\n{cmdPrefix}n" +
                    $"\n{cmdPrefix}n bomb" +
                    $"\n{cmdPrefix}n gif" +
                    $"\n" +
                    $"\nType \"{cmdPrefix}h n\" for more information. Must be used in an NSFW channel." +
                    $"\n```";

            var pages = new[]
            {
                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 1: Administration",
                    Description = administration,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 2: Currency",
                    Description = currency,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 3: EXP",
                    Description = exp,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 4: Fun",
                    Description = fun,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 5: Help",
                    Description = help,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 6: Music",
                    Description = music,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 7: NSFW",
                    Description = nsfw,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 8: osu!",
                    Description = osu,
                },

                new PaginatedMessage.Page
                {
                    Title = "Commands List Page 9: Utility",
                    Description = utility,
                },
            };

            var trashcanEmote = new Emoji("🚮");

            var options = new PaginatedAppearanceOptions
            {
                JumpDisplayOptions = JumpDisplayOptions.Always,
                Stop = trashcanEmote
            };


            var pager = new PaginatedMessage
            {
                Pages = pages,
                Color = new Color(252, 132, 255),
                Options = options
            };

            await PagedReplyAsync(pager, new ReactionList
            {
                First = true,
                Last = true,
                Backward = true,
                Forward = true,
                Trash = true
            });
        }

        [Command("owner")]
        [RequireOwner]
        public async Task OwnerCommands()
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            string commands = "```css" +
                "\nAll commands in category: Owner Only" +
                "\n" +
                $"\n{cmdPrefix}blacklist [bl] " +
                $"\n{cmdPrefix}bugaward " +
                $"\n{cmdPrefix}expadd [addexp] " +
                $"\n{cmdPrefix}kaguyawarn " +
                $"\n{cmdPrefix}kill " +
                $"\n{cmdPrefix}massblacklist " +
                $"\n{cmdPrefix}pointsadd [addpoints] " +
                $"\n{cmdPrefix}restart " +
                $"\n{cmdPrefix}timelyreset " +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                "\n```";

            embed.WithTitle("Owner Commands");
            embed.WithDescription(commands);
            await BE();
        }

        [Command("cooldowns")]
        public async Task ShowCooldowns()
        {
            var userAccount = UserAccounts.GetAccount(Context.User);

            //Future cooldown reset datetimes are always before DateTime.Now

            var nbombCD = userAccount.NBombCooldownReset - DateTime.Now;
            var timelyCD = DateTime.Now - userAccount.LastReceivedTimelyPoints;
            var weeklyCD = DateTime.Now - userAccount.LastReceivedWeeklyPoints;
            var repCD = DateTime.Now - userAccount.LastGivenRep;
            var upvoteCD = DateTime.Now - userAccount.LastUpvotedKaguya;

            //A = Available

            bool nbombA = nbombCD.TotalSeconds < 0;
            bool timelyA = timelyCD.TotalSeconds > 86400; //One day
            bool weeklyA = weeklyCD.TotalSeconds > 604800; //One week
            bool repA = repCD.TotalSeconds > 86400;
            bool upvoteA = upvoteCD.TotalSeconds > 86400;

            string nbombAvailable = $"NSFW: Available in `{0 - nbombCD.Hours}h {nbombCD.Minutes}m {nbombCD.Seconds}s`";
            string timelyAvailable = $"Available in `{23 - timelyCD.Hours}h {59 - timelyCD.Minutes}m {59 - timelyCD.Seconds}s`";
            string weeklyAvailable = $"Available in `{6 - weeklyCD.Days}d {23 - weeklyCD.Hours}h {59 - weeklyCD.Minutes}m {59 - weeklyCD.Seconds}s`";
            string repAvailable = $"Available in `{23 - repCD.Hours}h {59 - repCD.Minutes}m {59 - repCD.Seconds}s`";
            string upvoteAvailable = $"Available in `{11 - upvoteCD.Hours}h {59 - upvoteCD.Minutes}m {59 - upvoteCD.Seconds}s`";

            if (nbombA)
                nbombAvailable = "NSFW: `Available!`";
            if (userAccount.NSFWAgeVerified == "false")
                nbombAvailable = "";
            if (timelyA)
                timelyAvailable = "`Available!`";
            if (weeklyA)
                weeklyAvailable = "`Available!`";
            if (repA)
                repAvailable = "`Available!`";
            if (upvoteA)
                upvoteAvailable = "`Available!`";

            embed.WithTitle($"Cooldowns for {Context.User.Username}");
            embed.WithDescription($"{nbombAvailable}" +
                $"\nTimely Points: {timelyAvailable}" +
                $"\nWeekly Points: {weeklyAvailable}" +
                $"\nVote Rewards: {upvoteAvailable}" +
                $"\nRep Point: {repAvailable}");

            await BE();
        }

        [Command("sync")]
        public async Task Sync()
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            var difference = userAccount.KaguyaSupporterExpiration - DateTime.Now;

            if(difference.TotalSeconds > 0 && Context.Guild.Id == 546880579057221644) //This means that someone is an active supporter in the Kaguya Support server.
            {
                var supporterRole = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "supporter");

                await (Context.User as IGuildUser).AddRoleAsync(supporterRole);

                embed.WithTitle($"Kaguya Supporter Sync");
                embed.WithDescription($"The `Supporter` role has successfully been applied!");
                embed.WithFooter($"Thanks so much for your support!");
                await BE();
            }
            else if(!(difference.TotalSeconds > 0) && Context.Guild.Id == 546880579057221644)
            {
                embed.WithTitle($"Kaguya Supporter Sync");
                embed.WithDescription($"{Context.User.Mention} You must be an active supporter to use this command!");
                embed.SetColor(EmbedType.RED);
                await BE();
            }
            else
            {
                return;
            }
        }

        [Command("invite")]
        public async Task Invite()
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

            embed.WithDescription($"Here's a link to my support server: https://discord.gg/aumCJhr" +
                $"\nHere's a link that you can use to add me to your server: https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8");
            embed.SetColor(EmbedType.PINK);
            await dmChannel.SendMessageAsync(embed: embed.Build());

            embed.WithDescription($"{Context.User.Mention} DM: Sent! <:Kaguya:581581938884608001>");
            await BE();
        }

        [Command("helpdm")] //help
        [Alias("hdm")]
        public async Task HelpDM()
        {
            var cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");

            await BE();
            await Context.User.SendMessageAsync($"Need the commands list? Type `{cmdPrefix}h` to see a scrollable list of categories with all of their commands." +
                $"\nType `{cmdPrefix}h <command name>` for more information on how to use the command and a detailed description of what it does." +
                $"\nAdd me to your server with this link!: <https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8>" +
                $"\nWant to keep track of all the changes? Feel free to check out the Kaguya GitHub page!: <https://github.com/stageosu/Kaguya>" +
                $"\nKaguya Support Server: https://discord.gg/aumCJhr (This is also a good place to see what's coming soon and get notified when new updates come out :D)");
        }

        [Command("bugreport")]
        public async Task BugReport([Remainder]string report)
        {
            var bugChannel = Global.client.GetChannel(547448889620299826); //Kaguya support server #bugs channel.

            embed.WithTitle($"Bug Report");
            embed.WithDescription($"Report from user `{Context.User.Username}#{Context.User.Discriminator}` with ID: `{Context.User.Id}`" +
                $"\n" +
                $"\nMessage: `\"{report}\"`");
            embed.WithTimestamp(DateTime.Now);

            await (bugChannel as ISocketMessageChannel).SendMessageAsync($"", false, embed.Build()); //Sends first embed to bug report channel.

            embed.WithTitle($"Bug Report");
            embed.WithDescription($"**{Context.User.Mention} `Your bug report has been sent.`**");
            embed.WithFooter("Thank you for using this feature. Abuse will result in a permanent blacklist from Kaguya.");
            await BE();
        }

        [Command("vote")]
        public async Task Vote()
        {

            embed.WithTitle("Discord Bot List Voting");
            embed.WithDescription($"Show Kaguya some love and give her an upvote! https://discordbots.org/bot/538910393918160916/vote" +
                $"\nUsers that upvote receive a `2x` critical hit percentage for the next `12 hours` and `500` Kaguya points! Users may vote every 12 hours!");
            embed.WithFooter($"Thanks for showing your support! Use {Servers.GetServer(Context.Guild).CommandPrefix}voteclaim to claim your reward!");

            await BE();
        }

        [Command("voteclaim")]
        [Alias("vc")]
        public async Task VoteClaim()
        {
            if (Config.bot.RecentVoteClaimAttempts <= 50)
            {
                Config.bot.RecentVoteClaimAttempts++;
                HttpClient client = new HttpClient();
                UserAccount userAccount = UserAccounts.GetAccount(Context.User);
                var difference = DateTime.Now - userAccount.LastUpvotedKaguya;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Config.bot.DblApiKey}");
                var dblResponse = await client.GetStringAsync($"https://discordbots.org/api/bots/538910393918160916/check?userId={Context.User.Id}");

                if (dblResponse.Contains("{\"voted\":1}"))
                {
                    if (difference.TotalSeconds < 43200)
                    {
                        embed.WithDescription($"**{Context.User.Mention} You've already upvoted me and claimed your reward!" +
                            $"\nTime remaining: `{11 - (int)difference.TotalHours} hours {59 - difference.Minutes} minutes and {60 - difference.Seconds} seconds`**");
                        await BE();
                    }
                    else if (difference.TotalSeconds > 43200)
                    {
                        userAccount.LastUpvotedKaguya = DateTime.Now;
                        userAccount.Points += 500;
                        userAccount.NBombUsesThisHour += 5;

                        embed.WithDescription($"{Context.User.Mention} Thanks for upvoting! Your rewards of `500 Kaguya Points`, `2x critical hit rate` " +
                            $"and `cooldown resets` have been applied.");
                        embed.WithFooter("Thanks so much for your support!!");
                        await BE();
                    }
                }
                else if (dblResponse.Contains("{\"voted\":0}"))
                {
                    embed.WithDescription($"**{Context.User.Mention} you have not upvoted me! Please do so with the vote command!**");
                    embed.WithFooter($"If you have upvoted, please wait two minutes and try again.");
                    await BE();
                }
            }
            else
            {
                embed.WithDescription($"{Context.User.Mention} I am being rate limited! Please try again in 60 seconds.");
                embed.SetColor(EmbedType.RED);
                await BE();
            }
        }

        [Command("supporter")]
        public async Task SupporterInfo()
        {
            Stopwatch stopWatch = new Stopwatch();
            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            await GlobalCommandResponses.CreateCommandResponse(Context,
                "Kaguya Supporter",
                "For those who wish to support the growth and development of the Kaguya Project, and get some cool perks in return, " +
                "the Kaguya Supporter system is for you! While completely optional, those who wish to financially support may purchase a " +
                "**Kaguya Supporter Key** from the official Kaguya store. The funds received from the supporter tags go directly into Kaguya's " +
                "server and advertising budget. This way, we can keep her alive while also spreading her awesomeness to as many servers as possible!" +
                "\n" +
                "\n**If you wish to, you may purchase a supporter tag through the Kaguya Store: <https://stageosu.selly.store/>**" +
                "\n" +
                "\nSupporter Keys are available in `30`, `60`, and `90` day intervals. You may purchase as many keys as you wish, as the time stacks." +
                "\n" +
                $"\nKeys are redeemed through the `{cmdPrefix}redeem` command, and are one time use. They are safe to redeem in a public Discord channel!",
                "Supporter perks and benefits are described in the Kaguya Store. Thank you for considering!! 💙");
        }

    }
}
