using System.Linq;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.Collections.Generic;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class RandomQuote : KaguyaBase
    {
        [ExpCommand]
        [Command("RandomQuote")]
        [Alias("rq")]
        [Summary("Displays a random quote from the server!")]
        [Remarks("")]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var quotes = server.Quotes;
            var quoteCount = quotes.Count();

            if(quoteCount < 1)
            {
                await SendBasicErrorEmbedAsync("This server does not have any quotes saved.");
                return;
            }

            var rand = new Random();
            var quoteIndex = rand.Next(0, quoteCount);

            var quote = quotes.ElementAt(quoteIndex);
            SocketUser quoteAuthor = Client.GetUser(quote.UserId);

            var embed = new KaguyaEmbedBuilder
            {
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = $"Quote #{quote.Id}",
                        Value = $"{quote.Text}"
                    }
                },
            };

            if(quoteAuthor != null)
            {
                embed.Footer = new EmbedFooterBuilder
                {
                    Text = $"Quoted by: {quoteAuthor}"
                };
            }

            await SendEmbedAsync(embed);
        }
    }
}