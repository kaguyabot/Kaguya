using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class RemoveFilteredPhrase : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("FilterRemove")]
        [Alias("fr")]
        [Summary("Removes one phrase or a list of filtered phrases from your server's word filter. Words are separated by periods.")]
        [Remarks("<phrase>.<phrase 2>.{...}\ndodohead.big beachy muffins.penguins!!")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task RemovePhrase(string args)
        {
            string[] newArgs = ArrayInterpreter.ReturnParams(args);

            string s = "s";
            if (newArgs.Length == 1) s = "";

            Server server = ServerQueries.GetServer(Context.Guild.Id);
            List<FilteredPhrase> allFp = ServerQueries.GetAllFilteredPhrasesForServer(Context.Guild.Id);

            if (args.Length == 0)
            {
                KaguyaEmbedBuilder embed0 = new KaguyaEmbedBuilder
                {
                    Description = "Please specify at least one phrase."
                };
                embed0.SetColor(EmbedColor.RED);

                await Context.Channel.SendMessageAsync(embed: embed0.Build());
                return;
            }

            foreach (string element in newArgs)
            {
                FilteredPhrase fp = new FilteredPhrase
                {
                    ServerId = server.Id,
                    Phrase = element
                };

                if (!allFp.Contains(fp)) continue;

                ServerQueries.RemoveFilteredPhrase(fp);
                await ConsoleLogger.Log($"Server {server.Id} has removed the phrase \"{element}\" from their word filter.", DataStorage.JsonStorage.LogLevel.DEBUG);
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully removed {args.Length} phrase{s} from the filter."
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
