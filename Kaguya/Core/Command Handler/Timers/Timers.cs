using Discord;
using Discord.WebSocket;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace Kaguya.Core.Command_Handler
{
    public class Timers
    {
        readonly DiscordShardedClient _client = Global.client;
        readonly public IServiceProvider _services;
        readonly Logger logger = new Logger();
        KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        private bool AntiRaidActive(SocketGuild guild)
        {
            return Servers.GetServer(guild).AntiRaidList.Count > 0;
        }

        public Task SupporterExpirationTimer(DiscordSocketClient client) //Checks supporter expiration times
        {
            if (SupporterExpTimersActive < 1)
            {
                Timer timer = new Timer(1800000); //30 seconds
                timer.Enabled = true;
                timer.Elapsed += Supporter_Expiration_Timer_Elapsed;
                timer.AutoReset = true;
                SupporterExpTimersActive++;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private int SupporterExpTimersActive = 0;

        private async void Supporter_Expiration_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var guild = _client.GetGuild(546880579057221644);
            var role = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "supporter");
            foreach (var account in UserAccounts.UserAccounts.GetAllAccounts())
            {
                var difference = DateTime.Now - account.KaguyaSupporterExpiration;
                if (difference.TotalMinutes < 30 && !account.IsSupporter) //If the supporter tag has expired within 30 minutes
                {
                    try
                    {
                        var user = guild.GetUser(account.ID);
                        await user.RemoveRoleAsync(role); 

                        embed.WithTitle("Kaguya Supporter Status");
                        embed.WithDescription($"⚠ **Your Kaguya supporter tag has expired!** ⚠" +
                            $"\n" +
                            $"\nTo renew your supporter tag and keep your benefits, " +
                            $"please visit the following link: <https://stageosu.selly.store/>" +
                            $"\n" +
                            $"\nWe hope to see you again soon, and thanks for your support of the Kaguya Project! <:norilove:543371982855602186>");
                        embed.SetColor(EmbedColor.RED);

                        var dmChannel = await user.GetOrCreateDMChannelAsync();
                        await dmChannel.SendMessageAsync(embed: embed.Build());

                        logger.ConsoleStatusAdvisory($"{account.Username}'s supporter role has been removed (if they had one) and they have been notified.");
                    }
                    catch (Exception ex) //Continue here
                    {
                        logger.ConsoleCriticalAdvisory(ex, $"Exception occurred when removing supporter role from user. {account}");
                    }
                }
            }
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

            switch (server.AntiRaidPunishment.ToLower())
            {
                case "mute":
                    string mutedUsers = "";
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
                                logger.ConsoleGuildAdvisory(user.Guild, user, $"Kaguya Anti-Raid: User muted.");
                            }
                            else
                            { server.AntiRaidList.Clear(); }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to mute user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notMutedUsers += $"\n`{user.ToString()}` - ID: `{user.Id}`";
                            continue;
                        }
                    }

                    if (notMutedUsers != "")
                    {
                        embed.WithTitle("⚠️ Kaguya Anti-Raid Advisory: FAILED TO MUTE USERS!! ⚠️");
                        embed.WithDescription(notMutedUsers);
                        embed.WithFooter("Ensure that my role is at the top of the heirarchy and that there is a \"kaguya-mute\" role!");
                        embed.SetColor(EmbedColor.RED);
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithTitle("Kaguya Anti-Raid: Users Muted");
                        embed.WithDescription(mutedUsers);
                        embed.SetColor(EmbedColor.VIOLET);
                        embed.WithThumbnailUrl("https://i.imgur.com/6kBEiug.png");
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }

                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                case "kick":
                    string kickedUsers = "";
                    string notKickedUsers = "";

                    foreach (var userID in userIDs)
                    {
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await user.KickAsync(); //Applies punishment.
                                kickedUsers += $"\n`{user}` - ID: `{user.Id}`";
                                logger.ConsoleGuildAdvisory(user.Guild, user, $"Kaguya Anti-Raid: User kicked.");
                            }
                            else
                            { server.AntiRaidList.Clear(); }
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
                    {
                        embed.WithTitle("⚠️ Kaguya Anti-Raid Advisory: FAILED TO KICK USERS!! ⚠️");
                        embed.WithDescription(notKickedUsers);
                        embed.WithFooter("Ensure that my role is at the top of the heirarchy and that there is a \"kaguya-mute\" role!");
                        embed.SetColor(EmbedColor.RED);
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithTitle("Kaguya Anti-Raid: Users Muted");
                        embed.WithDescription(kickedUsers);
                        embed.SetColor(EmbedColor.VIOLET);
                        embed.WithThumbnailUrl("https://i.imgur.com/6kBEiug.png");
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                case "shadowban":
                    string shadowbannedUsers = "";
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
                                logger.ConsoleGuildAdvisory(user.Guild, user, $"Kaguya Anti-Raid: User shadowbanned.");
                            }
                            else
                            { server.AntiRaidList.Clear(); }
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
                    {
                        embed.WithTitle("⚠️ Kaguya Anti-Raid Advisory: FAILED TO SHADOWBAN USERS!! ⚠️");
                        embed.WithDescription(shadowbannedUsers);
                        embed.WithFooter("Ensure that my role is at the top of the heirarchy and that there is a \"kaguya-mute\" role!");
                        embed.SetColor(EmbedColor.RED);
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithTitle("Kaguya Anti-Raid: Users Shadowbanned");
                        embed.WithDescription(shadowbannedUsers);
                        embed.SetColor(EmbedColor.VIOLET);
                        embed.WithThumbnailUrl("https://i.imgur.com/6kBEiug.png");
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                case "ban":
                    string bannedUsers = "";
                    string notBannedUsers = "";

                    foreach (var userID in userIDs)
                    {
                        try
                        {
                            if (server.AntiRaidList.Count >= server.AntiRaidCount)
                            {
                                await guild.AddBanAsync(user); //Applies punishment.
                                bannedUsers += $"\n`{user}` - ID: `{user.Id}`";
                                logger.ConsoleGuildAdvisory(user.Guild, user, $"Kaguya Anti-Raid: User banned.");
                            }
                            else
                            { server.AntiRaidList.Clear(); }
                        }
                        catch (Exception ex)
                        {
                            logger.ConsoleCriticalAdvisory($"Kaguya Anti-Raid Advisory: Failed to ban user in guild {guild.Name}!" +
                                $"\nException: {ex.Message}");
                            notBannedUsers += $"\n⚠️ Kaguya Anti-Raid Advisory: FAILED TO BAN USERS!! ⚠️" +
                                $"\n`{user}` - ID: `{user.Id}`";
                            continue;
                        }
                    }

                    if (notBannedUsers != "")
                    {
                        embed.WithTitle("⚠️ Kaguya Anti-Raid Advisory: FAILED TO BAN USERS!! ⚠️");
                        embed.WithDescription(notBannedUsers);
                        embed.WithFooter("Ensure that my role is at the top of the heirarchy and that there is a \"kaguya-mute\" role!");
                        embed.SetColor(EmbedColor.RED);
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        embed.WithTitle("Kaguya Anti-Raid: Users Shadowbanned");
                        embed.WithDescription(bannedUsers);
                        embed.SetColor(EmbedColor.VIOLET);
                        embed.WithThumbnailUrl("https://i.imgur.com/6kBEiug.png");
                        await (_client.GetChannel(server.LogAntiRaids) as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
                    }
                    server.AntiRaidList.Clear();
                    Servers.SaveServers();
                    break;
                default:
                    break;
            }
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
            CopyDirectory($@"{executingDirectory}/Resources", $"{desktop}/Resources Backup");
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
