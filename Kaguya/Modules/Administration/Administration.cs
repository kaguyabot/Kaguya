﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Kaguya.Core.Embed;
using EmbedType = Kaguya.Core.Embed.EmbedType;

#pragma warning disable CS0472

namespace Kaguya.Modules
{
    public class AdministrationCommands : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();
        readonly DiscordShardedClient _client = Global.Client;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("warn")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task WarnMembers([Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);
            var warnActions = server.WarnActions;
            var warnedMembers = server.WarnedMembers;

            foreach (var user in users)
            {
                var userID = user.Id;
                int i = 0;
                warnedMembers.TryGetValue(userID, out int userWarnings); //Gets the user's total warnings
                userWarnings++; //Increments the user's warnings by 1.
                
                if (warnedMembers.ContainsKey(userID)) //If it exists in the dictionary, remove and replace it.
                {
                    warnedMembers.Remove(userID);
                }

                await user.GetOrCreateDMChannelAsync();

                EmbedBuilder embed2 = new EmbedBuilder();
                embed2.WithTitle("⚠️ Warning Received");
                embed2.WithDescription($"You have received a warning from `{Context.User}` in the server `{Context.Guild.Name}`.");
                embed2.WithFooter($"You currently have {userWarnings} warnings.");
                await user.SendMessageAsync(embed: embed2.Build());

                warnedMembers.Add(userID, userWarnings);
                embed.WithDescription($"{Context.User.Mention} **User `{users.ElementAt(i)}` has been warned.**");
                embed.WithFooter($"User now has {userWarnings} warning(s).");
                embed.SetColor(EmbedType.VIOLET);
                await ReplyAsync(embed: embed.Build());
                i++;

                if (warnActions.Values.Contains(userWarnings)) //If a user has the same amount of warnings (or more) as a warnaction, do stuff.
                {
                    int[] warnNums = new int[4];

                    warnActions.TryGetValue("mute", out warnNums[3]);
                    warnActions.TryGetValue("kick", out warnNums[2]);
                    warnActions.TryGetValue("shadowban", out warnNums[1]);
                    warnActions.TryGetValue("ban", out warnNums[0]);

                    if (userWarnings >= warnNums[3] && warnNums[3] != 0)
                        await Mute(user);
                    if (userWarnings >= warnNums[2] && warnNums[2] != 0)
                        await Kick(user);
                    if (userWarnings >= warnNums[1] && warnNums[1] != 0)
                        await ShadowBan(user);
                    else if (userWarnings >= warnNums[0] && warnNums[0] != 0)
                        await Ban(user);

                    Console.WriteLine("Ban: " + warnNums[0]);
                    Console.WriteLine("Shadowban: " + warnNums[1]);
                    Console.WriteLine("Kick: " + warnNums[2]);
                    Console.WriteLine("Mute: " + warnNums[3]);

                }
                Servers.SaveServers();
            }
        }

        [Command("warnset")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task WarnSettings(int warnNum, [Remainder]string warnAction)
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);

            string warning = "warning"; //Simply here for grammar.
            switch(warnNum)
            {
                case 1: warning = "warning"; break;
                default: warning = "warnings"; break;
            }

            switch(warnAction.ToLower()) //Ensures that the user is specifying a valid warning action.
            {
                case "mute":
                case "kick":
                case "shadowban":
                case "ban": break;
                default:
                    await GlobalCommandResponses.CreateCommandError(Context,
                        stopWatch.ElapsedMilliseconds,
                        CommandError.Unsuccessful,
                        "User attempted to set an invalid warning action.",
                        "Warn Settings",
                        $"{Context.User.Mention} Please specify a valid warning action!",
                        $"Use the \"warnoptions\" command to view your options!");
                    return;
            }

            if (server.WarnActions.ContainsKey(warnAction.ToLower())) //If the dictionary already contains the warn action, delete (and replace) it.
            {
                server.WarnActions.Remove(warnAction.ToLower());
            }

            server.WarnActions.Add(warnAction.ToLower(), warnNum);
            Servers.SaveServers();

            await GlobalCommandResponses.CreateCommandResponse(Context,
                stopWatch.ElapsedMilliseconds,
                "Warn Settings Changed",
                $"I now have `{warnAction.ToLower()}s` set to occur on `{warnNum}` {warning}.");
        }

        [Command("warnoptions")]
        public async Task WarnOptions()
        {
            stopWatch.Start();
            await GlobalCommandResponses.CreateCommandResponse(Context,
                stopWatch.ElapsedMilliseconds,
                "Warn Options",
                $"{Context.User.Mention} The following warning options are available:" +
                $"\n" +
                $"\n```Mute, Kick, Shadowban, Ban```");
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
                stopWatch.ElapsedMilliseconds,
                $"Kaguya Global Warning",
                $"{Context.User.Mention} User `{user}` has received a Kaguya Warning.",
                $"User currently has {totalWarnings} warning(s).");

            if (totalWarnings >= 3)
            {
                userAccount.Blacklisted = 1;
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
                embed.SetColor(EmbedType.VIOLET);
                await BE();

                logger.ConsoleCriticalAdvisory($"USER {user.ToString().ToUpper()} BLACKLISTED: RECEIVED 3 KAGUYA WARNINGS!!");
            }
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task Mute(string timeout, [Remainder]List<SocketGuildUser> users)
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
                embed.SetColor(EmbedType.VIOLET);
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
                embed.SetColor(EmbedType.VIOLET);
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
                    TimeSpanDuration = timeSpan.Duration();
                    await user.AddRoleAsync(muteRole);

                    //Unmute Timer Start

                    Timer timer = new Timer(timeSpan.TotalMilliseconds)
                    {
                        Enabled = true,
                        AutoReset = false
                    };
                    timer.Elapsed += UnMute_User_Async_Elapsed;

                    //Unmute Timer End

                    embed.WithDescription($"{Context.User.Mention} User `{user.Username}#{user.Discriminator}` " +
                        $"has been muted for `{days}{d1} {hours}{h1} {minutes}{m1} {seconds}{s1}`");
                    embed.SetColor(EmbedType.VIOLET);
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

        private TimeSpan TimeSpanDuration { get; set; }

        private void UnMute_User_Async_Elapsed(object sender, ElapsedEventArgs e)
        {
            var mutedMembers = Servers.GetServer(Context.Guild).MutedMembers;
            var muteRole = _client.GetGuild(Context.Guild.Id).Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

            foreach (var member in mutedMembers.ToList())
            {
                if (member.Value.Contains(TimeSpanDuration.ToString()))
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
                await BE();
                logger.ConsoleGuildAdvisory("Mute role not found, so I created it.");
                return;
            }

            int i = 0;

            if (!(Context.Channel as SocketGuildChannel).GetPermissionOverwrite(muteRole).HasValue)
            {
                embed.WithDescription($"{Context.User.Mention} Performing first time setup. Please wait...");
                embed.SetColor(EmbedType.VIOLET);
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
            if (i > 0)
                logger.ConsoleGuildAdvisory($"{i} channels had their permissions updated for a newly created mute role.");

            foreach (var user in users)
            {
                await user.AddRoleAsync(muteRole);
                embed.WithDescription($"{Context.User.Mention} User `{user}` has been muted.");
                embed.SetColor(EmbedType.VIOLET);
                await BE();
                logger.ConsoleGuildAdvisory(Context.Guild, "User muted.");
            }

            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("mute")]
        [Alias("m")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task Mute(IGuildUser user)
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
                await BE();
                logger.ConsoleGuildAdvisory("Mute role not found, so I created it.");
                return;
            }

            int i = 0;

            if (!(Context.Channel as SocketGuildChannel).GetPermissionOverwrite(muteRole).HasValue)
            {
                embed.WithDescription($"{Context.User.Mention} Performing first time setup. Please wait...");
                embed.SetColor(EmbedType.VIOLET);
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
            if (i > 0)
                logger.ConsoleGuildAdvisory($"{i} channels had their permissions updated for a newly created mute role.");

            await user.AddRoleAsync(muteRole);
            embed.WithDescription($"{Context.User.Mention} User `{user}` has been muted.");
            embed.SetColor(EmbedType.VIOLET);
            await BE();
            logger.ConsoleGuildAdvisory(Context.Guild, "User muted.");

            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
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
            embed.SetColor(EmbedType.VIOLET);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("filteradd")] //administration
        [Alias("fa")]
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
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }

            embed.WithTitle("Filtered Words List");
            int i = 1;
            foreach (var phrase in server.FilteredWords.ToList())
            {
                embed.AddField("#" + i++.ToString(), phrase);
            }
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
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }

            server.FilteredWords.Clear();
            Servers.SaveServers();

            embed.WithTitle("Cleared Filter");
            embed.WithDescription($"All filtered words for **{Context.Guild.Name}** have been successfully removed!");
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
            await BE();
            foreach (var server in servers)
            {
                var isolatedServer = Servers.GetServer(server.ID, Context.Guild.Name);
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
            int i = 0;

            foreach (IRole role in roles)
            {
                if (role != Context.Guild.EveryoneRole)
                {
                    try
                    {
                        i++;
                        await user.RemoveRoleAsync(role);
                    }
                    catch(Discord.Net.HttpException)
                    {
                        i--;
                        embed.WithDescription($"{Context.User.Mention} I failed to remove {role.Mention} from `{user}`. This most likely occurred because this role is higher than yours in the hierarchy, " +
                            $"or this role is automatically managed by an integration (bot specific roles).");
                        await BE();
                    }
                }
            }

            if (i > 0)
            {
                embed.WithTitle("Remove All Roles");
                embed.WithDescription($"`{i}` roles have been removed from `{user}`.");
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else
            {
                embed.WithDescription($"Failed to remove roles from `{user}`.");
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "Failed to remove roles from user.");
            }
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
                await BE();
            }
            else if (roles.Count() == 0)
            {
                embed.WithTitle("Role Deletion: Error");
                embed.WithDescription($"**{Context.User.Mention} I could not find the specified role!**");
                await BE(); return;
            }
            else if (roles.Count() == 1)
            {
                var role = roles.First();
                await role.DeleteAsync();
                embed.WithTitle("Role Deletion: Success");
                embed.WithDescription($"**{Context.User.Mention} Successfully deleted role `{role.Name}`**");
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
            
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("kaguyaexit")] //admin
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task LeaveGuild()
        {
            embed.WithTitle("Leaving Server");
            embed.SetColor(EmbedType.RED);
            embed.WithDescription($"Administrator {Context.User.Mention} has directed me to leave. Goodbye!");
            await BE();
            await Context.Guild.LeaveAsync();
            logger.ConsoleGuildAdvisory(Context.Guild, "KaguyaExit command executed.");
        }

        [Command("ban")] //admin
        [Alias("b")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, [Remainder]string reason = "No reason specified.")
        {
            stopWatch.Start();
            Console.WriteLine(reason);
            var sOwnerID = Context.Guild.Owner.Id;

            if(user.Id == Context.User.Id)
            {
                embed.WithDescription($"**{Context.User.Mention} You may not ban yourself!**");
                await BE();
            }
            else if (user.Id == sOwnerID)
            {
                embed.WithDescription($"**{Context.User.Mention} I cannot ban the server owner!**");
                await BE();
            }
            else if (user.Id != sOwnerID)
            {
                await user.BanAsync();
                embed.WithTitle($"User Banned");
                embed.WithDescription($"{Context.User.Mention} has banned `{user}`.");
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                Servers.GetServer(Context.Guild).MostRecentBanReason = reason;
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
                    await BE();
                    continue;
                }

                if (user.Id == sOwner.Id)
                {
                    embed.WithDescription($"**{Context.User.Mention} You may not ban the server owner!**");
                    await BE();
                    continue;
                }

                await user.BanAsync();
                embed.WithDescription($"**{user} has been permanently banned by {Context.User.Mention}.**");
                embed.SetColor(EmbedType.VIOLET);
                await BE();
            }
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory("Users massbanned.");
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
                    embed.WithDescription($"`{Context.User}` has kicked `{user}` with reason: \"{reason}\"");
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
                else
                {
                    await user.KickAsync(reason);
                    embed.WithTitle($"User Kicked");
                    embed.WithDescription($"`{Context.User.Mention}` has kicked `{user}` without a specified reason.");
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
            }

            else if (user.Id == Context.User.Id)
            {
                embed.WithDescription($"**{Context.User.Mention} You may not ban yourself!**");
                await BE();
            }
            else if (user.Id == sOwner.Id)
            {
                embed.WithDescription($"**{Context.User.Mention} I cannot ban the server owner!**");
                await BE();
            }
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
                await BE();
            }
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory(Context.Guild, "Users masskicked.");
        }

        [Command("clear")] //administration
        [Alias("c", "purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessages(int amount = 10)
        {
            stopWatch.Start();

            if(!(amount > 0))
            {
                await GlobalCommandResponses.CreateCommandError(Context,
                    stopWatch.ElapsedMilliseconds,
                    CommandError.Unsuccessful, 
                    "User tried to clear less than one message.",
                    "Clearing Messages", 
                    "The number of messages to be deleted must be greater than zero!");
                return;
            }

            if(!(amount <= 100))
            {
                await GlobalCommandResponses.CreateCommandError(Context,
                    stopWatch.ElapsedMilliseconds,
                    CommandError.Unsuccessful,
                    "User tried to clear more than 100 messages.",
                    "Clearing Messages",
                    "The number of messages to be deleted must not be more than 100!");
                return;
            }

            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            var m = await ReplyAsync($"Clearing of messages completed. This message will be deleted in 3 seconds.");
            await m.DeleteAsync();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            stopWatch.Stop();
        }

        [Command("shadowban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ShadowBan(IGuildUser user, [Remainder]string reason = "No reason specified.") //Shadowbans a user from a server.
        {
            stopWatch.Start();

            var server = Servers.GetServer(Context.Guild);
            server.MostRecentShadowbanReason = reason;

            embed.WithDescription($"Shadowbanning `[{user.Username}#{user.Discriminator}]`...");
            await BE();

            var channels = Context.Guild.Channels;
            int i = 0;

            foreach (var channel in channels)
            {
                i++;
                await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.DenyAll(channel));
            }

            embed.WithDescription($"**{Context.User.Mention} User `{user.Username}#{user.Discriminator}` has been shadowbanned from " +
                $"`{Context.Guild.Name} [{i} channels]`**");
            await BE();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory(Context.Guild, user as SocketGuildUser, stopWatch.ElapsedMilliseconds, "User shadowbanned.");

            if (server.LogShadowbans != 0) //Shadowban logtype.
            {
                KaguyaLogMethods logMethods = new KaguyaLogMethods();
                await logMethods.LoggingUserShadowbanned(user as SocketUser, Context.Guild);
            }
        }

        [Command("unshadowban")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task UnShadowBan(IGuildUser user) //Un-Shadowbans a user from a server.
        {
            stopWatch.Start();
            Server server = Servers.GetServer(Context.Guild);

            embed.WithDescription($"Un-Shadow Banning `[{user.Username}#{user.Discriminator}]`...");
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
            await BE();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory(Context.Guild, user as SocketGuildUser, stopWatch.ElapsedMilliseconds, "User un-shadowbanned.");

            if (server.LogUnShadowbans != 0)
            {
                KaguyaLogMethods logMethods = new KaguyaLogMethods();
                await logMethods.LoggingUserUnShadowbanned(user as SocketUser, Context.Guild);
            }
        }
    }
}
