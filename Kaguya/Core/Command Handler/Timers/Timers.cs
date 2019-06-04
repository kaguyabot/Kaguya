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
        readonly DiscordShardedClient _client = Global.Client;
        readonly public IServiceProvider _services;
        readonly Logger logger = new Logger();

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
