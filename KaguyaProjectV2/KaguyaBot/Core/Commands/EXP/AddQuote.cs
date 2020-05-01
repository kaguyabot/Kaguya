using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class AddQuote : KaguyaBase
    {
        [ExpCommand]
        [Command("AddQuote")]
        [Alias("aq")]
        [Summary("Allows a moderator to add a quote to this server's quote collection.")]
        [Remarks("<text>")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Command([Remainder]string text)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var quotes = server.Quotes;
            int quoteCount = quotes?.Count() ?? 0;

            if(quoteCount > 0)
            {
                if(server.Quotes.Any(x => x.Text.Equals(text)))
                {
                    var cEmbed = new KaguyaEmbedBuilder(EmbedColor.YELLOW)
                    {
                        Description = "A quote with the same text already exists. Do you want to create this one anyway?"
                    };

                    var data = new ReactionCallbackData("", cEmbed.Build(), true, true, TimeSpan.FromSeconds(120));
                    data.AddCallBack(GlobalProperties.CheckMarkEmoji(), async (c, r) => 
                    {
                        await InsertQuote(Context, server, text);
                    });

                    data.AddCallBack(GlobalProperties.NoEntryEmoji(), async (c, r) =>
                    {
                        await SendBasicErrorEmbedAsync("Okay, no action will be taken.");
                    });

                    await InlineReactionReplyAsync(data);
                    return;
                }
            }

            await InsertQuote(Context, server, text);
        }

        private async Task InsertQuote(ICommandContext context, Server server, string text)
        {
            var quote = new Quote
            {
                UserId = context.User.Id,
                ServerId = context.Guild.Id,
                Text = text,
                TimeStamp = DateTime.Now.ToOADate()
            };

            var quoteId = await DatabaseQueries.SafeAddQuote(server, quote);            
            var embed = new KaguyaEmbedBuilder
            {
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = $"Quote #{quoteId}",
                        Value = $"Successfully added the quote!\nQuote: `{text}`"
                    }
                }
            };

            await SendEmbedAsync(embed);

            server.NextQuoteId += 1;
            await DatabaseQueries.UpdateAsync(server);
        }
    }
}