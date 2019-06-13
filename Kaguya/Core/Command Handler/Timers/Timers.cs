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

        public Task AntiRaidTimer(SocketUser user)
        {
            var server = Servers.GetServer((user as SocketGuildUser).Guild);

            if (server.AntiRaid == true )
            {
                var seconds = server.AntiRaidSeconds;
                var punishment = server.AntiRaidPunishment;
                server.AntiRaidList.Add(user.Id);

                Timer timer = new Timer(seconds);
                timer.Elapsed += (sender, e) => Anti_Raid_Timer_Elapsed(sender, e, server);
                timer.AutoReset = false;
                timer.Enabled = true;
                return Task.CompletedTask;
            }
            else //If anti raid is not enabled for the server.
                return Task.CompletedTask;
        }

        private async Task Anti_Raid_Timer_Elapsed(object sender, ElapsedEventArgs e, Server server)
        {
            var users = server.AntiRaidList;
            var guild = _client.GetGuild(server.ID);


            switch (server.AntiRaidPunishment.ToLower())
            {
                case "mute":
                    string mutedUsers = "Kaguya Anti-Raid Service: Members Muted\n";
                    string notMutedUsers = "";

                    var muteRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                    foreach (var user in users)
                    {
                        var guildUser = _client.GetUser(user);
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await (guildUser as SocketGuildUser).AddRoleAsync(muteRole); //Applies punishment.
                                mutedUsers += $"\n`{guildUser.ToString()}` - ID: `{guildUser.Id}`";
                            }
                            else
                            { return; }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to mute user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notMutedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO MUTE USERS!! ⚠️" +
                                $"\n`{guildUser.ToString()}` - ID: `{guildUser.Id}`" +
                                $"\nException: `{ex.Message}`";
                            continue;
                        }
                    }

                    if (notMutedUsers != "")
                        await guild.Owner.SendMessageAsync(notMutedUsers); //DMs the owner of the server if something really bad happens.
                    else
                        await guild.Owner.SendMessageAsync(mutedUsers);
                    break;
                case "kick":
                case "shadowban":
                case "ban":
                    string bannedUsers = "Kaguya Anti-Raid Service: Members Muted\n";
                    string notBannedUsers = "";

                    foreach (var user in users)
                    {
                        var guildUser = _client.GetUser(user);

                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await guild.AddBanAsync(guildUser); //Applies punishment.
                                bannedUsers += $"\n`{guildUser.ToString()}` - ID: `{guildUser.Id}`";
                            }
                            else
                            { return; }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to ban user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notBannedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO BAN USERS!! ⚠️" +
                                $"\n`{guildUser.ToString()}` - ID: `{guildUser.Id}`" +
                                $"\nException: `{ex.Message}`";
                            continue;
                        }
                    }

                    if (notBannedUsers != "")
                        await guild.Owner.SendMessageAsync(notBannedUsers); //DMs the owner of the server if something really bad happens.
                    else
                        await guild.Owner.SendMessageAsync(bannedUsers);
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
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        private void Servers_Cleanup_Elapsed(object sender, ElapsedEventArgs e)
        {
            var servers = _client.Guilds;
            var serverFile = Servers.GetAllServers();
            double i = 0;

            foreach (var guild in serverFile)
            {
                guild.ServerName = "This is a test name.";
            }

            foreach (var server in servers)
            {
                try
                {
                    Servers.GetServer(server).ServerName = server.Name;
                    i++;
                    Console.WriteLine($"{(i / servers.Count).ToString("N3")}% complete.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed. Exception: {ex.Message}");
                    i--;
                    continue;
                }
            }

            foreach (var guild in serverFile.ToArray())
            {
                if (guild.ServerName == "This is a test name.")
                {
                    serverFile.Remove(guild);
                }
            }

            logger.ConsoleStatusAdvisory($"Timer Elapsed: Removed {i} guilds from the database because I am no longer connected to them!");

            Servers.SaveServers();
        }

        public Task VoteClaimRateLimitTimer(DiscordSocketClient _client)
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

        public Task MessageCacheTimer(DiscordSocketClient _client)
        {
            Timer timer = new Timer(2000); //2 Seconds
            timer.Elapsed += Message_Cache_Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        private void Message_Cache_Timer_Elapsed(object sender, ElapsedEventArgs e) //Saves the log every 2 seconds.
        {
            ServerMessageLogs.SaveServerLogging();
        }

        public Task GameTimer(DiscordSocketClient _client)
        {
            Timer timer = new Timer(300000); //5 minutes
            timer.Elapsed += Game_Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        int displayIndex = 0;

        private void Game_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var botID = ulong.TryParse(Config.bot.BotUserID, out ulong ID);
            var mutualGuilds = _client.GetUser(ID).MutualGuilds;

            int i = 0;
            foreach (var guild in mutualGuilds)
            {
                for (int j = 0; j <= guild.MemberCount; j++)
                {
                    i++;
                }
            }

            string[] games = { "Support Server: aumCJhr", "$help | @Kaguya#2708 help",
            $"Servicing {mutualGuilds.Count().ToString("N0")} guilds", $"Serving {i.ToString("N0")} users" };
            displayIndex++;
            if (displayIndex >= games.Length)
            {
                displayIndex = 0;
            }

            _client.SetGameAsync(games[displayIndex]);
            logger.ConsoleTimerElapsed($"Game updated to \"{games[displayIndex]}\"");
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
            Timer timer = new Timer(300000); //Every 5 minutes, backup the resources folder.
            timer.Elapsed += Resources_Backup_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
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
