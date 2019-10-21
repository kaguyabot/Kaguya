using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Utility
{
    public class Utility : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        private readonly DiscordShardedClient _client = Global.client;
        Logger logger = new Logger();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("remindme")]
        public async Task RemindMe(string time, [Remainder]string text)
        {
            var user = UserAccounts.GetAccount(Context.User);
            var regex = new Regex("/([0-9])*s|([0-9])*m|([0-9])*h|([0-9])*d/g");

            Regex[] regexs = {
            new Regex("(([0-9])*s)"),
            new Regex("(([0-9])*m)"),
            new Regex("(([0-9])*h)"),
            new Regex("(([0-9])*d)") };

            var s = regexs[0].Match(time).Value;
            var m = regexs[1].Match(time).Value;
            var h = regexs[2].Match(time).Value;
            var d = regexs[3].Match(time).Value;

            var seconds = s.Split('s').First();
            var minutes = m.Split('m').First();
            var hours = h.Split('h').First();
            var days = d.Split('d').First();

            int.TryParse(seconds, out int sec);
            int.TryParse(minutes, out int min);
            int.TryParse(hours, out int hour);
            int.TryParse(days, out int day);

            TimeSpan timeSpan = new TimeSpan(day, hour, min, sec);

            string s1 = " seconds";
            string m1 = " minutes";
            string h1 = " hours";
            string d1 = " days";

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

            if (s1 == null && m1 == null && h1 == null && d1 == null)
            {
                embed.WithDescription($"{Context.User.Mention} You must specify a time of at least `1s` for me to remind you for.");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            double remindTime = DateTime.Now.AddSeconds(timeSpan.TotalSeconds).ToOADate();

            Dictionary<string, double> reminderDictionary = new Dictionary<string, double>();
            reminderDictionary.Add(text, remindTime);
            user.Reminders.Add(reminderDictionary);

            embed.WithDescription($"{Context.User.Mention} I will remind you to `{text}` " +
                $"in `{days}{d1} {hours}{h1} {minutes}{m1} {seconds}{s1}`");
            embed.WithFooter($"Note: You must have DMs open to server members (at least for this server), " +
                $"or else your reminder will not be delivered!!");
            embed.SetColor(EmbedColor.BLUE);
            await BE();
            logger.ConsoleGuildAdvisory(Context.Guild, $"User has requested to be reminded in " +
                $"{days}{d1} {hours}{h1} {minutes}{m1} {seconds}{s1} to \"{text}\"");
        }

        [Command("changelog")]
        public async Task KaguyaChangelog()
        {
            string directory = Directory.GetCurrentDirectory();
            string filePath = Path.GetFullPath(Path.Combine(directory, @"..\..\..\ChangeLog.txt")); //Nav 3 folders up.
            string description = "";

            using(StreamReader reader = new StreamReader(filePath))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line.Contains(Utilities.GetAlert("OLDVERSION")))
                    {
                        reader.Dispose();
                        break;
                    }
                    if (line.Contains("Changelog"))
                        continue;

                    string newLine = line.Replace("-", $"{Environment.NewLine}-");

                    if(description.Count() >= 1800)
                    {
                        description += "\n\nThis changelog is longer than what I can display. " +
                            "View the rest in the Kaguya Support server!";
                        break;
                    }
                    description += newLine;
                }

                embed.WithTitle($"Kaguya Changelog: {Utilities.GetAlert("VERSION")}");
                embed.WithDescription($"```{description}```");
                embed.SetColor(EmbedColor.BLUE);
                await BE();
            }
        }

        [Command("ping")]
        public async Task Ping()
        {
            embed.WithTitle("Pong! 🏓");
            embed.WithDescription($"\n📡 Discord Latency: {Global.client.Latency}ms");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("stats")]
        public async Task KaguyaStats()
        {
            var shard = _client.GetShardIdFor(Context.Guild as IGuild) + 1;

            int textChannels = 0;
            int voiceChannels = 0;
            uint totalCurrency = 0;
            uint totalDiamonds = 0;
            uint totalSupporters = 0;
            double totalGambles = 0;
            double ramUsage = Global.ramUsage;

            foreach (var guild in _client.Guilds)
            {
                textChannels += guild.TextChannels.Count;
                voiceChannels += guild.VoiceChannels.Count;
            }

            foreach(var account in Global.UserAccounts)
            {
                totalCurrency += account.Points;
                totalDiamonds += account.Diamonds;
                if (account.IsSupporter)
                    totalSupporters++;
                totalGambles += account.LifetimeGambles;
            }

            embed.WithTitle($"Kaguya Statistics");
            embed.AddField("Author",
                $"User: `Stage#0001`" +
                $"\nID: `146092837723832320`");

            embed.AddField("Command Stats",
                $"Commands Run Today: **`{File.ReadAllLines($"{Directory.GetCurrentDirectory()}/Logs/SuccessfulCommandLogs/KaguyaLogger_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.csv").Count().ToString("N0")}`**");

            embed.AddField($"Shard Stats",
                $"Version: **`{Utilities.GetAlert("VERSION")}`**" +
                $"\nCurrent Shard: **`{shard}/{Global.ShardsLoggedIn}`**" +
                $"\nGuilds: **`{_client.GetShard(_client.GetShardIdFor(Context.Guild)).Guilds.Count.ToString("N0")}`**" +
                $"\nText Channels: **`{textChannels.ToString("N0")}`**" +
                $"\nVoice Channels: **`{voiceChannels.ToString("N0")}`**");

            var timeDiff = DateTime.Now - Process.GetCurrentProcess().StartTime;

            embed.AddField($"Global Stats",
             $"Uptime: **`{timeDiff.Days.ToString("N0")} days, {timeDiff.Hours} hours, {timeDiff.Minutes} minutes {timeDiff.Seconds} seconds`**" +
             $"\nGuilds: **`{Global.client.Guilds.Count.ToString("N0")}`**" +
             $"\nMembers: **`{Global.TotalMemberCount.ToString("N0")}`**" +
             $"\nText Channels: **`{Global.TotalTextChannels.ToString("N0")}`**" +
             $"\nVoice Channels: **`{Global.TotalVoiceChannels.ToString("N0")}`**" +
             $"\nRAM Usage: **`{Global.ramUsage.ToString("N2")}MB`**" +
             $"\n");

            embed.AddField($"User Stats",
                $"Registered Users: **`{Global.UserAccounts.Count.ToString("N0")}`**" +
                $"\nTotal Currency: **`{totalCurrency.ToString("N0")}`**" +
                $"\nTotal <a:KaguyaDiamonds:581562698228301876>: **`{totalDiamonds.ToString("N0")}`**" +
                $"\nTotal Gambles: **`{totalGambles.ToString("N0")}`**");

            await BE();
        }

        [Command("toggleannouncements")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ToggleAnnouncements()
        {
            Server guild = Servers.GetServer(Context.Guild);
            var cmdPrefix = guild.CommandPrefix;
            if (guild.MessageAnnouncements == true)
            {
                guild.MessageAnnouncements = false;

                embed.WithTitle("Level Up Announcements");
                embed.WithDescription($"**{Context.User.Mention} Level up announcements have been disabled.**");
                embed.WithFooter($"To re-enable, use {cmdPrefix}toggleannouncements again.");
                await BE();
                logger.ConsoleGuildAdvisory(Context.Guild, "Level up announcements disabled.");
            }
            else if(guild.MessageAnnouncements == false)
            {
                guild.MessageAnnouncements = true;

                embed.WithTitle("Level Up Announcements");
                embed.WithDescription($"**{Context.User.Mention} Level up announcements have been enabled.**");
                embed.WithFooter($"To disable, use {cmdPrefix}toggleannouncements again.");
                await BE();
                logger.ConsoleGuildAdvisory(Context.Guild, "Level up announcements enabled."); return;
            }
        }

        [Command("prefix")] //utility
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AlterPrefix(string prefix = "$")
        {
            var cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            var server = Servers.GetServer(Context.Guild);
            var oldPrefix = server.CommandPrefix;

            if(prefix.Length > 3)
            {
                embed.WithTitle("Change Command Prefix: Failure!");
                embed.WithDescription("The chosen prefix is too long! Please select a combination of less than 4 characters/symbols ");
                embed.WithFooter($"To reset the command prefix, type {cmdPrefix}prefix!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            server.CommandPrefix = prefix;

            embed.WithTitle("Change Command Prefix: Success!");
            embed.WithDescription($"The command prefix has been changed from `{oldPrefix}` to `{server.CommandPrefix}`.");
            embed.WithFooter($"If you ever forget the prefix, tag me and type \"prefix\"!");
            await BE();
        }
        
        [Command("author")] //utility
        public async Task Author()
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            var author = UserAccounts.GetAuthor();

            embed.WithTitle("Kaguya Author");
            embed.WithDescription($"Programmed with love by `{author.Username}` uwu");
            embed.WithFooter($"{author.Username} is level {author.LevelNumber} with {author.EXP} EXP and has +{author.Rep} rep!" +
                $"\nTo +rep Stage, type `{cmdPrefix}rep author`!");
            await BE();
        }

        [Command("createtextchannel")] //utility
        [Alias("ctc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateTextChannel([Remainder]string name)
        {
            var channel = await Context.Guild.CreateTextChannelAsync(name);
            embed.WithTitle("Text Channel Created");
            embed.WithDescription($"{Context.User.Mention} has successfully created the text channel `#{name}`.");
            await BE();
        }

        [Command("deletetextchannel")] //utility
        [Alias("dtc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildDeleteTextChannel(SocketGuildChannel channel)
        {
            foreach (var Channel in Context.Guild.TextChannels)
            {
                if (Channel == channel)
                {
                    await channel.DeleteAsync();
                    embed.WithTitle("Text Channel Deleted");
                    embed.WithDescription($"{Context.User.Mention} has successfully deleted the text channel {(channel.Name)}.");
                    await BE();
                    return;
                }
            }
        }

        [Command("createvoicechannel")] //utility
        [Alias("cvc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateVoiceChannel([Remainder]string name)
        {
            var channel = await Context.Guild.CreateVoiceChannelAsync(name);
            embed.WithTitle("Voice Channel Created");
            embed.WithDescription($"{Context.User.Mention} has successfully created the voice channel #{name}.");
            await BE();
        }

        [Command("deletevoicechannel")] //utility
        [Alias("dvc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildDeleteVoiceChannel([Remainder]string name)
        {
            foreach(var VoiceChannel in Context.Guild.VoiceChannels)
            {
                if(VoiceChannel.Name == name)
                {
                    await VoiceChannel.DeleteAsync();
                    embed.WithTitle("Voice Channel Deleted");
                    embed.WithDescription($"{Context.User.Mention} Successfully deleted the voice channel `{VoiceChannel.Name}`!");
                    await BE();
                }
            }
        }

        [Command("inrole")]
        public async Task InRole(string roleName)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                var guild = Context.Guild;
                var users = Context.Guild.Users;
                var roles = Context.Guild.Roles;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());

                List<string> nameList = new List<string>();

                foreach (var user in users)
                {
                    if (user.Roles.Contains(role as SocketRole))
                    {
                        nameList.Add($"{user.Username}#{user.Discriminator}");
                    }
                }
                string[] output = nameList.ToArray();
                Array.Sort(output);
                int i = nameList.Count();

                var pages = new[]
                {
                    new PaginatedMessage.Page
                    {
                        Title = $"Inrole: Page 1 - Found {i} users with role {role.Name}",
                        Description = String.Join("\n", output.Take(10))
                    },

                    new PaginatedMessage.Page
                    {
                        Title = "Inrole: Page 2",
                        Description = String.Join("\n", output.Skip(10).Take(10))
                    },

                    new PaginatedMessage.Page
                    {
                        Title = "Inrole: Page 3",
                        Description = String.Join("\n", output.Skip(20).Take(10))
                    },

                    new PaginatedMessage.Page
                    {
                        Title = "Inrole: Page 4",
                        Description = String.Join("\n", output.Skip(30).Take(10))
                    },

                    new PaginatedMessage.Page
                    {
                        Title = "Inrole: Page 5",
                        Description = String.Join("\n", output.Skip(40).Take(10))
                    },

                    new PaginatedMessage.Page
                    {
                        Title = "Inrole: Page 6",
                        Description = String.Join("\n", output.Skip(50).Take(10))
                    },

                    new PaginatedMessage.Page
                    {
                        Title = "Inrole: Page 7",
                        Description = String.Join("\n", output.Skip(60).Take(10))
                    },

                };

                var pager = new PaginatedMessage
                {
                    Pages = pages,
                    Color = new Color(252, 132, 255),
                };

                await PagedReplyAsync(pager, new ReactionList
                {
                    Backward = true,
                    Forward = true,
                });
            }
            catch(NullReferenceException)
            {
                stopWatch.Stop();
                await GlobalCommandResponses.CreateCommandError(
                    Context, stopWatch.ElapsedMilliseconds, 
                    CommandError.ObjectNotFound, 
                    "The role specified does not exist.", 
                    "Error: Inrole", $"{Context.User.Mention} **The role specified could not be found.**");
                return;
            }
            catch(Exception e)
            {
                await GlobalCommandResponses.CreateAutomaticBugReport(
                    "Inrole: Unknown Exception",
                    $"Exception at Utility.cs line 532. Exception: {e.Message}");
            }
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
