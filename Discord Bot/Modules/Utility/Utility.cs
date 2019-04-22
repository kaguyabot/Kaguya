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
using Kaguya.Core.UserAccounts;
using System.Net;
using System.Timers;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using Discord_Bot.Core;
using System.Diagnostics;

#pragma warning disable

namespace Kaguya.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.token;
        Logger logger = new Logger();
        Stopwatch stopWatch = new Stopwatch();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("modules")] //utility
        [Alias("mdls")]
        public async Task ModulesList()
        {
            stopWatch.Start();
            var server = Servers.GetServer(Context.Guild);
            var cmdPrefix = server.commandPrefix;
            embed.WithTitle("All Kaguya Modules");
            embed.WithDescription($"For all commands in a module, use `{cmdPrefix}commands <ModuleName>`. " +
                $"\nExample: `{cmdPrefix}cmds admin`" +
                $"\n" +
                $"\nAdministration" +
                $"\nCurrency" +
                $"\nEXP" +
                $"\nFun" +
                $"\nHelp" +
                $"\nosu" +
                $"\nUtility");
            embed.WithColor(Pink);
            BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("cmds")] //utility
        [Alias("commands")]
        public async Task ModulesList([Remainder]string category)
        {
            stopWatch.Start();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            if (category.ToLower() == "administration" || category.ToLower() == "admin")
            {
                embed.WithTitle("Module: Administration");
                embed.WithDescription("```css" +
                        "\nAll commands in category: Administration" +
                        "\n" +
                        $"\n{cmdPrefix}ban [b]" +
                        $"\n{cmdPrefix}clear [c] [purge]" +
                        $"\n{cmdPrefix}createrole [cr]" +
                        $"\n{cmdPrefix}deleterole [dr]" +
                        $"\n{cmdPrefix}filteradd [fa]" +
                        $"\n{cmdPrefix}filterclear [clearfilter]" +
                        $"\n{cmdPrefix}filterremove [fr]" +
                        $"\n{cmdPrefix}filterview [fv]" +
                        $"\n{cmdPrefix}kaguyaexit" +
                        $"\n{cmdPrefix}kick [k]" +
                        $"\n{cmdPrefix}logtypes [loglist]" +
                        $"\n{cmdPrefix}massban" +
                        $"\n{cmdPrefix}massblacklist" +
                        $"\n{cmdPrefix}masskick" +
                        $"\n{cmdPrefix}removeallroles [rar]" +
                        $"\n{cmdPrefix}resetlogchannel [rlog]" +
                        $"\n{cmdPrefix}setlogchannel [log]" +
                        $"\n{cmdPrefix}scrapeserver" +
                        $"\n{cmdPrefix}unblacklist" +
                        $"\n" +
                        $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                        "\n```");
                embed.WithColor(Pink);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
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
                    $"\n{cmdPrefix}globalexplb [gexplb]" +
                    $"\n{cmdPrefix}rep" +
                    $"\n{cmdPrefix}repauthor [rep author]" +
                    $"\n{cmdPrefix}serverexplb [explb]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    "\n```");
                embed.WithColor(Pink);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else if (category.ToLower() == "currency")
            {
                embed.WithTitle("Module: Currency");
                embed.WithDescription
                ("```css" +
                "\nAll commands in category: Currency" +
                "\n" +
                $"\n{cmdPrefix}awardeveryone [awardall]" +
                $"\n{cmdPrefix}roll [gr]" +
                $"\n{cmdPrefix}masspointsdistribute" +
                $"\n{cmdPrefix}points" +
                $"\n{cmdPrefix}pointsadd [addpoints]" +
                $"\n{cmdPrefix}timely [t]" +
                $"\n{cmdPrefix}timelyreset" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```"
                );
                embed.WithColor(Pink);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else if (category.ToLower() == "utility")
            {
                embed.WithTitle("Module: Utility");
                embed.WithDescription
                ("```css" +
                "\nAll commands in category: Utility" +
                "\n" +
                $"\n{cmdPrefix}author" +
                $"\n{cmdPrefix}commands [cmds]" +
                $"\n{cmdPrefix}createtextchannel [ctc]" +
                $"\n{cmdPrefix}createvoicechannel [cvc]" +
                $"\n{cmdPrefix}deletetextchannel [dtc]" +
                $"\n{cmdPrefix}deletevoicechannel [dvc]" +
                $"\n{cmdPrefix}prefix" +
                $"\n" +
                $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                $"\n```");
                embed.WithColor(Pink);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
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
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else if (category.ToLower() == "osu")
            {
                embed.WithTitle("Module: osu!");
                embed.WithDescription("```css" +
                    "\n" +
                    $"\n{cmdPrefix}osu" +
                    $"\n{cmdPrefix}createteamrole [ctr]" +
                    $"\n{cmdPrefix}delteams" +
                    $"\n{cmdPrefix}osuset" +
                    $"\n{cmdPrefix}osutop" +
                    $"\n{cmdPrefix}recent [r]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n```");
                embed.WithColor(Pink);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else if (category.ToLower() == "help")
            {
                embed.WithTitle("Module: Help");
                embed.WithDescription("```css" +
                    "\n" +
                    $"\n{cmdPrefix}help [h]" +
                    $"\n{cmdPrefix}helpdm [hdm]" +
                    $"\n" +
                    $"\nType {cmdPrefix}h <command> for more information on a specific command." +
                    $"\n```");
                embed.WithColor(Pink);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
        }

        [Command("toggleannouncements")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ToggleAnnouncements()
        {
            stopWatch.Start();
            Server guild = Servers.GetServer(Context.Guild);
            var cmdPrefix = guild.commandPrefix;
            if (guild.MessageAnnouncements == true)
            {
                guild.MessageAnnouncements = false;

                embed.WithTitle("Level Up Announcements");
                embed.WithDescription($"**{Context.User.Mention} Level up announcements have been disabled.**");
                embed.WithFooter($"To re-enable, use {cmdPrefix}toggleannouncements again.");
                embed.WithColor(Red);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }
            else if(guild.MessageAnnouncements == false)
            {
                guild.MessageAnnouncements = true;

                embed.WithTitle("Level Up Announcements");
                embed.WithDescription($"**{Context.User.Mention} Level up announcements have been enabled.**");
                embed.WithFooter($"To disable, use {cmdPrefix}toggleannouncements again.");
                embed.WithColor(Red);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }
        }

        [Command("prefix")] //utility
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AlterPrefix(string prefix = "$")
        {
            stopWatch.Start();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            var server = Servers.GetServer(Context.Guild);
            var oldPrefix = server.commandPrefix;

            if(prefix.Length > 2)
            {
                embed.WithTitle("Change Command Prefix: Failure!");
                embed.WithDescription("The chosen prefix is too long! Please select a combination of less than 3 characters/symbols ");
                embed.WithFooter($"To reset the command prefix, type {cmdPrefix}prefix!");
                embed.WithColor(Red);
                BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "Invalid prefix."); return;
            }

            server.commandPrefix = prefix;
            Servers.SaveServers();

            embed.WithTitle("Change Command Prefix: Success!");
            embed.WithDescription($"The command prefix has been changed from `{oldPrefix}` to `{server.commandPrefix}`.");
            embed.WithFooter($"If you ever forget the prefix, tag me and type \"`prefix`\"!");
            embed.WithColor(Pink);
            BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }
        
        [Command("author")] //utility
        public async Task Author()
        {
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            var author = UserAccounts.GetAuthor();

            embed.WithTitle("Kaguya Author");
            embed.WithDescription($"Programmed with love by `{author.Username}` uwu");
            embed.WithFooter($"{author.Username} is level {author.LevelNumber} with {author.EXP} EXP and has +{author.Rep} rep!" +
                $"\nTo +rep Stage, type `{cmdPrefix}rep author`!");
            embed.WithColor(Pink);
            BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("createtextchannel")] //utility
        [Alias("ctc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateTextChannel([Remainder]string name)
        {
            stopWatch.Start();
            var channel = await Context.Guild.CreateTextChannelAsync(name);
            embed.WithDescription($"{Context.User.Mention} has successfully created the channel #{name}.");
            embed.WithColor(Pink);
            BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("deletetextchannel")] //utility
        [Alias("dtc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildDeleteTextChannel(SocketGuildChannel channel)
        {
            stopWatch.Start();
            foreach (var Channel in Context.Guild.TextChannels)
            {
                if (Channel == channel)
                {
                    await channel.DeleteAsync();
                    embed.WithDescription($"{Context.User.Mention} has successfully deleted the text channel {(channel.Name)}.");
                    embed.WithColor(Pink);
                    BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
                }
            }
        }

        [Command("createvoicechannel")] //utility
        [Alias("cvc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildCreateVoiceChannel([Remainder]string name)
        {
            stopWatch.Start();
            var channel = await Context.Guild.CreateVoiceChannelAsync(name);
            embed.WithDescription($"{Context.User.Mention} has successfully created the voice channel #{name}.");
            embed.WithColor(Pink);
            BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("deletevoicechannel")] //utility
        [Alias("dvc")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task GuildDeleteVoiceChannel([Remainder]string name)
        {
            stopWatch.Start();
            foreach(var VoiceChannel in Context.Guild.VoiceChannels)
            {
                if(VoiceChannel.Name == name)
                {
                    VoiceChannel.DeleteAsync();
                    embed.WithTitle("Voice Channel Deleted");
                    embed.WithDescription($"{Context.User.Mention} Successfully deleted the voice channel `{VoiceChannel.Name}`!");
                    embed.WithColor(Pink);
                    BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                }
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
