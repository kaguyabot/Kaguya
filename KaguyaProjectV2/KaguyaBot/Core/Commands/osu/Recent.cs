using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using OsuSharp;
using OsuSharp.Oppai;
using User = KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models.User;

// ReSharper disable PossibleInvalidOperationException
namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuRecent : KaguyaBase
    {
        [OsuCommand]
        [Command("osuRecent")]
        [Alias("recent", "r")]
        [Summary("Displays the most recent osu! standard play for the user. The `[player]` parameter " +
                 "is optional if the command executor has already set their name with the `osuset` command.")]
        [Remarks("[player]")]
        public async Task OsuRecentCommand([Remainder] string player = null)
        {
            var embed = new KaguyaEmbedBuilder();
            OsuClient client = OsuBase.Client;

            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            OsuSharp.User osuPlayer = player == null
                ? await client.GetUserByUserIdAsync(user.OsuId, GameMode.Standard)
                : await client.GetUserByUsernameAsync(player, GameMode.Standard);

            // Maybe the user provided an ID to search for.
            if (osuPlayer == null && long.TryParse(player, out long id))
                osuPlayer = await client.GetUserByUserIdAsync(id, GameMode.Standard);

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

            // Reset the embed as its values were changed in the above code.
            embed = new KaguyaEmbedBuilder();
            
            Beatmap beatmap = await osuRecent.GetBeatmapAsync();
            //Author
            embed.WithAuthor(author =>
            {
                author
                    .WithName($"Recent: {osuPlayer.Username} | {beatmap.Title} [{beatmap.Difficulty}] by {beatmap.Author}")
                    .WithIconUrl("https://a.ppy.sh/" + osuPlayer.UserId);
            });

            //Description
            PerformanceData scoredPerformance = await OppaiClient.GetPPAsync(osuRecent.BeatmapId, osuRecent.Mods, (float) osuRecent.Accuracy, osuRecent.MaxCombo);
            PerformanceData wouldbePerformance = await OppaiClient.GetPPAsync(osuRecent.BeatmapId, osuRecent.Mods, (float) osuRecent.Accuracy, beatmap.MaxCombo);

            string beatmapLink = $"https://osu.ppy.sh/b/{beatmap.BeatmapId}";
            string discussionLink = $"https://osu.ppy.sh/beatmapsets/{beatmap.BeatmapsetId}/discussion";
            string downloadLink = $"https://osu.ppy.sh/beatmapsets/{beatmap.BeatmapsetId}/download";
            var descSb = new StringBuilder();
            // Links (Row 0)
            descSb.AppendLine($"**Links:** [Listing]({beatmapLink}) ▸ [Modding]({discussionLink}) ▸ [Download]({downloadLink})");
            descSb.AppendLine();
            // Row 1
            descSb.AppendLine($@"• {OsuBase.OsuGradeEmote(osuRecent.Rank)} {osuRecent.Mods.ToModeString(OsuBase.Client)
                                                                                             .Replace("No Mode", "No Mod")
                                                                                             .Replace("DTNC", "NC")} | {scoredPerformance.Stars:N2}★");
            // Row 2
            descSb.Append($"• **Combo:** {osuRecent.MaxCombo:N0}x / {beatmap.MaxCombo:N0}x ▸ ");
            descSb.AppendLine($"**Accuracy:** {osuRecent.Accuracy:N2}% ▸ **Score:** {osuRecent.TotalScore:N0}");
            // Row 3
            descSb.Append($"• {osuRecent.Count300:N0} / {osuRecent.Count100:N0} / {osuRecent.Count50:N0} / {osuRecent.Miss:N0} ▸ ");
            descSb.AppendLine($"**BPM:** {beatmap.Bpm:N0} ▸ **Length:** {beatmap.TotalLength.TotalMinutes:00}:{beatmap.TotalLength.Seconds:00}");
            // Row 4
            descSb.Append($"• **CS:** {GetStatNumAsString(scoredPerformance.Cs)} ▸ **AR:** {GetStatNumAsString(scoredPerformance.Ar)} ▸ ");
            descSb.AppendLine($"**OD:** {GetStatNumAsString(scoredPerformance.Od)} ▸ **HP:** {GetStatNumAsString(scoredPerformance.Hp)}");
            // Row 5
            descSb.AppendLine($"• **Performance:** {scoredPerformance.Pp:N2}pp ({wouldbePerformance.Pp:N2}pp for {osuRecent.Accuracy:N2}% FC)");
    
            if (osuRecent.MaxCombo == beatmap.MaxCombo)
            {
                embed.SetColor(EmbedColor.GOLD);
                descSb.Append("Full combo!");
            }
            embed.Description = descSb.ToString();
            
            //Footer
            TimeSpan difference = DateTime.UtcNow - osuRecent.Date.Value.DateTime;
            string humanizedDif = difference.Humanize(2, minUnit: TimeUnit.Second);

            embed.WithFooter($"{osuPlayer.Username} performed this play {humanizedDif} ago.");
            
            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        ///     Returns the whole number if even. Otherwise, returns the number to 1 decimal, as a string.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private string GetStatNumAsString(double num)
        {
            string numStr = num.ToString();
            bool isInt = num == (int) num;

            if (isInt)
                return numStr;

            return numStr[..3];
        }
    }
}