using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.UserAccounts;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Kaguya.Core;
using Kaguya.Core.Command_Handler;
using System.Timers;

#pragma warning disable CS0472

namespace Kaguya.Modules
{
    public class Administration : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public Color Violet = new Color(127, 0, 255);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.Token;
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();
        readonly DiscordSocketClient _client = Global.Client;
        

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("mute")]
        [Alias("m")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task MuteMembers(string timeout, [Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();

            var server = Servers.GetServer(Context.Guild);
            var cmdPrefix = server.commandPrefix;
            var roles = Context.Guild.Roles;
            var channels = Context.Guild.Channels;

            var muteRole = roles.FirstOrDefault(x => x.Name == "kaguya-mute");

            if (!roles.Contains(muteRole))
            {
                await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None);
                logger.ConsoleGuildAdvisory("Mute role not found, so I created it.");

                embed.WithDescription($"**{Context.User.Mention} I needed to create the mute role for first time setup! Please retry this command.**");
                embed.WithColor(Violet);
                await BE(); return;

            }

            var regex = new Regex("/([0-9])*s|([0-9])*m|([0-9])*h|([0-9])*d/g");

            Regex[] regexs = {
            new Regex("(([0-9])*s)"),
            new Regex("(([0-9])*m)"),
            new Regex("(([0-9])*h)"),
            new Regex("(([0-9])*d)") };

            var s = regexs[0].Match(timeout).Value;
            var m = regexs[1].Match(timeout).Value;
            var h = regexs[2].Match(timeout).Value;
            var d = regexs[3].Match(timeout).Value;

            var seconds = s.Split('s').First();
            var minutes = m.Split('m').First();
            var hours = h.Split('h').First();
            var days = d.Split('d').First();

            int.TryParse(seconds, out int sec);
            int.TryParse(minutes, out int min);
            int.TryParse(hours, out int hour);
            int.TryParse(days, out int day);

            TimeSpan timeSpan = new TimeSpan(day, hour, min, sec);

            int i = 0;

            if(!(Context.Channel as SocketGuildChannel).GetPermissionOverwrite(muteRole).HasValue)
            {
                embed.WithDescription($"{Context.User.Mention} Performing first time setup. Please wait...");
                embed.WithColor(Violet);
                await BE();
            }

            foreach (var channel in channels)
            {
                if (!channel.GetPermissionOverwrite(muteRole).HasValue)
                {
                    i++;
                    var permissionOverwrite = new OverwritePermissions(PermValue.Inherit, PermValue.Inherit, PermValue.Deny, PermValue.Inherit, PermValue.Deny);
                    await channel.AddPermissionOverwriteAsync(muteRole, permissionOverwrite); //Denies ability to add reactions and send messages.
                }
            }
            if(i > 0)
                logger.ConsoleGuildAdvisory($"{i} channels had their permissions updated for a newly created mute role.");

            foreach (var user in users)
            {
                MuteRetry:
                try
                {
                    var s1 = " seconds";
                    var m1 = " minutes";
                    var h1 = " hours";
                    var d1 = " days";

                    if (sec < 1)
                        s1 = null;
                    if (min < 1)
                        m1 = null;
                    if (hour < 1)
                        h1 = null;
                    if (day < 1)
                        d1 = null;

                    if (sec == 1)
                        s1 = " second";
                    if (min == 1)
                        m1 = " minute";
                    if (hour == 1)
                        h1 = " hour";
                    if (day == 1)
                        d1 = " day";

                    server.MutedMembers.Add(user.Id.ToString(), timeSpan.Duration().ToString());
                    Servers.SaveServers();
                    timeSpanDuration = timeSpan.Duration();
                    await user.AddRoleAsync(muteRole);

                    //Unmute Timer Start

                    Timer timer = new Timer(timeSpan.TotalMilliseconds);
                    timer.Enabled = true;
                    timer.AutoReset = false;
                    timer.Elapsed += UnMute_User_Async_Elapsed;

                    //Unmute Timer End

                    embed.WithDescription($"{Context.User.Mention} User `{user.Username}#{user.Discriminator}` " +
                        $"has been muted for `{days}{d1} {hours}{h1} {minutes}{m1} {seconds}{s1}`");
                    embed.WithColor(Violet);
                    await BE();
                    stopWatch.Stop();
                    logger.ConsoleGuildAdvisory(Context.Guild, $"User muted for {timeout}.");
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
                catch (System.ArgumentException)
                {
                    server.MutedMembers.Remove($"{Context.User.Id}");
                    goto MuteRetry;
                }
            }
        }

        private TimeSpan timeSpanDuration { get; set; }

        private void UnMute_User_Async_Elapsed(object sender, ElapsedEventArgs e)
        {
            var mutedMembers = Servers.GetServer(Context.Guild).MutedMembers;
            var muteRole = _client.GetGuild(Context.Guild.Id).Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

            foreach (var member in mutedMembers.ToList())
            {
                if (member.Value.Contains(timeSpanDuration.ToString()))
                {
                    var result = ulong.TryParse(member.Key, out ulong ID);
                    var user = _client.GetGuild(Context.Guild.Id).GetUser(ID);

                    user.RemoveRoleAsync(muteRole); //Removes mute role from user.
                    mutedMembers.Remove(ID.ToString()); //Removes muted member from the dictionary.
                    Servers.SaveServers();

                    logger.ConsoleTimerElapsed($"User [{user.Username}#{user.Discriminator} | {user.Id}] has been unmuted.");
                }
                else
                {
                    logger.ConsoleGuildAdvisory("I failed to execute the unmute timer.");
                }
            }

        }

        [Command("mute")]
        [Alias("m")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task MuteMembers([Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();

            var server = Servers.GetServer(Context.Guild);
            var cmdPrefix = server.commandPrefix;
            var roles = Context.Guild.Roles;
            var channels = Context.Guild.Channels;

            var muteRole = roles.FirstOrDefault(x => x.Name == "kaguya-mute");

            if (!roles.Contains(muteRole))
            {
                await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None);
                embed.WithDescription($"**{Context.User.Mention} I didn't find my mute role, so I created it. Please try again!**");
                embed.WithColor(Red);
                await BE();
                logger.ConsoleGuildAdvisory("Mute role not found, so I created it.");
                return;
            }

            foreach(var user in users)
            {
                await user.AddRoleAsync(muteRole);
                logger.ConsoleGuildAdvisory(Context.Guild, "User muted.");
            }
        }

        [Command("unmute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task UnMute([Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();
            var roles = Context.Guild.Roles;
            var muteRole = roles.FirstOrDefault(x => x.Name == "kaguya-mute");
            var mutedMembers = Servers.GetServer(Context.Guild).MutedMembers;

            int i = 0;

            foreach(var user in users)
            {
                await user.RemoveRoleAsync(muteRole);
                mutedMembers.Remove(user.Id.ToString());
                Servers.SaveServers();
                i++; logger.ConsoleGuildAdvisory(Context.Guild, "User unmuted.");
            }

            embed.WithDescription($"{Context.User.Mention} Unmuted `{i}` user(s).");
            embed.WithColor(Violet);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("filteradd")] //administration
        [Alias("fa1")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task FilterAdd([Remainder]string phrase) //Adds a word to the server word/phrase filter
        {
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            var server = Servers.GetServer(Context.Guild);
            server.FilteredWords.Add(phrase);
            Servers.SaveServers();

            embed.WithTitle("Filtered word added");
            embed.WithDescription($"**{Context.User.Mention} Successfully added specified word to the filter.**");
            embed.WithFooter($"To view your current list of filtered words, type {cmdPrefix}viewfilter!");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, $"Administrator has added word to their filter: \"{phrase}\"");
        }

        [Command("filterremove")] //administration
        [Alias("fr")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task FilterRemove([Remainder]string phrase)
        {
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            var server = Servers.GetServer(Context.Guild);
            server.FilteredWords.Remove(phrase);
            Servers.SaveServers();

            embed.WithTitle("Filtered word added");
            embed.WithDescription($"**{Context.User.Mention} Successfully removed specified word from the filter.**");
            embed.WithFooter($"To view your current list of filtered words, type {cmdPrefix}filterview!");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, $"Administrator has removed word \"{phrase}\" from their filter");
        }

        [Command("filterview")] //administration
        [Alias("fv", "viewfilter")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task FilterView()
        {
            stopWatch.Start();

            var server = Servers.GetServer(Context.Guild);

            if (server.FilteredWords.Count == 0 || server.FilteredWords == null)
            {
                embed.WithTitle("Empty Filter");
                embed.WithDescription($"{Context.User.Mention} Server filter is empty!");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }

            embed.WithTitle("Filtered Words List");
            int i = 1;
            foreach (var phrase in server.FilteredWords.ToList())
            {
                embed.AddField("#" + i++.ToString(), phrase);
            }
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("filterclear")] //administration
        [Alias("clearfilter")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task FilterClear()
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);

            if (server.FilteredWords.Count == 0)
            {
                embed.WithTitle("Filter Clearing");
                embed.WithDescription($"The filtered words list for **{Context.Guild.Name}** is already empty!");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }

            server.FilteredWords.Clear();
            Servers.SaveServers();

            embed.WithTitle("Cleared Filter");
            embed.WithDescription($"All filtered words for **{Context.Guild.Name}** have been successfully removed!");
            embed.WithColor(Red);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }


        [Command("Unblacklist")] //administration
        [RequireOwner]
        public async Task Unblacklist(SocketUser id)
        {
            stopWatch.Start();
            var userAccount = UserAccounts.GetAccount(id);
            userAccount.Blacklisted = 0;
            UserAccounts.SaveAccounts();

            embed.WithTitle("User Unblacklisted");
            if(userAccount.Username != null)
                embed.WithDescription($"User `{userAccount.Username}` with ID `{userAccount.ID}` has been Unblacklisted from Kaguya functionality.");
            else if(userAccount.Username == null || userAccount.Username == "")
                embed.WithDescription($"ID `{userAccount.ID}` has been Unblacklisted from Kaguya functionality.");
            embed.WithFooter("Please note that all Points and EXP are not able to be restored.");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, "User Unblacklisted");
        }

        [Command("massblacklist")] //administration
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task MassBlacklist(List<SocketGuildUser> users)
        {
            stopWatch.Start();
            await ScrapeServer();

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
                    $"\nYour new Kaguya EXP amount is `{userAccount.EXP}`. Your new Kaguya currency amount is `{userAccount.Points}`." +
                    $"\nUser `{userAccount.Username}` with ID `{userAccount.ID}` has been permanently blacklisted from all Kaguya functions, " +
                    $"and can no longer execute any of the Kaguya commands." +
                    $"\nIf you wish to appeal this blacklist, message `Stage#0001` on Discord.");
                await user.BanAsync();
                await ReplyAsync($"**{user.Mention} has been permanently `banned` and `blacklisted`.**");
                logger.ConsoleGuildAdvisory(Context.Guild, user, stopWatch.ElapsedMilliseconds, $"User {user.Username}#{user.Discriminator} has been blacklisted.");
            }
            stopWatch.Stop();
        }

        [Command("scrapeserver")] //administration
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task ScrapeServer() //Scrapes the server and creates accounts for ALL users, even if they've never typed in chat.
        {
            stopWatch.Start();
            embed.WithDescription($"Creating accounts for **`{Context.Guild.MemberCount}`** users...");
            embed.WithColor(Red);
            await BE();
            var users = Context.Guild.Users;
            foreach (var user in users)
            {
                var userAccount = UserAccounts.GetAccount(user);
                userAccount.Username = user.Username + "#" + user.Discriminator;
                userAccount.ID = user.Id;
                UserAccounts.SaveAccounts();
            }
            Console.WriteLine($"Created accounts for {Context.Guild.MemberCount} users.");
            embed.WithTitle("Admin Server Scraping");
            embed.WithDescription("Accounts obtained.");
            embed.WithColor(Red);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, "Server scraped");
        }

        [Command("scrapedatabase")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task ScrapeDatabase() //Scrapes the entire bot database and creates accounts for every single member of every single guild the bot is in.
        {
            stopWatch.Start();
            var _client = Global.Client;
            var servers = Servers.GetAllServers();
            embed.WithDescription($"**{Context.User.Mention} Scraping...**");
            embed.WithColor(Red);
            await BE();
            foreach (var server in servers)
            {
                var isolatedServer = Servers.GetServer(server.ID);
                ulong isolatedServerID = isolatedServer.ID;
                var guild = _client.GetGuild(isolatedServerID);
                if (guild != null)
                {
                    if (guild.MemberCount > 3500) { continue; } //If the guild has more than 3500 members, don't create accounts for everyone.
                    var guildUsers = guild.Users;
                    foreach (var user in guildUsers) //Creates account for the user, logging name, ID, and what servers they share with Kaguya.
                    {
                        if (user.IsBot) continue;
                        var userAccount = UserAccounts.GetAccount(user);
                        userAccount.Username = user.Username + "#" + user.Discriminator;
                        userAccount.ID = user.Id;

                        if(!userAccount.IsInServerIDs.Contains(guild.Id))
                            userAccount.IsInServerIDs.Add(guild.Id);
                        if (!userAccount.IsInServers.Contains(guild.Name))
                            userAccount.IsInServers.Add(guild.Name);
                    }
                }
            }
            UserAccounts.SaveAccounts();
            embed.WithDescription($"**{Context.User.Mention} Created accounts for `{UserAccounts.GetAllAccounts().Count}` users.**");
            embed.WithColor(Red);
            await BE();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, $"Database scraped: {UserAccounts.GetAllAccounts().Count} users affected.");
        }

        [Command("removeallroles")] //admin
        [Alias("rar")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RemoveAllRoles(SocketGuildUser user)
        {
            stopWatch.Start();
            var roles = user.Roles;

            foreach (IRole role in roles)
            {
                if (role != Context.Guild.EveryoneRole)
                {
                    await user.RemoveRoleAsync(role);
                }
            }

            embed.WithTitle("Remove All Roles");
            embed.WithDescription($"All roles have been removed from `{user}`.");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("deleterole")] //admin
        [Alias("dr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DeleteRole([Remainder]string targetRole)
        {
            stopWatch.Start();
            var roles = Context.Guild.Roles.Where(r => r.Name.ToLower() == targetRole.ToLower());
            if (roles.Count() > 1)
            {
                embed.WithTitle("Role Deletion: Multiple Matching Roles");
                embed.WithDescription($"I found `{roles.Count()}` matches for role `{targetRole}`.");
                foreach (var role in roles)
                {
                    embed.AddField("Role Deleted", $"`{role.Name}` with `{role.Permissions.ToList().Count()}` permissions has been deleted.");
                    await role.DeleteAsync();
                }
                embed.WithColor(Pink);
                await BE();
            }
            else if (roles.Count() == 0)
            {
                embed.WithTitle("Role Deletion: Error");
                embed.WithDescription($"**{Context.User.Mention} I could not find the specified role!**");
                embed.WithColor(Red);
                await BE(); return;
            }
            else if (roles.Count() == 1)
            {
                var role = roles.First();
                await role.DeleteAsync();
                embed.WithTitle("Role Deletion: Success");
                embed.WithDescription($"**{Context.User.Mention} Successfully deleted role `{role.Name}`**");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
        }

        [Command("createrole")] //admin
        [Alias("cr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task CreateRole([Remainder]string role)
        {
            stopWatch.Start();
            await Context.Guild.CreateRoleAsync(role);
            embed.WithTitle("Role Created");
            embed.WithDescription($"{Context.User.Mention} role **{role}** has been created.");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("kaguyaexit")] //admin
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task LeaveGuild()
        {
            embed.WithTitle("Leaving Server");
            embed.WithDescription($"Administrator {Context.User.Mention} has directed me to leave. Goodbye!");
            embed.WithColor(Red);
            await Context.Guild.LeaveAsync();
            logger.ConsoleGuildAdvisory(Context.Guild, "KaguyaExit command executed.");
        }

        [Command("kick")] //admin
        [Alias("k")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, string reason = "No reason provided.")
        {
            stopWatch.Start();

            var sOwner = Context.Guild.Owner;

            if (user.Id != sOwner.Id)
            {
                if (reason != "No reason provided.")
                {
                    await user.KickAsync(reason);
                    embed.WithTitle($"User Kicked");
                    embed.WithDescription($"`{Context.User.Username}#{Context.User.Discriminator}` has kicked `{user}` with reason: \"{reason}\"");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
                else
                {
                    await user.KickAsync(reason);
                    embed.WithTitle($"User Kicked");
                    embed.WithDescription($"`{Context.User.Mention}` has kicked `{user}` without a specified reason.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
            }

            else if (user.Id == Context.User.Id)
            {
                embed.WithDescription($"**{Context.User.Mention} You may not ban yourself!**");
                embed.WithColor(Red);
                await BE();
            }
            else if (user.Id == sOwner.Id)
            {
                embed.WithDescription($"**{Context.User.Mention} I cannot ban the server owner!**");
                embed.WithColor(Red);
                await BE();
            }
        }

        [Command("ban")] //admin
        [Alias("b")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, string reason = "No reason provided.")
        {
            stopWatch.Start();

            var sOwnerID = Context.Guild.Owner.Id;

            if (user.Id != sOwnerID)
            {
                if (reason != "No reason provided.")
                {
                    await user.BanAsync(0, reason);
                    embed.WithTitle($"User Banned");
                    embed.WithDescription($"{Context.User.Mention} has banned `{user}` with reason: \"{reason}\"");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
                else
                {
                    await user.BanAsync(0, reason);
                    embed.WithTitle($"User Banned");
                    embed.WithDescription($"{Context.User.Mention} has banned `{user}` without a specified reason.");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
            }
            else if(user.Id == Context.User.Id)
            {
                embed.WithDescription($"**{Context.User.Mention} You may not ban yourself!**");
                embed.WithColor(Red);
                await BE();
            }
            else if (user.Id == sOwnerID)
            {
                embed.WithDescription($"**{Context.User.Mention} I cannot ban the server owner!**");
                embed.WithColor(Red);
                await BE();
            }
        }

        [Command("massban")] //administration
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task MassBan([Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();
            var sOwner = Context.Guild.Owner;

            foreach (var user in users)
            {
                if(user.Id == Context.User.Id)
                {
                    embed.WithDescription($"**{Context.User.Mention} You may not ban yourself!**");
                    embed.WithColor(Red);
                    await BE();
                    continue;
                }

                if (user.Id == sOwner.Id)
                {
                    embed.WithDescription($"**{Context.User.Mention} You may not ban the server owner!**");
                    embed.WithColor(Red);
                    await BE();
                    continue;
                }

                await user.BanAsync();
                embed.WithDescription($"**{user} has been permanently banned by {Context.User.Mention}.**");
                embed.WithColor(Violet);
                await BE();
            }
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory("Users massbanned.");
        }

        [Command("masskick")] //administration
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task MassKick([Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();

            var sOwner = Context.Guild.Owner;

            foreach (var user in users)
            {
                if (user.Id == Context.User.Id)
                {
                    await ReplyAsync($"**{Context.User.Mention} You may not ban yourself!**");
                    continue;
                }

                if (user.Id == sOwner.Id)
                {
                    await ReplyAsync($"**{Context.User.Mention} You may not ban the server owner!**");
                    continue;
                }

                await user.KickAsync();
                embed.WithDescription($"**{user} has been kicked by {Context.User.Mention}.**");
                embed.WithColor(Pink);
                await BE();
            }
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory(Context.Guild, "Users masskicked.");
        }

        [Command("shadowban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ShadowBan(IGuildUser user) //Shadowbans a user from a server.
        {
            stopWatch.Start();

            embed.WithDescription($"Shadowbanning `[{user.Username}#{user.Discriminator}]`...");
            embed.WithColor(Red);
            await BE();

            var channels = Context.Guild.Channels;
            int i = 0;

            foreach(var channel in channels)
            {
                i++;
                await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.DenyAll(channel));
            }

            embed.WithDescription($"**{Context.User.Mention} User `{user.Username}#{user.Discriminator}` has been shadowbanned from " +
                $"`{Context.Guild.Name} [{i} channels]`**");
            embed.WithColor(Red);
            await BE();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory(Context.Guild, user as SocketGuildUser, stopWatch.ElapsedMilliseconds, "User shadowbanned.");
        }

        [Command("unshadowban")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task UnShadowBan(IGuildUser user) //Un-Shadowbans a user from a server.
        {
            stopWatch.Start();

            embed.WithDescription($"Un-Shadow Banning `[{user.Username}#{user.Discriminator}]`...");
            embed.WithColor(Red);
            await BE();

            var channels = Context.Guild.Channels;
            int i = 0;

            foreach (var channel in channels)
            {
                i++;
                await channel.RemovePermissionOverwriteAsync(user);
            }

            embed.WithDescription($"**{Context.User.Mention} User `{user.Username}#{user.Discriminator}` has been un-shadowbanned from " +
                $"`{Context.Guild.Name} [{i} channels]`**");
            embed.WithColor(Red);
            await BE();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory(Context.Guild, user as SocketGuildUser, stopWatch.ElapsedMilliseconds, "User un-shadowbanned.");
        }

        [Command("clear")] //administration
        [Alias("c", "purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessages(int amount = 10)
        {
            stopWatch.Start();
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            const int delay = 3000;
            var m = await ReplyAsync($"Clearing of messages completed. This message will be deleted in {delay / 1000} seconds.");
            await Task.Delay(delay);
            await m.DeleteAsync();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("setlogchannel")] //administration
        [Alias("log")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task SetLogChannel(string logType, IGuildChannel channel)
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);
            ulong logChannelID = channel.Id;

            switch (logType.ToLower())
            {
                case "deletedmessages":
                    server.LogDeletedMessages = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Deleted Messages` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "messageedits":
                    server.LogMessageEdits = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Edited Messages` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userjoins":
                    server.LogWhenUserJoins = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `User Joins` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userleaves":
                    server.LogWhenUserLeaves = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `User Leaves` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userisbanned":
                    server.LogWhenUserIsBanned = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `User Banned` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userisunbanned":
                    server.LogWhenUserIsUnbanned = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `User Kicked` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "changestologsettings":
                    server.LogChangesToLogSettings = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `changes to log settings` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filteredphrases":
                    server.LogWhenUserSaysFilteredPhrase = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `Filtered Phrases` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userconnectstovoice":
                    server.LogWhenUserConnectsToVoiceChannel = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `user connected to voice` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userdisconnectsfromvoice":
                    server.LogWhenUserDisconnectsFromVoiceChannel = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages for `user disconnected from voice` will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "all":
                    server.LogDeletedMessages = logChannelID;
                    server.LogMessageEdits = logChannelID;
                    server.LogWhenUserJoins = logChannelID;
                    server.LogWhenUserLeaves = logChannelID;
                    server.LogWhenUserIsBanned = logChannelID;
                    server.LogWhenUserIsUnbanned = logChannelID;
                    server.LogChangesToLogSettings = logChannelID;
                    server.LogWhenUserSaysFilteredPhrase = logChannelID;
                    server.LogWhenUserConnectsToVoiceChannel = logChannelID;
                    server.LogWhenUserDisconnectsFromVoiceChannel = logChannelID;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} All log messages will be sent in channel {channel.Name}");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                default:
                    embed.WithTitle("Invalid Log Specification");
                    embed.WithDescription($"{Context.User.Mention} Invalid logging type!");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
            }
        }

        [Command("resetlogchannel")] //administration
        [Alias("rlog")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task ResetLogChannel(string logType)
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);

            switch (logType.ToLower())
            {
                case "deletedmessages":
                    server.LogDeletedMessages = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Deleted Messages` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "messageedits":
                    server.LogMessageEdits = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Edited Messages` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userjoins":
                    server.LogWhenUserJoins = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `User Joins` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userleaves":
                    server.LogWhenUserLeaves = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `User Leaves` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userisbanned":
                    server.LogWhenUserIsBanned = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `User Banned` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userisunbanned":
                    server.LogWhenUserIsUnbanned = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `User Unbanned` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "changestologsettings":
                    server.LogChangesToLogSettings = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `changes to log settings` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "filteredphrases":
                    server.LogWhenUserSaysFilteredPhrase = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `Filtered Phrases` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userconnectstovoice":
                    server.LogWhenUserConnectsToVoiceChannel = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `user connects to voice` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "userdisconnectsfromvoice":
                    server.LogWhenUserDisconnectsFromVoiceChannel = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `user disconnects from voice` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                case "all":
                    server.LogDeletedMessages = 0;
                    server.LogMessageEdits = 0;
                    server.LogWhenUserJoins = 0;
                    server.LogWhenUserLeaves = 0;
                    server.LogWhenUserIsBanned = 0;
                    server.LogWhenUserIsUnbanned = 0;
                    server.LogChangesToLogSettings = 0;
                    server.LogWhenUserSaysFilteredPhrase = 0;
                    server.LogWhenUserConnectsToVoiceChannel = 0;
                    server.LogWhenUserDisconnectsFromVoiceChannel = 0;
                    Servers.SaveServers();
                    embed.WithTitle("Log Channel Set");
                    embed.WithDescription($"{Context.User.Mention} Log messages for `everything` have been disabled");
                    embed.WithColor(Pink);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
                default:
                    embed.WithTitle("Invalid Log Specification");
                    embed.WithDescription($"{Context.User.Mention} Please specify a valid logging type!");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); break;
            }
        }

        [Command("logtypes")] //administration
        [Alias("loglist")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task LogList()
        {
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            var server = Servers.GetServer(Context.Guild);
            ulong[] logChannels = { server.LogDeletedMessages, server.LogMessageEdits, server.LogWhenUserJoins, server.LogWhenUserLeaves,
            server.LogWhenUserIsBanned, server.LogWhenUserIsUnbanned, server.LogChangesToLogSettings, server.LogWhenUserSaysFilteredPhrase,
            server.LogWhenUserConnectsToVoiceChannel, server.LogWhenUserDisconnectsFromVoiceChannel };
            embed.WithTitle("List of Log Events");
            embed.WithDescription($"List of all types of logging you can subscribe to. Use these with {cmdPrefix}log to enable logging!" +
                "\n" +
                $"\n**DeletedMessages** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[0])}`**" +
                $"\n**MessageEdits** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[1])}`**" +
                $"\n**UserJoins** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[2])}`**" +
                $"\n**UserLeaves** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[3])}`**" +
                $"\n**UserIsBanned** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[4])}`**" +
                $"\n**UserIsUnbanned** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[5])}`**" +
                $"\n**ChangesToLogSettings** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[6])}`**" +
                $"\n**FilteredPhrases** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[7])}`**" +
                $"\n**UserConnectsToVoice** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[8])}`**" +
                $"\n**UserDisconnectsFromVoice** - Currently Assigned to: **`#{Context.Guild.GetChannel(logChannels[9])}`**" +
                "\n**All**");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

    }
}
