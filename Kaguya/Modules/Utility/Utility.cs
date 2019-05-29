using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Utility
{
    public class Utility : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        //public InteractiveService _interactive = new InteractiveService(Global.Client);
        Logger logger = new Logger();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("toggleannouncements")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ToggleAnnouncements()
        {
            Server guild = Servers.GetServer(Context.Guild);
            var cmdPrefix = guild.commandPrefix;
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
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            var server = Servers.GetServer(Context.Guild);
            var oldPrefix = server.commandPrefix;

            if(prefix.Length > 3)
            {
                embed.WithTitle("Change Command Prefix: Failure!");
                embed.WithDescription("The chosen prefix is too long! Please select a combination of less than 4 characters/symbols ");
                embed.WithFooter($"To reset the command prefix, type {cmdPrefix}prefix!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                // logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "Invalid prefix."); ERROR HANDLER HERE
                return;
            }

            server.commandPrefix = prefix;
            Servers.SaveServers();

            embed.WithTitle("Change Command Prefix: Success!");
            embed.WithDescription($"The command prefix has been changed from `{oldPrefix}` to `{server.commandPrefix}`.");
            embed.WithFooter($"If you ever forget the prefix, tag me and type \"`prefix`\"!");
            await BE();
        }
        
        [Command("author")] //utility
        public async Task Author()
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

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
