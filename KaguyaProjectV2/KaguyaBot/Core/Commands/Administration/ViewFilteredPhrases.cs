using System;
using System.IO;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class ViewFilteredPhrases : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("ViewFilteredPhrases")] 
        [Alias("vfp", "fv", "filterview")]
        [Summary("Displays all currently filtered phrases. If the character count of all phrases total to more than " +
                 "1,750 characters, they will be sent as a text file.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var fp = server.FilteredPhrases.ToList();

            int chars = fp.Sum(phrase => phrase.Phrase.Length);

            string fpStr = "";
            if (chars < 1750)
            {
                fpStr = fp.Aggregate(fpStr, (current, phrase) => current + $"`{phrase.Phrase}`\n");

                if (chars == 0)
                {
                    await Context.Channel.SendBasicErrorEmbedAsync("This server currently has no registered filtered phrases.");
                    return;
                }

                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Filtered Phrases for {Context.Guild.Name}",
                    Description = fpStr
                };

                await Context.Channel.SendEmbedAsync(embed);
                return;
            }

            using (var stream = new MemoryStream())
            {
                var sr = new StreamWriter(stream);

                foreach (var phrase in fp)
                {
                    await sr.WriteLineAsync(phrase.Phrase);
                }

                stream.Seek(0, SeekOrigin.Begin);
                await stream.FlushAsync();

                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Filtered Phrases for {Context.Guild.Name}",
                    Description = $"{Context.User.Mention}, your filtered phrases were too long to send in one message, " +
                                  $"so I put them in a text file for you!"
                };
                await Context.Channel.SendFileAsync(stream, 
                    $"Filtered_Phrases_{Context.Guild.Name}_{DateTime.Now.ToLongDateString()}.txt", 
                    embed: embed.Build());
            }
        }
    }
}
