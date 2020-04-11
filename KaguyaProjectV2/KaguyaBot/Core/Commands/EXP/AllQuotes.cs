using System.Text;
using System.Linq;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.IO;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class AllQuotes : KaguyaBase
    {
        [ExpCommand]
        [Command("AllQuotes")]
        [Alias("quotes", "listquotes")]
        [Summary("Replies with all quotes that the server has added. If the " + 
                 "total amount of quotes exceeds 5, all quotes will be consolidated " + 
                 "into a text file and DM'd to the command executor. Otherwise, they will " + 
                 "be displayed in chat.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var quotes = server.Quotes;
            var quoteCount = quotes.Count();

            if(quoteCount == 0)
            {
                await SendBasicErrorEmbedAsync($"This server's quote collection is empty.\nCreate some with `{server.CommandPrefix}addquote`.");
                return;
            }

            if(quoteCount <= 5)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Quotes for {Context.Guild}"
                };

                foreach(var quote in quotes)
                {
                    embed.AddField($"Quote #{quote.Id}", QuoteString(quote));
                }

                await SendEmbedAsync(embed);
                return;
            }

            var n = DateTime.Now;
            using(var s = new MemoryStream())
            {
                var sw = new StreamWriter(s);
                await sw.WriteLineAsync($"### Quotes for: {Context.Guild.Name} ###");
                foreach(var quote in quotes)
                {
                    await sw.WriteLineAsync(QuoteString(quote, true));
                }

                await sw.FlushAsync();
                s.Seek(0, SeekOrigin.Begin);

                try
                {
                    await Context.User.SendFileAsync(s, $"Quotes_{Context.Guild.Name}-{n.Day}-{n.Month}-{n.Year}--{n.Hour:00}-{n.Minute:00}-{n.Second:00}.txt");
                }
                catch(Exception)
                {
                    throw new KaguyaSupportException("Failed to DM you the quotes. Do you allow DMs from me/bots?");
                }
            }

            await SendBasicSuccessEmbedAsync("All quotes have been sent to your DM.");
        }

        private string QuoteString(Quote quote, bool toFile = false)
        {
            var quoteDate = DateTime.FromOADate(quote.TimeStamp);
            if(!toFile)
            {
                var qSb = new StringBuilder();
                qSb.AppendLine($"Quote: `{quote.Text}`");
                qSb.AppendLine($"Quoted by: `{Client.GetUser(quote.UserId).ToString() ?? quote.UserId.ToString()}`");
                qSb.AppendLine($"Time: `{quoteDate.ToLongDateString()} at {quoteDate.ToLongTimeString()} (EST)`");
                return qSb.ToString();
            }

            var qSb2 = new StringBuilder();
            qSb2.AppendLine($"Id: {quote.Id}");
            qSb2.AppendLine($"Quote: {quote.Text}");
            qSb2.AppendLine($"Quoted by: {Client.GetUser(quote.UserId).ToString() ?? quote.UserId.ToString()}");
            qSb2.AppendLine($"Time: {quoteDate.ToLongDateString()} at {quoteDate.ToLongTimeString()} (EST)");

            return qSb2.ToString();
        }
    }
}