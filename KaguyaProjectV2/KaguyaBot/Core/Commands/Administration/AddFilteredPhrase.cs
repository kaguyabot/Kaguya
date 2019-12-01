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
    public class AddFilteredPhrase : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("FilterAdd")]
        [Alias("fa")]
        [Summary("Adds one phrase (or a list of phrases) to your server's word filter. " +
            "These are phrases that will automatically be deleted when typed in chat. UserQueries with the Administrator permission are excluded from punishment.")]
        [Remarks("dodohead.big beachy moofins.penguins!!")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddPhrase(params string[] args)
        {
            string s = "s";
            if (args.Length == 1) s = "";

            Server server = ServerQueries.GetServer(Context.Guild.Id);
            List<FilteredPhrase> allFp = ServerQueries.GetAllFilteredPhrasesForServer(Context.Guild.Id);

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
                    ServerId = server.Id,
                    Phrase = element
                };

                if (allFp.Contains(fp)) continue;

                ServerQueries.AddFilteredPhrase(fp); 
                await ConsoleLogger.Log($"Server {server.Id} has added the phrase \"{element}\" to their word filter.", DataStorage.JsonStorage.LogLevel.DEBUG);
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