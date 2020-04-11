using System.Linq;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.Text;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class RemoveQuote : KaguyaBase
    {
        [ExpCommand]
        [Command("RemoveQuote")]
        [Alias("deletequote", "dq", "rq")]
        [Summary("Allows a moderator to remove a quote from the server. Quotes are removed by passing in the ID.")]
        [Remarks("<ID>")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Command(int id)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var quotes = server.Quotes;
            int quoteCount = quotes.Count();

            if(quoteCount == 0)
            {
                await SendBasicErrorEmbedAsync("This server does not have any quotes, there is nothing to remove.");
                return;
            }

            Quote match = null;
            try
            {
                match = quotes.First(x => x.Id == id && x.ServerId == server.ServerId);
            }
            catch(ArgumentNullException)
            {
                var eSb = new StringBuilder();
                eSb.AppendLine("The ID you specified does not exist in this server's quote collection.");
                eSb.AppendLine($"To find all of your quotes with their IDs, use the `{server.CommandPrefix}allquotes` command.");
                await SendBasicErrorEmbedAsync(eSb.ToString());
            }

            if(match == null)
            {
                var eSb = new StringBuilder();
                eSb.AppendLine("The quote object returned null. This is most likely because the quote ID ");
                eSb.Append("you provided did not match any quotes for this server.");
                throw new KaguyaSupportException(eSb.ToString());
            }

            await DatabaseQueries.DeleteAsync(match);
            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully removed quote #{id} from the server's quote collection."
            };

            await SendEmbedAsync(embed);
        }
    }
}