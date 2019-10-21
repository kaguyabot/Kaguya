using Discord;
using Discord.WebSocket;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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

        private int ProcessRAMTimersActive = 0;
        private int RateLimitTimersActive = 0;
        private int SupporterExpTimersActive = 0;
        private int gameTimersActive = 0; //Prevents more than one timer being active at a time, per shard.
        private int messageCacheTimersActive = 0;
        private int gameRotationTimersActive = 0;
        private int resourcesBackupTimersActive = 0;
        private int messageReceivedTimersActive = 0;

        public Task RemindTimer(DiscordSocketClient client)
        {
            Timer timer = new Timer(5000); //5.00 seconds
            timer.Enabled = true;
            timer.Elapsed += (sender, e) => Remind_Timer_Elapsed(sender, e, client);
            return Task.CompletedTask;
        }

        private async void Remind_Timer_Elapsed(object sender, ElapsedEventArgs e, DiscordSocketClient client)
        {
            var users = UserAccounts.UserAccounts.GetAllAccounts();

            foreach (var user in users)
            {
                if (user.Reminders != null)
                {
                    foreach (var item in user.Reminders.ToList())
                    {
                        var remindTime = item.Values.FirstOrDefault();
                        if (DateTime.Now.ToOADate() > remindTime)
                        {
                            var socketUser = client.GetUser(user.ID);

                            try
                            {
                                embed.WithTitle("⚠ Kaguya Reminder");
                                embed.WithDescription($"`{item.Keys.FirstOrDefault()}`");
                                embed.SetColor(EmbedColor.BLUE);

                                await socketUser.SendMessageAsync(embed: embed.Build());
                            }
                            catch (NullReferenceException ex)
                            {
                                user.Reminders.Remove(item);
                                logger.ConsoleCriticalAdvisory(ex, $"User {socketUser} requested a reminder, but their DMs are disabled, " +
                                    $"meaning I cannot send them their reminder.");
                                break;
                            }

                            logger.ConsoleInformationAdvisory($"User {socketUser} has been successfully reminded to " +
                                    $"\"{item.Keys.FirstOrDefault()}\"");
                            user.Reminders.Remove(item);
                        }
                    }
                }
            }
        }

        public Task UnMuteTimer(DiscordSocketClient client)
        {
            Timer timer = new Timer(2500); //2.50 seconds
            timer.Enabled = true;
            timer.Elapsed += (sender, e) => UnMute_Timer_Elapsed(sender, e, client);
            return Task.CompletedTask;
        }

        private void UnMute_Timer_Elapsed(object sender, ElapsedEventArgs e, DiscordSocketClient client)
        {
            var guilds = Servers.GetAllServers();

            foreach (var guild in guilds)
            {
                var socketGuild = client.GetGuild(guild.ID);
                var mutedMembers = guild.MutedMembers;

                if(mutedMembers != null && socketGuild != null)
                {
                    try
                    {
                        var muteRole = socketGuild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                        foreach (var member in mutedMembers.ToList())
                        {
                            if (mutedMembers.TryGetValue(member.Key, out double time))
                            {
                                if (DateTime.Now.ToOADate() > time)
                                {
                                    SocketGuildUser user = socketGuild.GetUser(member.Key);

                                    user.RemoveRoleAsync(muteRole); //Removes mute role from user.
                                    mutedMembers.Remove(user.Id); //Removes muted member from the dictionary.

                                    logger.ConsoleTimerElapsed($"User [{user.Username}#{user.Discriminator} | {user.Id}] has been unmuted.");
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    catch(NullReferenceException ex)
                    {
                        logger.ConsoleCriticalAdvisory($"NullReferenceException handled for unmuting a user inside of guild {socketGuild.Name}.");
                    }
                }
            }
        }

        public Task ProcessCPUTimer(DiscordSocketClient client)
        {
            if (ProcessRAMTimersActive < 1)
            {
                Timer timer = new Timer(2000); //20 seconds
                timer.Enabled = true;
                timer.Elapsed += Process_RAM_Timer_Elapsed;
                ProcessRAMTimersActive++;
            }
            return Task.CompletedTask;
        }

        private void Process_RAM_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process kaguya = Process.GetProcessesByName("Kaguya")[0];

            var ram = new PerformanceCounter("Process", "Private Bytes", kaguya.ProcessName, true);

            // Getting first initial values
            ram.NextValue();

            dynamic result = new ExpandoObject();

            // If system has multiple cores, that should be taken into account
            // Returns number of MB consumed by application
            result.RAM = Math.Round(ram.NextValue() / 1024 / 1024, 2);

            Global.ramUsage = result.RAM;
        }


        public Task RateLimitResetTimer(DiscordSocketClient client)
        {
            if(RateLimitTimersActive < 1)
            {
                Timer timer = new Timer(3900); //Milliseconds at which to reset the rate limit (3.90 seconds)
                timer.Enabled = true;
                timer.Elapsed += RateLimit_Reset_Timer_Elapsed;
                RateLimitTimersActive++;
            }
            return Task.CompletedTask;
        }

        private void RateLimit_Reset_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Global.UserAccounts == null)
                return;
            Logger logger = new Logger();
            var accounts = Global.UserAccounts;
            embed.WithTitle($"⚠️ Kaguya Rate Limit Advisory ⚠️");

            foreach(var account in accounts)
            {
                //This checks whether the user has previously been temp blacklisted. If they have,
                //and the expiration is finished, unblacklist them.

                if(account.RatelimitStrikes > 0 && account.RatelimitStrikes < 5 && 
                    (account.TemporaryBlacklistExpiration - DateTime.Now).TotalSeconds < 0 && account.IsBlacklisted)
                {
                    account.IsBlacklisted = false;
                    logger.ConsoleStatusAdvisory($"User {account.Username} " +
                        $"(ID: {account.ID}) has been unblacklisted after being ratelimited.");
                }

                if(account.RatelimitStrikes > 0 && account.RatelimitStrikes < 5 &&
                    (DateTime.Now - account.TemporaryBlacklistExpiration).TotalDays > 14)
                {
                    account.RatelimitStrikes = 0;
                    logger.ConsoleStatusAdvisory($"User {Global.client.GetUser(account.ID).Username} (ID: {account.ID}) has " +
                        $"had their ratelimit strikes reset due to not ratelimiting for the last 14 days.");
                }

                if ((!account.IsSupporter && account.CommandRateLimit >= 3) || 
                    (account.IsSupporter && account.CommandRateLimit >= 5)) //If someone has used at least 3 commands in 4.25 seconds, add a strike.
                {
                    account.RatelimitStrikes++;
                    if(account.RatelimitStrikes == 1)
                    {
                        string timeout = "60 seconds.";
                        embed.WithDescription($"You are being rate limited and have been temporarily blacklisted. " +
                        $"Please slow down with your command usage. You have been blacklisted for {timeout}");
                        account.IsBlacklisted = true;
                        account.TemporaryBlacklistExpiration = DateTime.Now + TimeSpan.FromSeconds(60);
                        Global.client.GetUser(account.ID).SendMessageAsync(embed: embed.Build());
                        // ^ Try to DM them and let them know they're blacklisted ^
                        logger.ConsoleStatusAdvisory($"User {Global.client.GetUser(account.ID).Username} (ID: {account.ID}) has been temporarily blacklisted " +
                            $"for {timeout}");
                    }

                    if(account.RatelimitStrikes == 2)
                    {
                        string timeout = "10 minutes.";
                        embed.WithDescription($"You are being rate limited and have been temporarily blacklisted. " +
                        $"Please slow down with your command usage. You have been blacklisted for {timeout}");
                        account.IsBlacklisted = true;
                        account.TemporaryBlacklistExpiration = DateTime.Now + TimeSpan.FromSeconds(600);
                        Global.client.GetUser(account.ID).SendMessageAsync(embed: embed.Build());
                        // ^ Try to DM them and let them know they're blacklisted ^
                        logger.ConsoleStatusAdvisory($"User {Global.client.GetUser(account.ID).Username} (ID: {account.ID}) has been temporarily blacklisted " +
                            $"for {timeout}");
                    }

                    if (account.RatelimitStrikes == 3)
                    {
                        string timeout = "60 minutes.";
                        embed.WithDescription($"You are being rate limited and have been temporarily blacklisted. " +
                        $"Please slow down with your command usage. You have been blacklisted for {timeout}.");
                        account.IsBlacklisted = true;
                        account.TemporaryBlacklistExpiration = DateTime.Now + TimeSpan.FromSeconds(3600);
                        Global.client.GetUser(account.ID).SendMessageAsync(embed: embed.Build());
                        // ^ Try to DM them and let them know they're blacklisted ^
                        logger.ConsoleStatusAdvisory($"User {Global.client.GetUser(account.ID).Username} (ID: {account.ID}) has been temporarily blacklisted " +
                            $"for {timeout}");
                    }

                    if (account.RatelimitStrikes == 4)
                    {
                        string timeout = "12 hours.";
                        embed.WithDescription($"You are being rate limited and have been temporarily blacklisted. " +
                        $"Please slow down with your command usage. You have been blacklisted for {timeout}." +
                        $"\n**If you continue to breach the rate limit (3 commands within 3.90 seconds), you will be permanently blacklisted.**");
                        account.IsBlacklisted = true;
                        account.TemporaryBlacklistExpiration = DateTime.Now + TimeSpan.FromSeconds(43200);
                        Global.client.GetUser(account.ID).SendMessageAsync(embed: embed.Build());
                        // ^ Try to DM them and let them know they're blacklisted ^
                        logger.ConsoleStatusAdvisory($"User {Global.client.GetUser(account.ID).Username} (ID: {account.ID}) has been temporarily blacklisted " +
                            $"for {timeout}");
                    }

                    if(account.RatelimitStrikes == 5)
                    {
                        embed.WithDescription($"You have been permanently blacklisted due to repetitive breaching of " +
                            $"the rate limit. All points and EXP have been reset to zero. This blacklist will not be lifted.");
                        account.Points = 0;
                        account.EXP = 0;
                        account.TemporaryBlacklistExpiration += TimeSpan.FromDays(900000); //Perma blacklist
                        logger.ConsoleStatusAdvisory($"User {Global.client.GetUser(account.ID).Username} (ID: {account.ID}) has been " +
                            $"permanently blacklisted due to receiving 5 ratelimit strikes.");
                    }
                }
                account.CommandRateLimit = 0;
            }
            UserAccounts.UserAccounts.SaveAccounts();
            Servers.SaveServers();
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
            }
            return Task.CompletedTask;
        }


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

        public Task MessageCacheTimer(DiscordSocketClient _client)
        {
            if (messageCacheTimersActive < 1)
            {
                Timer timer = new Timer(10000); //10 Seconds
                timer.Elapsed += Message_Cache_Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
                messageCacheTimersActive++;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }


        private void Message_Cache_Timer_Elapsed(object sender, ElapsedEventArgs e) //Saves the log every 10 seconds.
        {
            ServerMessageLogs.SaveServerLogging();
        }

        public Task GameTimer(DiscordSocketClient _client)
        {
            if (gameTimersActive < 1)
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

        int index = 0;

        private void Game_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var botID = ulong.TryParse(Config.bot.BotUserID, out ulong ID);

                string[] games = { "Support Server: aumCJhr", "$help | @Kaguya#2708 help",
                $"Servicing {Global.client.Guilds.Count} guilds", $"Serving {Global.TotalMemberCount.ToString("N0")} users",
                $"{Utilities.GetAlert("VERSION")}"};
                index++;
                if (index >= games.Length)
                {
                    index = 0;
                }

                _client.SetGameAsync(games[index]);
                logger.ConsoleTimerElapsed($"Game updated to \"{games[index]}\"");
            }
            catch(Exception ex)
            {
                logger.ConsoleCriticalAdvisory(ex.Message);
            }
        }

        public Task VerifyMessageReceived(DiscordSocketClient _client)
        {
            if (messageReceivedTimersActive < 1)
            {
                messageReceivedTimersActive++;
                Timer timer = new Timer(120000); //Every 120 seconds, make sure the bot is seeing messages. If it hasn't seen a message in 120 seconds, restart!
                timer.Elapsed += Verify_Message_Received_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
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
                resourcesBackupTimersActive++;
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