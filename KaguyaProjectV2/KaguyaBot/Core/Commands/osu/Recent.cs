using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using OsuSharp;
using OsuSharp.Oppai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using User = KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models.User;

// ReSharper disable PossibleInvalidOperationException
namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuRecent : KaguyaBase
    {
        [OsuCommand]
        [Command("osuRecent")]
        [Alias("recent", "r")]
        [Summary("Displays the most recent osu! Standard play for the user. The `[player]` parameter " +
                 "is optional if the command executor has already set their name with the `osuset` command.")]
        [Remarks("[player]")]
        public async Task OsuRecentCommand([Remainder]string player = null)
        {
            var embed = new KaguyaEmbedBuilder();
            var client = OsuBase.Client;

            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            OsuSharp.User osuPlayer = player == null
                ? await client.GetUserByUserIdAsync(user.OsuId, GameMode.Standard)
                : await client.GetUserByUsernameAsync(player, GameMode.Standard);

            // Maybe the user provided an ID to search for.
            if (osuPlayer == null && long.TryParse(player, out long id))
            {
                osuPlayer = await client.GetUserByUserIdAsync(id, GameMode.Standard);
            }

            // If it's still null, they don't exist.
            if (osuPlayer == null)
            {
                embed.Description = $"{Context.User.Mention} Failed to acquire username! " +
                                      "Please specify a player or set your osu! username with " +
                                      $"`{server.CommandPrefix}osuset`!";

                await ReplyAsync(embed: embed.Build());

                return;
            }

            Score osuRecent = player == null
                ? (await client.GetUserRecentsByUserIdAsync(user.OsuId, GameMode.Standard, 1)).FirstOrDefault()
                : (await client.GetUserRecentsByUsernameAsync(player, GameMode.Standard, 1)).FirstOrDefault();

            if (osuRecent == null)
            {
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"{osuPlayer.Username} doesn't have any recent plays.")
                        .WithIconUrl("https://a.ppy.sh/" + osuPlayer.UserId);
                });

                await SendEmbedAsync(embed);
                return;
            }
            
            //Author
            embed.WithAuthor(author =>
            {
                author
                    .WithName($"Most recent osu! standard play for {osuPlayer.Username}")
                    .WithIconUrl("https://a.ppy.sh/" + osuPlayer.UserId);
            });

            //Description
            Beatmap beatmap = await osuRecent.GetBeatmapAsync();
            PerformanceData pp = await beatmap.GetPPAsync((float) osuRecent.Accuracy);

            embed.Description += $"▸ **{OsuBase.OsuGrade(osuRecent.Rank)}{osuRecent.Mods}** ▸ " +
                                 $"**[{beatmap.Title} [{beatmap.Difficulty}]]" +
                                 $"(https://osu.ppy.sh/b/{osuRecent.BeatmapId})** by **{beatmap.Artist}**\n" +
                                 $"▸ **☆{beatmap.StarRating:F}** ▸ **{osuRecent.Accuracy:N2}%**\n" +
                                 $"▸ **Combo:** `{osuRecent.MaxCombo:N0}x / {beatmap.MaxCombo:N0}x`\n" +
                                 $"▸ `[{osuRecent.Count300} / {osuRecent.Count100} / " +
                                 $"{osuRecent.Count50} / {osuRecent.Miss}]` ▸ " +
                                 $"`[{osuRecent.Geki}激 / {osuRecent.Katu}喝]`\n" +
                                 $"▸ **Map Completion:** `{MapCompletionPercent(osuRecent, beatmap) * 100:N2}%`\n" +
                                 $"▸ **Max Combo Percentage:** `{((double) osuRecent.MaxCombo / beatmap.MaxCombo) * 100:N2}%`\n";

            embed.Description += $"▸ **PP for FC**: `{pp.Pp:N0}pp`\n";

            //Footer
            TimeSpan difference = DateTime.UtcNow - osuRecent.Date.Value.DateTime;
            string humanizedDif = difference.Humanize(2, minUnit: TimeUnit.Second);

            embed.WithFooter($"{osuPlayer.Username} performed this play {humanizedDif} ago.");

            await ReplyAsync(embed: embed.Build());
        }

        private double MapCompletionPercent(Score score, Beatmap beatmap)
        {
            if (beatmap.MaxCombo == null)
                throw new KaguyaSupportException("This score does not have a max combo, according to the API.");

            return (double) (score.Count50 + score.Count100 + score.Count300 + score.Miss) / beatmap.MaxCombo.Value;
        }
    }
}