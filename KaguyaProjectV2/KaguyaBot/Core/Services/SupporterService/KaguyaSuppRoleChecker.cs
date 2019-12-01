using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Services.SupporterService
{
    public static class KaguyaSuppRoleChecker
    {
        public static Task CheckRoleTimer()
        {
            Timer timer = new Timer(1800000); //30 mins
            timer.Start();
            timer.AutoReset = true;
            timer.Elapsed += (sender, e) => 
            {
                var client = Global.ConfigProperties.client;
                var kaguyaSupportServer = client.GetGuild(546880579057221644); //Kaguya Support Discord Server

                var supporters = UtilityQueries.GetAllKeys()
                    .Where(x => x.UserId != 0 && x.Expiration - DateTime.Now.ToOADate() > 0);

                foreach (var element in supporters)
                {
                    SocketUser socketUser = client.GetUser(element.UserId);

                    if (!socketUser.MutualGuilds.Contains(kaguyaSupportServer))
                        continue;

                    var supporterRole = kaguyaSupportServer.Roles.FirstOrDefault(x => x.Name == "Supporter");
                    var kaguyaSuppUser = kaguyaSupportServer.GetUser(socketUser.Id);

                    if (kaguyaSuppUser.Roles.Contains(supporterRole))
                        continue;

                    kaguyaSuppUser.AddRoleAsync(supporterRole);
                    ConsoleLogger.Log($"User {socketUser} has had their supporter role given to them in the Kaguya Support server.", LogLevel.INFO);
                }
            };
            return Task.CompletedTask;
        }
    }
}
