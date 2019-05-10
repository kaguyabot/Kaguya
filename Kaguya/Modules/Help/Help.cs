using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using Kaguya.Core;
using System.Diagnostics;
using Discord.WebSocket;
using System;
using Kaguya.Core.UserAccounts;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Kaguya.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.Token;
        readonly DiscordSocketClient _client = Global.Client;
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("h")] //The BIG fish, Help
        [Alias("help")]
        public async Task HelpCommand([Remainder]string command)
        {
            stopWatch.Start();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            switch (command.ToLower())
            {
                case "h":
                case "help":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Help!! | `{cmdPrefix}h` / `{cmdPrefix}help`");
                    embed.WithDescription($"Shows the command list. If typed with the name of a command (Ex: `{cmdPrefix}help <command>`), the response will instead contain helpful information on the specified " +
                        $"command, including how to use it.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "helpdm":
                case "hdm":
                    stopWatch.Start();
                    embed.WithTitle($"Help: HelpDM | `{cmdPrefix}helpdm`");
                    embed.WithDescription($"{Context.User.Mention} Sends a DM with helpful information, including a link to add the bot to your own server, and a link to the Kaguya Github page!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "bug":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Bug Report | `{cmdPrefix}bug`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to report a bug directly to the support server's `#bugs` channel so that my creator can take a look at what's wrong (and hopefully fix it)! " +
                        $"Please use this feature whenever you feel something is wrong with Kaguya, but don't overdo it! Messages that are spammy, violate the Discord TOS, or are abusive/deragotory in nature will " +
                        $"result in a `permanent blacklist` from all of Kaguya. A bug report that leads to something getting fixed will result in `+2000 Kaguya points` added to your account on the next patch as a thank you!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}bug <message>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "vote":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Voting | `{cmdPrefix}vote`");
                    embed.WithDescription($"{Context.User.Mention} I will reply with a link to my discordbots.org page. If you wish to support me and want more people to have the ability to use me, " +
                        $"give me an upvote! My creator and I greatly appreciate it uwu. After voting, use `{cmdPrefix}voteclaim` to get some rewards for your support!");
                    embed.WithFooter($"Use {cmdPrefix}h voteclaim to find out what the rewards for upvoting are!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "voteclaim":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Claiming Voting Rewards | `{cmdPrefix}voteclaim`");
                    embed.WithDescription($"{Context.User.Mention} Use this command after voting (see `{cmdPrefix}h vote`) to have some rewards applied to your Kaguya account!" +
                        $"\nRewards: `2x critical hit chance for 12 hours` and `500 Kaguya Points`! You may ask \"well, what's a critical?\" I have a help command for that! Use `{cmdPrefix}h critical` to find out more!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "critical":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Critical Hits");
                    embed.WithDescription($"{Context.User.Mention} No matter what currency related command you use (roll, timely, weekly, etc.), there is a chance that reward can be a \"critical\"." +
                        $"\nThe critical factor will greatly multiply your point rewards." +
                        $"\n" +
                        $"\nCritical Rewards:" +
                        $"\n" +
                        $"\nRolls: `8% chance that the multiplier of your bet is multiplied by 2.5x`" +
                        $"\nTimely: `14% chance that the value of your reward is multiplied by 3.5x`" +
                        $"\nWeekly: `8% chance that the value of your reward will be multiplied by 3.5x`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "exp":
                    stopWatch.Start();
                    embed.WithTitle($"Help: EXP | `{cmdPrefix}exp`");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}exp`." +
                        $"\nReturns the value of experience points the user has in their account." +
                        $"\nSyntax: `{cmdPrefix}exp`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "expadd":
                case "addexp":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Adding Experience Points | `{cmdPrefix}expadd` / `{cmdPrefix}addexp");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n{Context.User.Mention} Adds EXP points to the specified user. The number of exp you are adding must be a positive whole number." +
                        $"\nSyntax: `{cmdPrefix}expadd <number of experience points to add> <User {{ID, Name, Mention}}>`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "points":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Points | `{cmdPrefix}points`");
                    embed.WithDescription($"\n{Context.User.Mention} Syntax: `{cmdPrefix}points`." +
                        $"\nReturns the value of points a user has in their account.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "pointsadd":
                case "addpoints":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Adding Points | `{cmdPrefix}pointsadd`, `{cmdPrefix}addpoints`");
                    embed.WithDescription($"**Permissions Required: Administrator, Bot Owner**" +
                        $"\n{Context.User.Mention} Adds points to the specified user's kaguya account. The number of points you are adding must be a positive whole number." +
                        $"\nSyntax: `{cmdPrefix}pointsadd <number of points to add> <User {{ID, Name, Mention}}>`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "level":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Level | `{cmdPrefix}level`");
                    embed.WithDescription($"{Context.User.Mention} Displays your current Kaguya level!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "createtextchannel":
                case "ctc":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Creating Text Channels | `{cmdPrefix}createtextchannel`, `{cmdPrefix}ctc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a text channel with the speficied name. " +
                        $"\nSyntax: `{cmdPrefix}createtextchannel <channel name>`. " +
                        $"\nThis name can have spaces. Example: `{cmdPrefix}createtextchannel testing 123`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "commands":
                case "cmds":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Commands | `{cmdPrefix}commands` / `{cmdPrefix}cmds`");
                    embed.WithDescription($"{Context.User.Mention} Displays a list of commands for the specified module. Use {cmdPrefix}modules " +
                        $"for the list of modules you may pick from. Modules are essentially command groups seperated by category." +
                        $"\nSyntax: `{cmdPrefix}cmds <module>`, `{cmdPrefix}cmds administrator`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "modules":
                case "mdls":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Modules | `{cmdPrefix}modules` / `{cmdPrefix}mdls`");
                    embed.WithDescription($"{Context.User.Mention} Displays a list of modules that contain commands. Use `{cmdPrefix}cmds <module>` " +
                        $"to see the commands list for that module." +
                        $"\nSyntax: `{cmdPrefix}modules`, `{cmdPrefix}mdls`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "deletetextchannel":
                case "dtc":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Deleting Text Channels | `{cmdPrefix}deletetextchannel`, `{cmdPrefix}dtc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a text channel with the speficied name. " +
                        $"\nThis name can **not** have spaces. Type the text channel exactly as displayed; If the text channel contains a `-`, type that in." +
                        $"\nSyntax: `{cmdPrefix}deletetextchannel <channel name>`." +
                        $"Example: `{cmdPrefix}deletetextchannel super-long-name-with-lots-of-spaces`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "createvoicechannel":
                case "cvc":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Creating Voice Channels | `{cmdPrefix}createvoicechannel`, `{cmdPrefix}cvc`");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Creates a voice channel with the speficied name." +
                        $"\nThis name can have spaces." +
                        $"\nSyntax: `{cmdPrefix}createvoicechannel <channel name>`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "deletevoicechannel":
                case "dvc":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Deleting Voice Channels | `{cmdPrefix}deletevoicechannel`, `{cmdPrefix}dvc");
                    embed.WithDescription("**Permissions Required: Manage Channels**" +
                        "\n" +
                        $"\n{Context.User.Mention} Deletes a voice channel with the speficied name. " +
                        $"\nThis name can have spaces. Replace the `-` symbols with spaces." +
                        $"\nSyntax: `{cmdPrefix}deletevoicechannel <channel name>`." +
                        $"Example: `{cmdPrefix}deletevoicechannel super long name with lots of spaces`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "createrole":
                case "cr":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Creating Roles | `{cmdPrefix}createrole`, `{cmdPrefix}cr`");
                    embed.WithDescription($"{Context.User.Mention} Creates a role with the specified name. The role will have no special permissions or colors." +
                        $"\nSyntax: `{cmdPrefix}createrole <name>`, `{cmdPrefix}cr <name>`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "echo":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Echoed Messages | `{cmdPrefix}echo`");
                    embed.WithDescription($"{Context.User.Mention} Makes the bot repeat anything you say!" +
                        $"\nSyntax: `{cmdPrefix}echo <message>`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "pick":

                    stopWatch.Start();
                    embed.WithTitle($"Help: Pick | `{cmdPrefix}pick`");
                    embed.WithDescription($"{Context.User.Mention} Tells the bot to pick between any amount of options, randomly." +
                        $"\nSyntax: `{cmdPrefix}pick option1|option2|option3|option4`...etc." +
                        $"\nYou may have as many \"Options\" as you'd like!" +
                        $"\nThe bot will always pick with totally random odds.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "8ball":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Magic 8Ball | `{cmdPrefix}8ball`");
                    embed.WithDescription($"{Context.User.Mention} Ask Kaguya a question and she will use her divine powers to answer you extremely accurately!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}8ball <question>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "slap":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Slapping! | `{cmdPrefix}slap`");
                    embed.WithDescription($"{Context.User.Mention} Slap someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}slap <word>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "hug":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Hugging! | `{cmdPrefix}hug`");
                    embed.WithDescription($"{Context.User.Mention} hug someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}hug <word>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "kiss":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Kissing! | `{cmdPrefix}kiss`");
                    embed.WithDescription($"{Context.User.Mention} Hug someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}kiss <word>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "pat":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Patting! | `{cmdPrefix}pat`");
                    embed.WithDescription($"{Context.User.Mention} pat someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}pat <word>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "poke":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Poking! | `{cmdPrefix}poke`");
                    embed.WithDescription($"{Context.User.Mention} Poke someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}poke <word>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "tickle":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Tickling! | `{cmdPrefix}tickle`");
                    embed.WithDescription($"{Context.User.Mention} Tickle someone! An emotionally-accurate gif will be displayed in chat to show your target how you really feel." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}tickle <word>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "baka":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Baka | `{cmdPrefix}baka`");
                    embed.WithDescription($"{Context.User.Mention} Someone said something stupid? Show them how much of a baka they are with the baka command! An emotionally-accurate gif will be posted in chat." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}baka`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "nekoavatar":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Neko Avatar | `{cmdPrefix}nekoavatar`");
                    embed.WithDescription($"{Context.User.Mention} Generates a Neko Avatar for you!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}nekoavatar`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "smug":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Smug | `{cmdPrefix}smug`");
                    embed.WithDescription($"{Context.User.Mention} Posts a \"smug\" gif in chat." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}smug`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "waifu":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Waifu! | `{cmdPrefix}waifu`");
                    embed.WithDescription($"{Context.User.Mention} Posts an image of a waifu in chat." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}waifu`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "wallpaper":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Wallpaper! | `{cmdPrefix}wallpaper`");
                    embed.WithDescription($"{Context.User.Mention} An anime wallpaper will be posted in chat!" +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}wallpaper`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "timely":
                case "t":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Timely Points | `{cmdPrefix}timely`");
                    embed.WithDescription($"{Context.User.Mention} The timely command allows any user to claim 500 free points every 24 hours." +
                        "\nThese points are added to your Kaguya account. The timely command has a 14% chance of landing a critical hit, " +
                        "multiplying your reward by `3.50x`." +
                        $"\nSyntax: `{cmdPrefix}timely`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "weekly":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Weekly Points | `{cmdPrefix}weekly`");
                    embed.WithDescription($"{Context.User.Mention} The weekly command allows any user to claim 5,000 points every week." +
                        "\nThese points are automatically added to your Kaguya account. The weekly command has an 8% chance to land a critical hit, multiplying " +
                        "your reward by `3.50x`." +
                        $"\nSyntax: `{cmdPrefix}weekly`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "clear":
                case "purge":
                case "c":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Clearing Messages | `{cmdPrefix}clear`, `{cmdPrefix}purge`, `{cmdPrefix}c`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Messages**" +
                        $"\n" +
                        $"\nDeletes a specified number of messages in a given channel. This number may not exceed `100`. Messages older than two weeks will need to be deleted manually." +
                        $"\nSyntax: `{cmdPrefix}clear <num>`, `{cmdPrefix}purge <num>`, {cmdPrefix}c <num>");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "kick":
                case "k":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Kicking Users | `{cmdPrefix}kick`, `{cmdPrefix}k`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Kick Members**" +
                        $"\n" +
                        $"\nKicks an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}kick @User#0000`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "mute":
                    stopWatch.Start();
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
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "shadowban":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Shadowbanning Users | `{cmdPrefix}shadowban`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nShadowbans an individual member from the server, blocking all access to all channels. All permissions " +
                        $"for this user, in every channel, will be denied. Their roles will remain, however." +
                        $"\nSyntax: `{cmdPrefix}shadowban @User#0000`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "unshadowban":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Un-Shadowbanning Users | `{cmdPrefix}unshadowban`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nUn-Shadowbans an individual member from the server, reinstating all access to all channels. All permissions " +
                        $"for this user, in every channel, will be set to default (the user is neither allowed or denied any explicit permissions)." +
                        $"\nSyntax: `{cmdPrefix}unshadowban @User#0000`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "ban":
                case "b":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Banning Users | `{cmdPrefix}ban`, `{cmdPrefix}b`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Ban Members**" +
                        $"\n" +
                        $"\nBans an individual member from the server." +
                        $"\nSyntax: `{cmdPrefix}ban @User#0000`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "massban":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Mass Banning of Users | `{cmdPrefix}massban`");
                    embed.WithDescription($"**{Context.User.Mention} Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nTakes a list of mentioned users and permanently bans them simultaneously." +
                        $"\nSyntax: `{cmdPrefix}massban @mentioneduser#0001 @otheruser#0002 @smellysushi#2623 [...]`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "masskick":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Mass Kicking of Users | `{cmdPrefix}masskick`");
                    embed.WithDescription($"**{Context.User.Mention} Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nTakes a list of mentioned users and kicks them simultaneously." +
                        $"\nSyntax: `{cmdPrefix}masskick @bullyHunter#0001 @stinkysushi#0002 @smellysushi#2623 [...]`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "removeallroles":
                case "rar":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Removing All Roles | `{cmdPrefix}removeallroles`, `{cmdPrefix}rar`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        "\n" +
                        "\nRemoves all roles from the specified user." +
                        $"\nSyntax: `{cmdPrefix}removeallroles @User#0000`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "deleterole":
                case "dr":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Deleting Roles | `{cmdPrefix}deleterole`, `{cmdPrefix}dr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nDeletes a role from the server (and in the process, removes said role from everyone who had it). " +
                        $"If multiple matches of the same role are found, the bot will delete all occurrences of said role." +
                        $"\nSyntax: `{cmdPrefix}deleterole <role name>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "osu":
                    stopWatch.Start();
                    embed.WithTitle($"Help: osu! | `{cmdPrefix}osu`");
                    embed.WithDescription($"{Context.User.Mention} Presents lots of statistics from the given osu! profile name. If your `{cmdPrefix}osuset` username " +
                        $"is set, you may use `{cmdPrefix}osu` by itself. Otherwise, you must specify a name afterward." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}osu`, `{cmdPrefix}osu <username>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "createteamrole":
                case "ctr":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Create Team Roles | `{cmdPrefix}createteamrole`, `{cmdPrefix}ctr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Roles**" +
                        $"\n" +
                        $"\nCreates a role, then applies it to all mentioned users." +
                        $"\nThis is very ideal for managing many groups of people (such as teams in a tournament, hence the name)." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}createteamrole <role name> <mentioned users>`" +
                        $"\nExample: `{cmdPrefix}createteamrole \"Smelly Sushi\" @user1#0000 @smellyfish#2100 @smellysushilover#9999`.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "osutop":
                    stopWatch.Start();
                    embed.WithTitle($"Help: osu! Top | `{cmdPrefix}osutop`");
                    embed.WithDescription($"\n" +
                        $"\n{Context.User.Mention} Displays the specified amount of top osu! plays for a given player with other relevant information." +
                        $"\nThe number of requested plays to display may not be more than 10." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}osutop 5 Stage` | `{cmdPrefix}osutop 8 \"Smelly sushi\"`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "delteams":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Deleting Teams | `{cmdPrefix}delteams`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: `Manage Roles`, `Administrator`, `Bot Owner`**" +
                        $"\n" +
                        $"\nDeletes all team roles. A team role is any role that has the word \"Team: \" inside of it (with the space)." +
                        $"\nThis command will delete ALL team roles upon execution, making this command dangerous and irreversable.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "recent":
                case "r":
                    stopWatch.Start();
                    embed.WithTitle($"Help: osu! Recent | `{cmdPrefix}r` / `{cmdPrefix}recent`");
                    embed.WithDescription($"{Context.User.Mention} Displays the most recent osu! play for the given user. If there is no user specified," +
                        $" the bot will use the osu! username that was specified to the command executor's Kaguya account (through {cmdPrefix}osuset).\n" +
                        $"As of right now, no response will be given for an invalid username.\n");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "osuset":
                    string name = Context.User.Username;
                    stopWatch.Start();
                    embed.WithTitle($"Help: osuset | `{cmdPrefix}osuset`");
                    embed.WithDescription($"{Context.User.Mention} Adds an osu! username to your Kaguya account! Setting your osu! username allows you to use all osu! related commands without any additional " +
                        $"parameters. For example, instead of typing `{cmdPrefix}osutop {name}`, you can now just type `{cmdPrefix}osutop` to get your most recent osu! plays. Same thing for `{cmdPrefix}r` / `{cmdPrefix}recent`!");
                    embed.WithFooter("Ensure your username is spelled properly, otherwise all osu! related commands will not work for you!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "massblacklist":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Mass Blacklist | `{cmdPrefix}massblacklist`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner, Administrator**" +
                        $"\n" +
                        $"\nA bot owner may execute this command on a list of users they deem unworthy of being able to ever use Kaguya again. These users are permanently banned from the server this command is executed in." +
                        $"These users will have all of their EXP and Points reset to zero, and will be permanently filtered from receiving EXP and executing Kaguya commands." +
                        $"\nSyntax: `{cmdPrefix}massblacklist @username#123` | `{cmdPrefix}massblacklist @username#123 @ToxicPlayer123#7777 @SuckySmellySushi#1234`");
                    embed.WithFooter("Bot owners: This command is EXTREMELY DANGEROUS.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "unblacklist":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Unblacklisting Users | `{cmdPrefix}unblacklist <UserID>`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nUnblacklists the specified userID." +
                        $"\nSelf-Hosters: If you do not know the ID of the person to unblacklist, look through accounts.json.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "roll":
                case "gr":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Betting | `{cmdPrefix}roll` / `{cmdPrefix}gr`");
                    embed.WithDescription($"{Context.User.Mention} Allows you to roll the dice and gamble your points!" +
                        $"\n" +
                        $"\nA roll between `0-66` will result in a loss of your bet. " +
                        $"A roll between `67-78` will return your bet back to you with a multiplier of `1.25x`" +
                        $"\nRolls between `79-89`, `90-95`, `96-99`, and `100` will yield multipliers of `1.75x`, `2.25x`, `3x`, and `5x` respectively." +
                        $"\n" +
                        $"\nThe maximum amount of points you can gamble at one time is set to `25,000`." +
                        $"\n" +
                        $"\nIn addition, all rolls have a `4%` chance of landing a critical hit, multiplying the `multiplier` of the roll by `3.50x`. " +
                        $"The best possible roll is a `critical 100`, multiplying your bet by `17.5x` (The odds of this are `1 / 2,500` or `0.04%`.)" +
                        $"\nRolls also have a 4% chance of being a `critical loss`, resulting in `25%` additional lost points. (The odds of this are `66 / 2,500` or `2.64%`.)");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "kaguyaexit":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Kaguya, gtfo! | `{cmdPrefix}kaguyagtfo`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAdministrator only command that forces Kaguya to leave the current server.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "prefix":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Prefix Alteration | `{cmdPrefix}prefix`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to change the bot's command prefix. Typically, this is one or two symbols `(!, $, %, >, etc.)`." +
                        $"\nTo reset the command prefix, type {cmdPrefix}prefix, or tag me and type `prefix`! The bot will always display the last known command prefix " +
                        $"and the new prefix when using this command.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "serverexplb":
                case "explb":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Server EXP Leaderboard | `{cmdPrefix}serverexplb` / `{cmdPrefix}explb`");
                    embed.WithDescription($"{Context.User.Mention} Displays the 10 top EXP holders in the server. This command " +
                        $"also displays their level.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "globalexplb":
                case "gexplb":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Global EXP Leaderboard | `{cmdPrefix}globalexplb` / `{cmdPrefix}gexplb`");
                    embed.WithDescription($"{Context.User.Mention} Displays the 10 top EXP holders in the entire Kaguya database! This command " +
                        $"also displays their level.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "scrapeserver":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Server Scraping | `{cmdPrefix}scrapeserver`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\nOrders the bot to create user accounts for every individual in the server, even if they have never typed " +
                        $"in chat. This function is automatically called when using `{cmdPrefix}massblacklist` to ensure that " +
                        $"there is no question on whether they will be able to be banned/unbanned. Creating a user account allows for name " +
                        $"and ID logging, the latter is necessary if a bot owner wishes to unblacklist a user.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "scrapedatabase":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Database Scraping | `{cmdPrefix}scrapedatabase`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator, Bot Owner**" +
                        $"\n" +
                        $"\nCreates an account for every user in every server that Kaguya is connected to. This command will not create accounts " +
                        $"for other bots or users in servers with over `3,500` members. This command primarily exists for stability reasons (occasionally, if a " +
                        $"user doesn't have an account, a bot function may not work for said user [such as with `$ctr`]).");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "bugaward":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Bug Rewards | `{cmdPrefix}bugaward`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nDM's the target and adds 2,000 Kaguya Points to their account. This is the reward for a `$bug` report that directly led to a patch/fix.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "rep":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Rep | `{cmdPrefix}rep`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user in the server to add one reputation point to another member." +
                        $"\nThis can be done once every 24 hours, and can not be used on yourself. This rep will show on your Kaguya profile!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "rep author":
                case "repauthor":
                    stopWatch.Start();
                    embed.WithTitle($"Help: +Rep Author | `{cmdPrefix}repauthor` / `{cmdPrefix}rep author`");
                    embed.WithDescription($"{Context.User.Mention} Gives my creator your daily +rep point!");
                    embed.WithFooter($"We appreciate your generosity uwu | To give rep to another user, use $rep!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "author":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Author | `{cmdPrefix}author`");
                    embed.WithDescription($"{Context.User.Mention} Displays information about my creator!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "timelyreset":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Timely Reset | `{cmdPrefix}timelyreset`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAllows a bot owner to reset the {cmdPrefix}timely cooldown for every user in the Kaguya database.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filteradd":
                case "fa":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Filter Adding | `{cmdPrefix}filteradd` / `{cmdPrefix}fa`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to add a word or phrase to the list of filtered words for the server." +
                        $"\nSpaces may be used when adding a phrase to the filter. The filter is not case sensitive." +
                        $"\nExamples: `{cmdPrefix}fa Smelly Sushi`, `{cmdPrefix}fa frogs`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filterremove":
                case "fr":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Filter Removing | `{cmdPrefix}filterremove` / `{cmdPrefix}fr`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to remove a word or phrase from the list of filtered words for the server." +
                        $"\nSpaces may be used when removing a phrase from the filter. The filter is not case sensitive." +
                        $"\nExamples: `{cmdPrefix}fr Smelly Sashimi`, `{cmdPrefix}fr caterpillars`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filterview":
                case "fv":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Viewing Filtered Words | `{cmdPrefix}filterview` / `{cmdPrefix}fv`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Manage Messages**" +
                        $"\n" +
                        $"\nAllows viewing of all filtered words and phrases in the server. Ideally this would be used in a private \"Moderator\" channel.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filterclear":
                case "clearfilter":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Filter Clearing | `{cmdPrefix}filterclear` / `{cmdPrefix}clearfilter`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows a server administrator to remove ALL words and phrases from the server's list of filtered words/phrases." +
                        $"\nThis command does not take any parameters." +
                        $"\nExamples: `{cmdPrefix}filterclear`, `{cmdPrefix}clearfilter`");
                    embed.WithFooter("This action is dangerous and irreversible!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "setlogchannel":
                case "log":
                    stopWatch.Start();
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
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "resetlogchannel":
                case "rlog":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Resetting Logging Channels | `{cmdPrefix}resetlogchannel`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to reset (disable) the logging channel for a given log type." +
                        $"\n" +
                        $"\nSyntax: `{cmdPrefix}resetlogchannel <logtype>`" +
                        $"\nExample: `{cmdPrefix}resetlogchannel all` would disable all logging in the server.");
                    embed.WithFooter("To see all available log types, and to see what channel the log types are being sent to, use $logtypes");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "logtypes":
                case "loglist":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Log Types | `{cmdPrefix}logtypes`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Administrator**" +
                        $"\n" +
                        $"\nAllows an Administrator to see a list of all available log types. In addition to this, the channels that are " +
                        $"currently occupied by the specified logtype will be displayed. If the log type is not logging at all, it will not " +
                        $"show any channels after it.");
                    embed.WithFooter("Note to Server Admins: This command will put out the log list in the chat channel you call this command from.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "awardeveryone":
                case "awardall":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Awarding Points | `{cmdPrefix}awardeveryone` / `{cmdPrefix}awardall`");
                    embed.WithDescription($"{Context.User.Mention} **Permissions Required: Bot Owner**" +
                        $"\n" +
                        $"\nAllows a bot owner to award a specified number of points to **all** users in their Kaguya database." +
                        $"\nThis can be a negative number, however it can not send a user's points below zero.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "masspointsdistribute":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Mass Distributing Points | `{cmdPrefix}masspointsdistribute`");
                    embed.WithDescription($"{Context.User.Mention} Allows any user to mass redistribute all of their Kaguya Points evenly to the rest of the server. " +
                        $"Upon using this command, your points will be set to zero and they will have been evenly divided amongst everyone in the server. If you do not " +
                        $"have at least one point for every member in the server, the command will not be executed.");
                    embed.WithFooter("What a generous act!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "n":
                    stopWatch.Start();
                    embed.WithTitle($"Help: NSFW | `{cmdPrefix}n`");
                    embed.WithDescription($"{Context.User.Mention} The `{cmdPrefix}n` command will post a 2D image (no real people) in an NSFW channel with the specified tag." +
                        $"\nWhen using the `{cmdPrefix}n` command, append a tag to the end like so: `{cmdPrefix}n <tag>`." +
                        $"\nNSFW Command List: `$n <lewd, boobs, anal, bdsm, bj, classic, cum, feet, eroyuri, pussy, solo, hentai, avatar, trap, yuri, gif, bomb>` (Select one).");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "m":
                    stopWatch.Start();
                    embed.WithTitle($"Help: Music Commands | `{cmdPrefix}m <modifier>`");
                    embed.WithDescription($"{Context.User.Mention} The `{cmdPrefix}p`command group is for all Kaguya Music commands. They are described in detail below:" +
                        $"\n" +
                        $"\n**Play/Pause:** Plays or pauses the music player. `{cmdPrefix}m play <song name>`, `{cmdPrefix}m pause`" +
                        $"\n**Join:** Makes Kaguya join the voice channel you are currently in. `{cmdPrefix}m join`" +
                        $"\n**Leave:** Makes Kaguya leave the voice channel she is currently in. `{cmdPrefix}m leave`" +
                        $"\n**Queue:** Displays Kaguya's playlist. Add more songs to the queue with the play command. `{cmdPrefix}m queue`" +
                        $"\n**Resume:** If Kaguya's music player is paused, she will resume playing music. `{cmdPrefix}m resume`" +
                        $"\n**Skip:** Skips the current song. `{cmdPrefix}m skip`" +
                        $"\n**Volume:** Sets the volume to a value between 0-150. `{cmdPrefix}m volume <0-150>`");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                default:
                    stopWatch.Start();
                    embed.WithDescription($"**{Context.User.Mention} \"{command}\" is not a valid command.**");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User requested a help command for a command that doesn't exist."); break;
            }
        }

        [Command("help")] //help
        [Alias("h")]
        public async Task HelpCommand()
        {
            stopWatch.Start();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            embed.WithTitle("Commands List");
            embed.WithDescription($"All Kaguya commands separated by category. To use the command, have \nthe `{cmdPrefix}` symbol appended before the phrase. For more information on a specific command, " +
                $"type `{cmdPrefix}h <command>`");
            embed.AddField("Administration", "`kick [k]` \n`ban [b]` \n`masskick` \n`massban` \n`massblacklist` `\nmute` \n`unblacklist` \n`removeallroles [rar]` \n`createrole [cr]` \n`deleterole [dr]` \n`shadowban` \n`unshadowban`" +
                "\n`clear [c] [purge]` \n`kaguyaexit` \n`scrapeserver` \n`filteradd [fa]` \n`filterremove [fr]` \n`filterview [fv]` \n`filterclear [clearfilter]` \n`setlogchannel [log]` \n`resetlogchannel [rlog]`" +
                "\n`logtypes [loglist]`", true);
            embed.AddField("Currency", "`points` \n`pointsadd [addpoints]` \n`timely [t]` \n`weekly` \n`timelyreset` \n`roll [gr]` \n`awardeveryone [awardall]` \n`masspointsdistribute`", true);
            embed.AddField("EXP", "`exp` \n`expadd [addexp]` \n`level` \n`rep` \n`repauthor [rep author]` \n`serverexplb [explb]` \n`globalexplb [gexplb]`", true);
            embed.AddField("Fun", "`echo` \n`pick` \n`8ball` \n`slap` \n`hug` \n`kiss` \n`tickle` \n`poke` \n`pat` \n`baka` \n`nekoavatar` \n`smug` \n`waifu` \n`wallpaper` ", true);
            embed.AddField("Help", "`help [h]` \n`helpdm [hdm]` \n`bug`", true);
            embed.AddField("osu!", "`osu` \n`osutop` \n`recent [r]` \n`osuset`", true);
            embed.AddField("Utility", "`modules [mdls]` \n`createtextchannel [ctc]` \n`deletetextchannel [dtc]` \n`createvoicechannel [cvc]` \n`deletevoicechannel [dvc]` \n`prefix` \n`author` \n`commands [cmds]`", true);
            embed.AddField("NSFW", $"`View with {cmdPrefix}cmds nsfw`", true);
            embed.AddField("Music", $"`View with {cmdPrefix}cmds music`", true);
            embed.WithColor(Pink);
            embed.WithFooter($"For more information, including a link to add this bot to your server and a link to the Kaguya Support Discord, type {cmdPrefix}hdm!");
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("helpdm")] //help
        [Alias("hdm")]
        public async Task HelpDM()
        {
            stopWatch.Start();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            embed.WithTitle("Help");
            embed.WithDescription($"{Context.User.Mention} Help is on the way, check your DM!");
            embed.WithColor(Pink);
            await BE();
            await Context.User.SendMessageAsync($"Need help with a specific command? Type `{cmdPrefix}mdls` to see a list of categories the commands are listed under." +
                $"\nType `{cmdPrefix}commands <module name>` to see all commands listed under that module." +
                $"\nType `{cmdPrefix}h <command name>` for more information on how to use the command and a detailed description of what it does." +
                $"\nAdd me to your server with this link!: <https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=2146958847>" +
                $"\nWant to keep track of all the changes? Feel free to check out the Kaguya Github page!: <https://github.com/stageosu/Kaguya>" +
                $"\nKaguya Support Server: https://discord.gg/yhcNC97");
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("bug")]
        public async Task BugReport([Remainder]string report)
        {
            stopWatch.Start();

            var bugChannel = _client.GetChannel(547448889620299826); //Kaguya support server #bugs channel.

            embed.WithTitle($"Bug Report");
            embed.WithDescription($"Report from user `{Context.User.Username}#{Context.User.Discriminator}` with ID: `{Context.User.Id}`" +
                $"\n" +
                $"\nMessage: `\"{report}\"`");
            embed.WithColor(Red);
            embed.WithTimestamp(DateTime.Now);

            await (bugChannel as ISocketMessageChannel).SendMessageAsync($"", false, embed.Build()); //Sends first embed to bug report channel.

            embed.WithTitle($"Bug Report");
            embed.WithDescription($"**{Context.User.Mention} `Your bug report has been sent.`**");
            embed.WithFooter("Thank you for using this feature. Abuse will result in a permanent blacklist from Kaguya.");
            embed.WithColor(Red);
            await BE(); stopWatch.Stop(); //Sends response to user.
            logger.ConsoleBugLog(Context, stopWatch.ElapsedMilliseconds, $"{report}");
        }

        [Command("vote")]
        public async Task Vote()
        {
            stopWatch.Start();
            embed.WithTitle("Discord Bot List Voting");
            embed.WithDescription($"Show Kaguya some love and give her an upvote! https://discordbots.org/bot/538910393918160916/vote" +
                $"\nUsers that upvote receive a `2x` critical hit percentage for the next `12 hours` and `500` Kaguya points! Users may vote every 12 hours!");
            embed.WithFooter($"Thanks for showing your support! Use {Servers.GetServer(Context.Guild).commandPrefix}voteclaim to claim your reward!");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("voteclaim")]
        public async Task VoteClaim()
        {
            stopWatch.Start();
            HttpClient client = new HttpClient();

            UserAccount userAccount = UserAccounts.GetAccount(Context.User);
            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Config.bot.DblApiKey}");
            var dblResponse = await client.GetStringAsync($"https://discordbots.org/api/bots/{Config.bot.BotUserID}/check?userId={Context.User.Id}");

            if (dblResponse.Contains("{\"voted\":1}"))
            {
                if (difference.TotalHours < 12)
                {
                    embed.WithDescription($"**{Context.User.Mention} You've already upvoted me and claimed your reward!" +
                        $"\nTime remaining: `{11 - (int)difference.TotalHours} hours {60 - difference.Minutes} minutes and {60 - difference.Seconds} seconds`**");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
                }
                else if (difference.TotalHours > 12)
                {
                    userAccount.LastUpvotedKaguya = DateTime.Now;
                    userAccount.Points += 500;

                    embed.WithDescription($"{Context.User.Mention} Thanks for upvoting! Your rewards of `500 Kaguya Points` and `2x critical hit rate` have been applied.");
                    embed.WithFooter("Thanks so much for your support!!");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
            }
            else if(dblResponse.Contains("{\"voted\":0}"))
            {
                embed.WithDescription($"**{Context.User.Mention} you have not upvoted me! Please do so with the vote command!**");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop(); logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
        }
    }
}
