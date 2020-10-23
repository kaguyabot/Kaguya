using System.Collections.Generic;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class DeleteAsync : KaguyaBase
    {
        [AdminCommand]
        [Command("FilterRemove")]
        [Alias("fr")]
        [Summary("Removes one phrase or a list of filtered phrases from your server's word filter. Phrases are separated by spaces. " +
                 "If a phrase is longer than one word, surround it with `\"\"`.")]
        [Remarks("<phrase> {...}\nMyPhrase \"My phrase with spaces\"")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemovePhrase(params string[] args)
        {
            string s = "s";
            if (args.Length == 1) s = "";

            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            List<FilteredPhrase> allFp = server.FilteredPhrases.ToList();

            if (args.Length == 0)
            {
                var embed0 = new KaguyaEmbedBuilder
                {
                    Description = "Please specify at least one phrase."
                };

                embed0.SetColor(EmbedColor.RED);

                await Context.Channel.SendEmbedAsync(embed0);

                return;
            }

            int remCount = 0;
            bool matches = false;
            foreach (string element in args)
            {
                if (!allFp.Any(x => x.Phrase.ToLower() == element.ToLower() && x.ServerId == server.ServerId)) continue;

                await DatabaseQueries.DeleteAsync(allFp.FirstOrDefault(x =>
                    x.Phrase.ToLower() == element.ToLower() && x.ServerId == server.ServerId));

                matches = true;
                remCount++;

                await ConsoleLogger.LogAsync($"Server {server.ServerId} has removed the phrase \"{element}\" from their word filter.",
                    DataStorage.JsonStorage.LogLvl.DEBUG);
            }

            KaguyaEmbedBuilder embed;

            if (!matches)
            {
                embed = new KaguyaEmbedBuilder(EmbedColor.RED)
                {
                    Description = "The phrases you wrote here are not present in your filter.",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "If your phrase has a space, wrap it in quotation marks."
                    }
                };
            }
            else
            {
                if (remCount == args.Length)
                {
                    embed = new KaguyaEmbedBuilder(EmbedColor.VIOLET)
                    {
                        Description = $"Successfully removed `{args.Length:N0}` phrase{s} from the filter."
                    };
                }
                else
                {
                    s = remCount == 0 ? "" : "s";
                    embed = new KaguyaEmbedBuilder(EmbedColor.ORANGE)
                    {
                        Description = $"Successfully removed `{remCount:N0}` phrase{s} from the filter.\n" +
                                      $"`{args.Length - remCount}` phrases did not exist in the filter."
                    };
                }
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}