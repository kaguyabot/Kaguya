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



namespace Kaguya.Modules
{
    public class Administration : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.Token;
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        //[Command("mute")]
        //[Alias("m")]
        //[RequireUserPermission(GuildPermission.ManageMessages)]
        //[RequireUserPermission(GuildPermission.MuteMembers)]
        //[RequireBotPermission(GuildPermission.ManageMessages)]
        //[RequireBotPermission(GuildPermission.MuteMembers)]
        //public async Task MuteMembers(string timeout, [Remainder]List<SocketGuildUser> users)
        //{
        //    Stopwatch.Start();
        //    var server = Servers.GetServer(Context.Guild);

        //    if (timeout.Contains('s'))
        //    {
        //        var seconds = timeout.Split('s').First();
        //    }
        //    if (timeout.Contains('m'))
        //    {
        //        var minutes = timeout.Split('m').First();
        //    }
        //    if (timeout.Contains('h'))
        //    {
        //        var hours = timeout.Split('h').First();
        //    }


        //    foreach (SocketGuildUser user in users)
        //    {
        //        server.MutedMembers.Add($"{user.Username}#{user.Discriminator} {timeout}");
        //    }



        //    Stopwatch.Stop();
        //    logger.ConsoleCommandLog(ContextBoundObject, Stopwatch.ElapsedMilliseconds);
        //}


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
            var roles = Context.Guild.Roles.Where(r => r.Name == targetRole);
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
        }

        [Command("kick")] //admin
        [Alias("k")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, string reason = "No reason provided.")
        {
            stopWatch.Start();
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

        [Command("ban")] //admin
        [Alias("b")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, string reason = "No reason provided.")
        {
            stopWatch.Start();
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

        [Command("massban")] //administration
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task MassBan([Remainder]List<SocketGuildUser> users)
        {
            stopWatch.Start();
            foreach (var user in users)
            {
                await user.BanAsync();
                await ReplyAsync($"**{user} has been permanently banned by {Context.User.Mention}.**");
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
            foreach (var user in users)
            {
                await user.BanAsync();
                await ReplyAsync($"**{user} has been kicked by {Context.User.Mention}.**");
            }
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            logger.ConsoleGuildAdvisory("Users masskicked.");
        }

        [Command("shadowban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ShadowBan(IGuildUser user) //Shadowbans a user from a server.
        {
            stopWatch.Start();

            embed.WithDescription($"Shadowbanning `{user.Nickname}` [{user.Username}#{user.Discriminator}]...");
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
                $"{Context.Guild.Name} [{i} channels]**");
            embed.WithColor(Red);
            await BE();
            stopWatch.Stop();
            logger.ConsoleGuildAdvisory(Context.Guild, user as SocketGuildUser, stopWatch.ElapsedMilliseconds, "User shadowbanned.");
        }

        [Command("unshadowban")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task UnShadowBan(IGuildUser user) //Un-Shadowbans a user from a server.
        {
            stopWatch.Start();

            embed.WithDescription($"Un-Shadow Banning `{user.Nickname}` [{user.Username}#{user.Discriminator}]...");
            embed.WithColor(Red);
            await BE();

            var channels = Context.Guild.Channels;
            int i = 0;

            foreach (var channel in channels)
            {
                i++;
                await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.InheritAll);
            }

            embed.WithDescription($"**{Context.User.Mention} User `{user.Username}#{user.Discriminator}` has been un-shadowbanned from " +
                $"{Context.Guild.Name} [{i} channels]**");
            embed.WithColor(Red);
            await BE();
            stopWatch.Stop();
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
