using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Core.Server_Files;
using Discord;
using Kaguya.Modules.osu;
using System.Diagnostics;
using System.Timers;

namespace Kaguya.Core.Command_Handler
{
    public class Timers
    {
        readonly DiscordSocketClient _client = Global.Client;
        readonly public IServiceProvider _services;
        readonly Logger logger = new Logger();

        public Task GameTimer()
        {
            Timer timer = new Timer(600000); //10 minutes
            timer.Elapsed += Game_Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        int displayIndex = 0;

        private void Game_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string[] games = { "Support Server: yhcNC97", "$help | @Kaguya#2708 help",
            $"Currently servicing {_client.Guilds.Count()} guilds", $"Currently servicing {UserAccounts.UserAccounts.GetAllAccounts().Count().ToString("N0")} members" };
            displayIndex++;
            if (displayIndex >= games.Length)
            {
                displayIndex = 0;
            }

            _client.SetGameAsync(games[displayIndex]);
            logger.ConsoleTimerElapsed($"Game updated to \"{games[displayIndex]}\"");
        }

        public Task CheckChannelPermissions()
        {
            Timer timer = new Timer(1800000); //30 minutes
            timer.Elapsed += Check_Channel_Permissions_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        private void Check_Channel_Permissions_Elapsed(object sender, ElapsedEventArgs e)
        {
            var kID = ulong.TryParse(Config.bot.botUserID, out ulong ID);
            SocketUser kaguya = _client.GetUser(ID);
            if (kaguya != null)
            {
                var servers = Servers.GetAllServers();
                foreach (var server in servers.ToList())
                {
                    var id = server.ID;
                    var guild = _client.GetGuild(id);
                    if (guild == null) //If the server returns null, delete it from the database.
                    {
                        logger.ConsoleCriticalAdvisory($"Guild returned null for {server.ID} [REMOVING!!], Timers.cs line 109.");
                        Servers.RemoveServer(server.ID);
                        continue;
                    }

                    foreach (SocketTextChannel channel in guild.TextChannels)
                    {
                        if (!channel.GetPermissionOverwrite(kaguya).HasValue && server.IsBlacklisted == false)
                        {
                            try
                            {
                                channel.AddPermissionOverwriteAsync(kaguya, OverwritePermissions.AllowAll(channel));
                                logger.ConsoleGuildAdvisory(guild, channel, $"Kaguya has been granted permissions for channel #{channel.Name}");
                            }
                            catch(Exception exception)
                            {
                                logger.ConsoleStatusAdvisory($"Could not overwrite permissions for #{channel.Name} in guild \"{channel.Guild.Name}\"");
                                logger.ConsoleCriticalAdvisory(exception, $"Guild {guild.Name} has been blacklisted.");
                                server.IsBlacklisted = true;
                            }
                        }
                    }
                    continue;
                }
            }
            else
            {
                return;
            }
        }

        public Task ServerInformationUpdate()
        {
            Timer timer = new Timer(86400000); //24 hours
            timer.Elapsed += Server_Information_Update_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            return Task.CompletedTask;
        }

        private void Server_Information_Update_Elapsed(object sender, ElapsedEventArgs e) //Updates every server's name (in case it changes) every 24 hours.
        {
            var servers = Servers.GetAllServers();
            int i = 0;
            foreach (var server in servers.ToList())
            {
                var guild = _client.GetGuild(server.ID); //If the server returns null, delete it from the database.
                if (guild == null)
                {
                    logger.ConsoleCriticalAdvisory($"Guild returned null for {server.ID} [REMOVING!!], Timers.cs line 109."); 
                    Servers.RemoveServer(server.ID);
                    continue;
                }
                server.ServerName = guild.Name;
                Servers.SaveServers();
                i++;
            }

            logger.ConsoleStatusAdvisory($"Updated server names for {i} guilds.");
        }
    }
}
