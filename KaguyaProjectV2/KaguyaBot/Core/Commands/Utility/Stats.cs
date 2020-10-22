using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;

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
            Context.Channel.EnterTypingState();

            var curProcess = Process.GetCurrentProcess();

            DiscordShardedClient client = Client;
            SocketUser owner = client.GetUser(ConfigProperties.BotConfig.BotOwnerId);

            DiscordSocketClient curShard = client.GetShardFor(Context.Guild);

            int curTextChannels = 0;
            int curVoiceChannels = 0;
            int curOnline = 0;

            foreach (SocketGuild guild in curShard.Guilds)
            {
                curTextChannels += guild.TextChannels.Count;
                curVoiceChannels += guild.VoiceChannels.Count;
                curOnline += guild.Users.Count(x => x.Status != UserStatus.Offline);
            }

            int totalGuilds = 0;
            int totalTextChannels = 0;
            int totalVoiceChannels = 0;

            foreach (DiscordSocketClient shard in client.Shards)
            {
                totalGuilds += shard.Guilds.Count;

                foreach (SocketGuild guild in shard.Guilds)
                {
                    totalTextChannels += guild.TextChannels.Count;
                    totalVoiceChannels += guild.VoiceChannels.Count;
                }
            }

            List<CommandHistory> cmdsLastDay = await DatabaseQueries.GetAllAsync<CommandHistory>(h =>
                h.Timestamp >= DateTime.Now.AddHours(-24));

            Dictionary<string, int> mostPopCommand = MemoryCache.MostPopularCommandCache;

            string mostPopCommandName = mostPopCommand?.Keys.First();
            string mostPopCommandCount = mostPopCommand?.Values.First().ToString("N0");

            string mostPopCommandText = "";

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
                    Value = $"Commands Run (Last 24 Hours): `{cmdsLastDay.Count:N0}`\n" +
                            $"Commands Run (All-time): `{MemoryCache.AllTimeCommandCount:N0}`\n" +
                            $"Most Popular Command: `{mostPopCommandText}`"
                },
                new EmbedFieldBuilder
                {
                    Name = "Shard Stats",
                    Value = $"Current Shard: `{curShard.ShardId:N0} / {client.Shards.Count:N0}`\n" +
                            $"Guilds: `{curShard.Guilds.Count:N0}`\n" +
                            $"Text Channels: `{curTextChannels:N0}`\n" +
                            $"Voice Channels: `{curVoiceChannels:N0}`\n" +
                            $"Total Users: `{client.TotalUsersForShard(curShard.ShardId):N0}`\n" +
                            $"Online Users: `{curOnline:N0}`\n" +
                            $"Latency: `{curShard.Latency:N0}ms`\n"
                },
                new EmbedFieldBuilder
                {
                    Name = "Global Stats",
                    Value = $"Uptime: `{(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(4, minUnit: TimeUnit.Second)}`\n" +
                            $"Guilds: `{totalGuilds:N0}`\n" +
                            $"Text Channels: `{totalTextChannels:N0}`\n" +
                            $"Voice Channels: `{totalVoiceChannels:N0}`\n" +
                            $"Users: `{client.TotalUsers():N0}`\n" +
                            $"RAM Usage: `{(double) curProcess.PrivateMemorySize64 / 1000000:N2} Megabytes`\n" +
                            $"Current Version: `{ConfigProperties.Version}`"
                },
                new EmbedFieldBuilder
                {
                    Name = "Kaguya User Stats",
                    Value = $"Unique Interactions (Users): `{await DatabaseQueries.GetCountAsync<User>():N0}`\n" +
                            $"Total Points in Circulation: `{DatabaseQueries.GetTotalCurrency():N0}`\n" +
                            $"Total Gambles: `{await DatabaseQueries.GetCountAsync<GambleHistory>():N0}`"
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