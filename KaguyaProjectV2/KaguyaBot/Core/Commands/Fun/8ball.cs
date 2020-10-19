using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class MagicEightBall : KaguyaBase
    {
        [FunCommand]
        [Command("8ball")]
        [Summary("Ask a yes or no question and Kaguya will use her divine powers to " +
                 "answer your question with perfect accuracy!")]
        [Remarks("<question>")]
        public async Task Command([Remainder] string question)
        {
            List<EightBall> responses = await DatabaseQueries.GetAllAsync<EightBall>();

            var r = new Random();
            int val = r.Next(responses.Count);

            EightBall response = responses[val];

            var suicidePreventionRegexArray = new[]
            {
                new Regex(@"should i \b(?:kill|end|hang)\b \b(myself|it all)\b"),
                new Regex(@"should i \b(commit|hang)\b \b(?:myself|suicide)\b"),
                new Regex(@"\b(hang|kill|suicide)\b \b(myself)\b"),
                new Regex(@"suicide"),
                new Regex(@"i \b(end)\b \b(it all|my life|myself)\b")
            };

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Magic 8Ball | {new Emoji("🔮")}",
                Description = response.Response
            };

            if (suicidePreventionRegexArray.Any(x => x.IsMatch(question.ToLower())))
            {
                embed = new KaguyaEmbedBuilder(EmbedColor.VIOLET)
                {
                    Title = $"Magic 8Ball | {new Emoji("🔮")}",
                    Description = $"Hey, friend. I'm not really sure how to answer this " +
                                  $"one. These resources may be able to help you figure this out:\n\n" +
                                  $"[[USA Suicide Prevention Hotline: 1-800-273-8255]](https://suicidepreventionlifeline.org/)\n" +
                                  $"[[USA Suicide Prevention Website]](https://suicidepreventionlifeline.org/)\n" +
                                  $"[[International Suicide Prevention Website]](https://www.supportisp.org/)"
                };
            }

            await SendEmbedAsync(embed);
        }
    }
}