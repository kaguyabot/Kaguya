using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Owner_Only
{
    public class OwnerCommands : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();
        readonly DiscordShardedClient _client = Global.client;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("serverblacklist")]
        [Alias("sbl")]
        [RequireOwner]
        public async Task OwnerBlacklist(params ulong[] IDs)
        {
            string description = "";

            foreach (var ID in IDs)
            {
                var guild = Servers.GetServer(ID);
                var server = _client.GetGuild(ID);
                guild.IsBlacklisted = true;
                Servers.SaveServers();
                description += $"\n{server.Name} has been blacklisted!";
            }
            embed.WithTitle("Server Blacklist");
            embed.WithDescription(description);
            embed.SetColor(EmbedColor.VIOLET);
            await BE();
        }

        [Command("serverunblacklist")]
        [Alias("subl")]
        [RequireOwner]
        public async Task OwnerUnBlacklist(params ulong[] IDs)
        {
            string description = "";

            foreach (var ID in IDs)
            {
                var guild = Servers.GetServer(ID);
                var server = _client.GetGuild(ID);
                guild.IsBlacklisted = false;
                Servers.SaveServers();
                description += $"\n{server.Name} has been un-blacklisted!";
            }

            embed.WithTitle("Server Un-Blacklist");
            embed.WithDescription(description);
            embed.SetColor(EmbedColor.VIOLET);
            await BE();
        }

        [Command("kaguyawarn")]
        [RequireOwner]
        public async Task KaguyaGlobalWarning(ulong ID, [Remainder]string reason = "No reason provided.")
        {
            stopWatch.Start();
            var user = _client.GetUser(ID);
            UserAccount userAccount = UserAccounts.GetAccount(user);

            var totalWarnings = userAccount.KaguyaWarnings;
            totalWarnings++; //Adds a global Kaguya warning to the user's account.
            userAccount.KaguyaWarnings = totalWarnings;

            UserAccounts.SaveAccounts();

            await user.GetOrCreateDMChannelAsync();

            embed.WithTitle($"⚠️ Kaguya Global Warning");
            embed.WithDescription($"You have received a global warning from a **Kaguya Administrator**. Upon receiving `three` warnings, " +
                $"you will be permanently blacklisted from using Kaguya. This results in all `points`, `experience points`, and `rep` being reset to zero." +
                $"\n" +
                $"\nNote from Administrator `{Context.User}`:" +
                $"\n" +
                $"\n\"{reason}\"");
            embed.WithFooter($"You currently have {totalWarnings} warning(s).");

            await user.SendMessageAsync(embed: embed.Build());

            await GlobalCommandResponses.CreateCommandResponse(Context,
                $"Kaguya Global Warning",
                $"{Context.User.Mention} User `{user}` has received a Kaguya Warning.",
                $"User currently has {totalWarnings} warning(s).");

            if (totalWarnings >= 3)
            {
                userAccount.IsBlacklisted = true;
                userAccount.EXP = 0;
                userAccount.Points = 0;
                userAccount.Rep = 0;

                UserAccounts.SaveAccounts();

                embed.WithTitle($"⚠️ Kaguya Blacklist ⚠️");
                embed.WithDescription($"You have been blacklisted from Kaguya due to receiving too many global warnings from a Kaguya Administrator." +
                    $"\n" +
                    $"\nNew Points Balance: `0`" +
                    $"\nNew EXP Balance: `0`" +
                    $"\nNew Rep Balance: `0`" +
                    $"\n");
                embed.WithFooter($"You currently have {totalWarnings} warning(s).");

                await user.SendMessageAsync(embed: embed.Build());

                embed.WithTitle($"User Blacklisted");
                embed.WithDescription($"User `{user}` has been blacklisted from Kaguya for receiving three global warnings from a Kaguya Administrator.");
                embed.SetColor(EmbedColor.VIOLET);
                await BE();

                logger.ConsoleCriticalAdvisory($"USER {user.ToString().ToUpper()} BLACKLISTED: RECEIVED 3 KAGUYA WARNINGS!!");
            }
            stopWatch.Stop();
        }

        [Command("userunblacklist")] 
        [Alias("uubl")]
        [RequireOwner]
        public async Task Unblacklist(params ulong[] IDs)
        {
            string description = "";
            int i = 0;

            foreach (var ID in IDs)
            {
                var user = _client.GetUser(ID);
                var userAccount = UserAccounts.GetAccount(ID);
                userAccount.IsBlacklisted = false;
                UserAccounts.SaveAccounts();

                description += $"\n`{user.Username}` has been `unblacklisted`.";
                i++;
            }

            if (IDs.Length > 1) //Simply here for grammar.
                embed.WithTitle($"Unblacklisted {i} users.");
            else
                embed.WithTitle($"Unblacklisted {i} user.");
            embed.WithDescription(description);
            embed.SetColor(EmbedColor.VIOLET);
            await BE();
        }

        [Command("userblacklist")]
        [Alias("ubl")]
        [RequireOwner]
        public async Task MassBlacklist(params ulong[] users)
        {
            stopWatch.Start();
            string blacklist = "";

            foreach (var user in users)
            {
                var discordUser = _client.GetUser(user);

                var serverID = Context.Guild.Id;
                var serverName = Context.Guild.Name;
                var userAccount = UserAccounts.GetAccount(user);
                userAccount.EXP = 0;
                userAccount.Points = 0;
                userAccount.IsBlacklisted = true;

                UserAccounts.SaveAccounts();

                blacklist += $"\n**`{discordUser}` has been `blacklisted`.**";
                logger.ConsoleCriticalAdvisory($"{discordUser} has been permanently blacklisted.");
            }

            embed.WithTitle($"Mass Blacklist");
            embed.WithDescription(blacklist);
            await BE();

            stopWatch.Stop();
        }

        [Command("pointsadd")] //currency
        [Alias("addpoints")]
        [RequireOwner]
        public async Task PointsAdd(uint points, IGuildUser user = null)
        {
            if (user == null)
            {
                var userAccount = UserAccounts.GetAccount(Context.User);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{user.Mention} has been awarded {points} points.");
                await BE();
            }
            else if (user is IGuildUser)
            {
                var userAccount = UserAccounts.GetAccount((SocketUser)user);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{user.Mention} has been awarded {points} points.");
                await BE();
            }
            else
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} Unable to add points to {user.Mention}! Make sure they exist and try again!");
                await BE();
            }
        }

        [Command("bugaward")]
        [RequireOwner]
        public async Task BugAward(IUser user)
        {
            var userAccount = UserAccounts.GetAccount(user as SocketUser);
            userAccount.Points += 2000;
            UserAccounts.SaveAccounts();

            await user.GetOrCreateDMChannelAsync();
            await user.SendMessageAsync($"Hello, you reported a bug that led to a fix! As a reward, `2,000 Kaguya Points` have been added to your account. Thank you for your contribution, and " +
                $"if you notice anymore bugs, don't hesitate to keep using the bug reporter!" +
                $"\n" +
                $"\n-Stage.");

            embed.WithDescription($"{Context.User.Mention} The user has been rewarded and was sent a DM.");
            await BE();
        }

        [Command("awardeveryone")] //currency
        [Alias("awardall")]
        [RequireOwner]
        public async Task AwardEveryone(int bonus)
        {
            var userAccounts = UserAccounts.GetAllAccounts();
            int i = 0;
            foreach (UserAccount account in userAccounts)
            {
                i++;
                account.Points = (uint)(account.Points + bonus);
            }
            UserAccounts.SaveAccounts();
            embed.WithTitle("Points Awarded");
            embed.WithDescription($"{Context.User.Mention} has awarded `{bonus.ToString("N0")}` points to `{i.ToString("N0")}` users!");
            embed.SetColor(EmbedColor.GOLD);
            await BE();
        }

        [Command("timelyreset")] //currency
        [RequireOwner]
        public async Task TimelyReset()
        {
            var accounts = UserAccounts.GetAllAccounts();
            foreach (var account in accounts)
            {
                var difference = DateTime.Now.AddHours(-24);
                account.LastReceivedTimelyPoints = difference;
            }
            embed.WithTitle("Timely Reset");
            embed.WithDescription($"**{Context.User.Mention} Timely points for `{accounts.Count}` users have been reset!**");

            await BE();
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
            embed.WithDescription($"{user.Mention} has gained {exp} EXP.");
            await BE();
        }

        [Command("owner")]
        [RequireOwner]
        public async Task OwnerOnlyCommands()
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            string commands = "```css" +
                "\nAll commands in category: Administration" +
                "\n" +
                $"\n{cmdPrefix}bugaward" +
                $"\n{cmdPrefix}expadd [addexp]" +
                $"\n{cmdPrefix}kaguyawarn" +
                $"\n{cmdPrefix}kill" +
                $"\n{cmdPrefix}massblacklist" +
                $"\n{cmdPrefix}pointsadd [addpoints]" +
                $"\n{cmdPrefix}restart" +
                $"\n{cmdPrefix}serverblacklist [sbl]" +
                $"\n{cmdPrefix}serverunblacklist [subl]" +
                $"\n{cmdPrefix}timelyreset" +
                $"\n{cmdPrefix}userblacklist [ubl]" +
                $"\n{cmdPrefix}userunblacklist [uubl]" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                "\n```";

            embed.WithTitle("Owner Commands");
            embed.WithDescription(commands);
            await BE();
        }

        [Command("delteams")] //osu
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task DeleteTeams()
        {
            var roles = Context.Guild.Roles;
            embed.WithTitle("Teams Deleted");
            embed.WithDescription("The following teams have been deleted: ");
            foreach (IRole role in roles)
            {
                if (role.Name.Contains("Team: "))
                {
                    await role.DeleteAsync();
                    embed.WithDescription(embed.Description.ToString() + $"\n`{role}`");
                }
            }
            await BE();
        }

        [RequireOwner]
        [Command("reloadconfig")]
        [Alias("rc")]
        public async Task ReloadAccounts()
        {
            KaguyaLogMethods.LoadKaguyaData();
            embed.WithDescription("User accounts and servers have been reloaded.");
            embed.SetColor(EmbedColor.VIOLET);
            await BE();
        }

        [Command("restart")]
        [RequireOwner]
        public async Task Restart()
        {
            embed.WithDescription($"**{Context.User.Mention} Attempting to restart...**");
            await BE(); logger.ConsoleCriticalAdvisory("Attempting to restart...");

            var filePath = Assembly.GetExecutingAssembly().Location;
            Process.Start(filePath); logger.ConsoleCriticalAdvisory("Process started!!");

            embed.WithDescription($"**{Context.User.Mention} Process started successfully. Exiting...**");
            await BE();

            Environment.Exit(0);
        }

        [Command("kill")]
        [RequireOwner]
        public async Task Kill()
        {
            embed.WithDescription($"**{Context.User.Mention} Exiting...**");
            await BE(); logger.ConsoleCriticalAdvisory("Exiting!!");
            Environment.Exit(0);
        }
    }
}
