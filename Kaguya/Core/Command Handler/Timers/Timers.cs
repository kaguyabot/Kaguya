using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Core.Server_Files;
using Discord;
using System.Diagnostics;
using System.Timers;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace Kaguya.Core.Command_Handler
{
    public class Timers
    {
        readonly DiscordShardedClient _client = Global.client;
        readonly public IServiceProvider _services;
        readonly Logger logger = new Logger();

        private bool AntiRaidActive(SocketGuild guild)
        {
            return Servers.GetServer(guild).AntiRaidList.Count > 0;
        }

        public Task AntiRaidTimer(SocketGuildUser user)
        {
            #pragma warning disable //Can't await Anti_Raid_Timer_Elapsed
           
            var server = Servers.GetServer((user as SocketGuildUser).Guild);

            if (server.AntiRaid == true)
            {
                var seconds = server.AntiRaidSeconds;
                var punishment = server.AntiRaidPunishment;
                server.AntiRaidList.Add(user.Id);
                Servers.SaveServers();

                if (!AntiRaidActive((user as SocketGuildUser).Guild))
                    return Task.CompletedTask;

                Timer timer = new Timer(seconds * 1000);
                timer.Elapsed += (sender, e) => Anti_Raid_Timer_Elapsed(sender, e, server, user);
                timer.AutoReset = false;
                timer.Enabled = true;

                return Task.CompletedTask;
            }
            else //If anti raid is not enabled for the server.
                return Task.CompletedTask;
        }

        private async Task Anti_Raid_Timer_Elapsed(object sender, ElapsedEventArgs e, Server server, SocketGuildUser user)
        {
            var userIDs = server.AntiRaidList;
            var guild = _client.GetGuild(server.ID);
            var roles = guild.Roles;

            Console.WriteLine("Antiraid started.");

            switch (server.AntiRaidPunishment.ToLower())
            {
                case "mute":
                    string mutedUsers = "Kaguya Anti-Raid Service: Members Muted\n";
                    string notMutedUsers = "";

                    var muteRole = roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                    foreach (var userID in userIDs)
                    {
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await user.AddRoleAsync(muteRole); //Applies punishment.
                                mutedUsers += $"\n`{user.ToString()}` - ID: `{user.Id}`";
                            }
                            else
                            { return; }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to mute user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notMutedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO MUTE USERS!! ⚠️" +
                                $"\n`{user.ToString()}` - ID: `{user.Id}`" +
                                $"\nException: `{ex.Message}`";
                            continue;
                        }
                    }

                    if (notMutedUsers != "")
                        await guild.Owner.SendMessageAsync(notMutedUsers); //DMs the owner of the server if something really bad happens.
                    else
                        await guild.Owner.SendMessageAsync(mutedUsers);
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                case "kick":
                    string kickedUsers = "Kaguya Anti-Raid Service: Members Kicked\n";
                    string notKickedUsers = "";

                    foreach (var userID in userIDs)
                    {
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await user.KickAsync(); //Applies punishment.
                                kickedUsers += $"\n`{user}` - ID: `{user.Id}`";
                            }
                            else
                            { return; }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to kick user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notKickedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO KICK USERS!! ⚠️" +
                                $"\n`{user}` - ID: `{user.Id}`";
                            continue;
                        }
                    }

                    if (notKickedUsers != "")
                        await guild.Owner.SendMessageAsync(notKickedUsers); //DMs the owner of the server if something really bad happens.
                    else
                        await guild.Owner.SendMessageAsync(kickedUsers);
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                case "shadowban":
                    string shadowbannedUsers = "Kaguya Anti-Raid Service: Members Shadowbanned\n";
                    string notShadowbannedUsers = "";

                    foreach (var userID in userIDs)
                    {
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                foreach (var channel in guild.Channels)
                                {
                                    await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.DenyAll(channel)); //Applies punishment.
                                }
                                shadowbannedUsers += $"\n`{user}` - ID: `{user.Id}`";
                            }
                            else
                            { return; }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to shadowban user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notShadowbannedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO SHADOWBAN USERS!! ⚠️" +
                                $"\n`{user}` - ID: `{user.Id}`";
                            continue;
                        }
                    }

                    if (notShadowbannedUsers != "")
                        await guild.Owner.SendMessageAsync(notShadowbannedUsers); //DMs the owner of the server if something really bad happens.
                    else
                        await guild.Owner.SendMessageAsync(shadowbannedUsers);
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                case "ban":
                    string bannedUsers = "Kaguya Anti-Raid Service: Members Banned\n";
                    string notBannedUsers = "";

                    foreach (var userID in userIDs)
                    {
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await guild.AddBanAsync(user); //Applies punishment.
                                bannedUsers += $"\n`{user}` - ID: `{user.Id}`";
                            }
                            else
                            { return; }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to ban user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notBannedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO BAN USERS!! ⚠️" +
                                $"\n`{user}` - ID: `{user.Id}`" +
                                $"\nException: `{ex.Message}`";
                            continue;
                        }
                    }

                    if (notBannedUsers != "")
                        await guild.Owner.SendMessageAsync(notBannedUsers); //DMs the owner of the server if something really bad happens.
                    else
                        await guild.Owner.SendMessageAsync(bannedUsers);
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                default:
                    break;
            }
        }

        public Task ServersCleanup(DiscordSocketClient _client)
        {
            Timer timer = new Timer(3600000); //1 Hour
            timer.Elapsed += Servers_Cleanup_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = false; //Return when confident that it'll work
            return Task.CompletedTask;
        }

        private void Servers_Cleanup_Elapsed(object sender, ElapsedEventArgs e)
        {
            //var servers = _client.Guilds;
            //var serverFile = Servers.GetAllServers();
            //double i = 0;

            //foreach (var guild in serverFile)
            //{
            //    guild.ServerName = "This is a test name.";
            //}

            //foreach (var server in servers)
            //{
            //    try
            //    {
            //        Servers.GetServer(server).ServerName = server.Name;
            //        i++;
            //        Console.WriteLine($"{(i / servers.Count).ToString("N3")}% complete.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Failed. Exception: {ex.Message}");
            //        i--;
            //        continue;
            //    }
            //}

            //foreach (var guild in serverFile.ToArray())
            //{
            //    if (guild.ServerName == "This is a test name.")
            //    {
            //        serverFile.Remove(guild);
            //    }
            //}

            //logger.ConsoleStatusAdvisory($"Timer Elapsed: Removed {i} guilds from the database because I am no longer connected to them!");

            //Servers.SaveServers();
        }

        public Task VoteClaimRateLimitTimer(DiscordSocketClient _client) //Bandaid until I use the webhook
        {
            Timer timer = new Timer(75000); //75 Seconds
            timer.Elapsed += Vote_Claim_Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        private void Vote_Claim_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Config.bot.RecentVoteClaimAttempts = 0;
        }

        int gameTimersActive = 0; //Prevents more than one timer being active at a time, per shard.
        int messageCacheTimersActive = 0;
        int gameRotationTimersActive = 0;
        int resourcesBackupTimersActive = 0;

        public Task MessageCacheTimer(DiscordSocketClient _client)
        {
            if (messageCacheTimersActive < Global.ShardsToLogIn)
            {
                Timer timer = new Timer(3000); //2 Seconds
                timer.Elapsed += Message_Cache_Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                messageCacheTimersActive++;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }


        private void Message_Cache_Timer_Elapsed(object sender, ElapsedEventArgs e) //Saves the log every 2 seconds.
        {
            ServerMessageLogs.SaveServerLogging();
        }

        public Task GameTimer(DiscordSocketClient _client)
        {
            if (gameTimersActive < Global.ShardsToLogIn)
            {
                Timer timer = new Timer(300000); //5 minutes
                timer.Elapsed += Game_Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                gameTimersActive++;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        int displayIndex = 0;

        private void Game_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (gameRotationTimersActive < Global.ShardsToLogIn)
            {
                var botID = ulong.TryParse(Config.bot.BotUserID, out ulong ID);

                string[] games = { "Support Server: aumCJhr", "$help | @Kaguya#2708 help",
            $"Servicing {Global.TotalGuildCount.ToString("N0")} guilds", $"Serving {Global.TotalMemberCount.ToString("N0")} users",
                $"{Utilities.GetAlert("VERSION")}"};
                displayIndex++;
                if (displayIndex >= games.Length)
                {
                    displayIndex = 0;
                }

                _client.SetGameAsync(games[displayIndex]);
                logger.ConsoleTimerElapsed($"Game updated to \"{games[displayIndex]}\"");
            }
        }

        public Task VerifyMessageReceived(DiscordSocketClient _client)
        {
            Timer timer = new Timer(120000); //Every 120 seconds, make sure the bot is seeing messages. If it hasn't seen a message in 120 seconds, restart!
            timer.Elapsed += Verify_Message_Received_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        private void Verify_Message_Received_Elapsed(object sender, ElapsedEventArgs e) //Restarts bot if no messages have been seen for 60 seconds.
        {
            var difference = DateTime.Now - Config.bot.LastSeenMessage;

            if(difference.TotalMilliseconds >= 120000)
            {
                var filePath = Assembly.GetExecutingAssembly().Location;
                Process.Start(filePath);
                Environment.Exit(0);
            }
        }

        public Task ResourcesBackup(DiscordSocketClient _client)
        {
            if (resourcesBackupTimersActive < 1)
            {
                Timer timer = new Timer(300000); //Every 5 minutes, backup the resources folder.
                timer.Elapsed += Resources_Backup_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private void Resources_Backup_Elapsed(object sender, ElapsedEventArgs e)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            CopyDirectory(@$"{executingDirectory}/Resources", $"{desktop}/Resources Backup");
            CopyDirectory($@"{executingDirectory}/Logs", $"{desktop}/Logs Backup");

            logger.ConsoleTimerElapsed($"Backed up Resources and Log files.");
        }

        public static void CopyDirectory(string Src, string Dst)
        {
            String[] Files;

            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files = Directory.GetFileSystemEntries(Src);
            foreach (string Element in Files)
            {
                // Sub directories
                if (Directory.Exists(Element))
                    CopyDirectory(Element, Dst + Path.GetFileName(Element));
                // Files in directory
                else
                    File.Copy(Element, Dst + Path.GetFileName(Element), true);
            }
        }

        
    }
}
