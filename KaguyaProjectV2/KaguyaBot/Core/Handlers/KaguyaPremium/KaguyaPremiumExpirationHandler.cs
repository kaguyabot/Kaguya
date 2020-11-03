using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaPremium
{
    public static class KaguyaPremiumExpirationHandler
    {
        private static readonly DiscordShardedClient _client = ConfigProperties.Client;
        private static readonly List<Server> _serverNotificationCache = new List<Server>();
        private static readonly List<PremiumKey> _keysCache = new List<PremiumKey>();

        public static Task Initialize()
        {
            var timer = new Timer
            {
                Interval = 900000, // 15 minutes
                Enabled = true,
                AutoReset = true
            };

            timer.Elapsed += async (s, e) =>
            {
                List<PremiumKey> allUnexpiredKeys = await DatabaseQueries.GetAllAsync<PremiumKey>(x => !x.HasExpired && x.UserId != 0);
                List<Server> premiumServers = await DatabaseQueries.GetAllAsync<Server>(x => x.PremiumExpiration < DateTime.Now.ToOADate());

                foreach (PremiumKey key in allUnexpiredKeys)
                {
                    if (_keysCache.Contains(key))
                        continue;

                    Server serverToCheck = premiumServers.FirstOrDefault(x => x.ServerId == key.ServerId);

                    if (serverToCheck == null || _serverNotificationCache.Contains(serverToCheck))
                        continue;

                    User kaguyaUser = await DatabaseQueries.GetOrCreateUserAsync(key.UserId);

                    SocketUser keyRedeemSocketUser = _client.GetUser(key.UserId);
                    SocketGuild keyGuild = _client.GetGuild(key.ServerId);

                    var descSb = new StringBuilder($"Your [Kaguya Premium]({ConfigProperties.KAGUYA_STORE_URL}) benefits have expired " +
                                                   $"in the server `{keyGuild.Name}`.");

                    if (kaguyaUser.IsPremium)
                    {
                        descSb.AppendLine("\n\nYour personal Kaguya Premium benefits will expire in " +
                                          $"`{DateTime.FromOADate(kaguyaUser.PremiumExpiration).Humanize(false)}`.");
                    }
                    else
                        descSb.AppendLine("\n\nYour personal Kaguya Premium benefits have run out as well.");

                    descSb.AppendLine($"\n___***[Click here to resubscribe for $4.99/month!](https://sellix.io/KaguyaStore)***___");

                    var embed = new KaguyaEmbedBuilder(EmbedColor.ORANGE)
                    {
                        Title = "Kaguya Premium Expiration Notification",
                        Description = descSb.ToString(),
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"You have subscribed for {kaguyaUser.TotalDaysPremium} days. Thank you for your support!"
                        }
                    };

                    try
                    {
                        await keyRedeemSocketUser.SendMessageAsync(embed: embed.Build());
                        _serverNotificationCache.Add(serverToCheck);
                    }
                    catch (Exception ex)
                    {
                        await ConsoleLogger.LogAsync(ex,
                            $"Failed to DM user {keyRedeemSocketUser.UsernameAndDescriminator()} " +
                            $"about a key expiration for guild [{keyGuild.Name} | {keyGuild.Id}]", LogLvl.WARN);
                    }

                    foreach (PremiumKey key2 in allUnexpiredKeys.Where(x => x.ServerId == serverToCheck.ServerId))
                    {
                        key2.HasExpired = true;
                        _keysCache.Add(key2);
                    }

                    await DatabaseQueries.UpdateAsync(_keysCache);
                }

                _serverNotificationCache.Clear();
                _keysCache.Clear();
            };

            return Task.CompletedTask;
        }
    }
}