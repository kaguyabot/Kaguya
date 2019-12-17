using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Humanizer;

namespace KaguyaUptimeAnnouncer.Timers
{
    public class UptimeTimer
    {
        private static bool HasNotified { get; set; } = false;
        public static async Task Start()
        {
            Timer timer = new Timer(2500);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (sender, args) =>
            {
                var client = GlobalProperties.client;
                var kaguya = client.GetUser(146092837723832320);
                var status = kaguya.Status;
                var guild = client.GetGuild(546880579057221644);
                var downtimeChannel = guild.GetTextChannel(581215173034115092);
                var newsRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "warlords");

                var filePath = @"..\..\..\..\..\KaguyaProjectV2\Resources\Logs\UptimeLogs.txt";

                if (status != UserStatus.Invisible || status != UserStatus.Offline)
                {
                    await File.AppendAllTextAsync(filePath, $"Kaguya online. - {DateTime.Now.Humanize()}\n");
                }
                else
                {
                    await File.AppendAllTextAsync(filePath, $"Kaguya offline. - {DateTime.Now.Humanize(false)}\n");
                }

                var content = File.ReadAllLines(filePath);
                var countOnline = content.Count(x => x.Contains("online"));
                var countOffline = content.Count(x => x.Contains("offline"));
                double uptimePercent = (double)countOnline / (countOnline + countOffline);
                //continue with how to find percentage.
                if (status == UserStatus.Offline && HasNotified == false || 
                    status == UserStatus.Invisible && HasNotified == false)
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"Kaguya has gone offline. A new advisory will be posted if " +
                                      $"the bot stays offline for another 30 minutes. I am attempting to " +
                                      $"restart the program, but it may take awhile for Kaguya to connect to " +
                                      $"all shards. I'll post a notification when Kaguya is back online. " +
                                      $"Thank you for your patience.",
                        Color = Color.Red
                    };
                    await downtimeChannel.SendMessageAsync(newsRole?.Mention ?? "", false, embed.Build());

                    HasNotified = true;

                    var kaguyaFilePath = @"..\..\..\..\..\KaguyaProjectV2\bin\Release\netcoreapp3.0\KaguyaProjectV2.exe";

                    ProcessStartInfo info = new ProcessStartInfo(kaguyaFilePath);
                    info.CreateNoWindow = false;

                    Process.Start(info);
                    return;
                }

                if (status != UserStatus.Offline && HasNotified && status != UserStatus.Invisible && HasNotified)
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"Kaguya has come back online! We apologize for any inconvenience. " +
                                      $"Lifetime uptime history: `{uptimePercent}`",
                        Color = Color.Gold
                    };

                    await downtimeChannel.SendMessageAsync(newsRole?.Mention ?? "", false, embed.Build());
                    HasNotified = false;
                }
            };
        }

        public async Task ResetNotification() //Allows a new notification to be sent every 30 minutes.
        {
            Timer timer = new Timer(1800000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += (sender, args) => { HasNotified = false; };
        }
    }
}
