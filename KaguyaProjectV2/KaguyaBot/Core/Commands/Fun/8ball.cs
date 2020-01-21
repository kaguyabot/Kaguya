using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class MagicEightBall : ModuleBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("8ball")]
        [Summary("Ask a yes or no question and Kaguya will use her divine powers to " +
                 "answer your question with perfect accuracy!")]
        [Remarks("<question>")]
        public async Task Command([Remainder]string question)
        {
            var responses = await DatabaseQueries.GetAllAsync<EightBall>();

            Random r = new Random();
            int val = r.Next(responses.Count);

            var response = responses[val];

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Magic 8Ball | {new Emoji("🔮")}",
                Description = response.Response
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}
