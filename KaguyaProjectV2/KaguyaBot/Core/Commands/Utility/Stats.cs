using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class Stats : KaguyaBase
    {
        [UtilityCommand]
        [Command("Stats")]
        [Summary("Returns a set of pretty interesting stats!")]
        [Remarks("")]
        public async Task Command()
        {
            using (Context.Channel.EnterTypingState())
            {
                DiscordShardedClient client = Client;
                SocketUser owner = client.GetUser(ConfigProperties.BotConfig.BotOwnerId);

                KaguyaStatistics stats = MemoryCache.MostRecentStats;
                
                int totalGuilds = stats.Guilds;
                int totalTextChannels = stats.TextChannels;
                int totalVoiceChannels = stats.VoiceChannels;

                Dictionary<string, int> mostPopCommand = MemoryCache.MostPopularCommandCache;

                string mostPopCommandName = mostPopCommand?.Keys.First();
                string mostPopCommandCount = mostPopCommand?.Values.First().ToString("N0");

                string mostPopCommandText;
                if (mostPopCommandName == null || String.IsNullOrWhiteSpace(mostPopCommandCount))
                    mostPopCommandText = "Data not loaded into cache yet.";
                else
                    mostPopCommandText = $"{mostPopCommandName} with {int.Parse(mostPopCommandCount.Replace(",", "")):N0} uses.";

                var fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Author",
                        Value = $"User: `{owner}`\n" +
                                $"Id: `{owner.Id}`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Command Stats",
                        Value = $"Commands Run (Last 24 Hours): `{stats.CommandsLast24Hours:N0}`\n" +
                                $"Commands Run (All-time): `{stats.Commands:N0}`\n" +
                                $"Most Popular Command: `{mostPopCommandText}`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Global Stats",
                        Value = $"Uptime: `{(DateTime.Now - DateTime.Now.AddSeconds(-stats.UptimeSeconds)).Humanize(4, minUnit: TimeUnit.Second)}`\n" +
                                $"Guilds: `{totalGuilds:N0}`\n" +
                                $"Text Channels: `{totalTextChannels:N0}`\n" +
                                $"Voice Channels: `{totalVoiceChannels:N0}`\n" +
                                $"Users: `{stats.GuildUsers:N0}`\n" +
                                $"RAM Usage: `{stats.RamUsageMegabytes:N2} Megabytes`\n" +
                                $"Current Version: `{stats.Version}`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Kaguya User Stats",
                        Value = $"Unique Interactions (Users): `{stats.KaguyaUsers:N0}`\n" +
                                $"Total Points in Circulation: `{stats.Points:N0}`\n" +
                                $"Total Gambles: `{stats.Gambles:N0}`"
                    }
                };

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Kaguya Statistics",
                    Fields = fields
                };

                embed.SetColor(EmbedColor.GOLD);
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}