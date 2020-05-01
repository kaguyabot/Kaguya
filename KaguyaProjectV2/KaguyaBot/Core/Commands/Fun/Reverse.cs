using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using Microsoft.VisualBasic;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class Reverse : KaguyaBase
    {
        [FunCommand]
        [Command("Reverse")]
        [Summary("Takes the text you provide and returns it's reversed value.")]
        [Remarks("<text>\nracecar")]
        public async Task Command([Remainder]string text)
        {
            char[] chars = text.ToCharArray();
            Array.Reverse(chars);
            
            string reversedText = new string(chars);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"{Context.User.Mention} Here's your reversed text!",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Reversed Text",
                        Value = reversedText
                    }
                }
            };

            // ReSharper disable once PossibleNullReferenceException
            if (embed.Description.Length + embed.Fields[0].Value.ToString().Length > 1800)
            {
                await SendBasicErrorEmbedAsync("Sorry, you're at the character limit! Please try something shorter.");
            }
            
            await SendEmbedAsync(embed);
        }
    }
}