using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class UpdateAsync : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("FilterAdd")]
        [Alias("fa")]
        [Summary("Adds a list of filtered phrases to your server's word filter. " +
            "These are phrases that will automatically be deleted when typed in chat. " +
            "Users with the `Administrator` permission automatically are ignored by this " +
            "filter.")]
        [Remarks("<phrase> {...}")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddPhrase(params string[] args)
        {
            string s = "s";
            if (args.Length == 1) s = "";

            Server server = DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id).Result;
            var allFp = server.FilteredPhrases.ToList();

            if(args.Length == 0)
            {
                KaguyaEmbedBuilder embed0 = new KaguyaEmbedBuilder
                {
                    Description = "Please specify at least one phrase."
                };
                embed0.SetColor(EmbedColor.RED);

                await Context.Channel.SendMessageAsync(embed: embed0.Build());
                return;
            }

            foreach (string element in args)
            {
                FilteredPhrase fp = new FilteredPhrase
                {
                    ServerId = server.ServerId,
                    Phrase = element
                };

                if (allFp.Contains(fp)) continue;

                await DatabaseQueries.InsertOrReplaceAsync(fp); 
                await ConsoleLogger.LogAsync($"Server {server.ServerId} has added the phrase \"{element}\" to their word filter.", DataStorage.JsonStorage.LogLvl.DEBUG);
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully added {args.Length} phrase{s} to the filter."
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}