using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using OsuSharp;
using OsuSharp.Oppai;
using System;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable PossibleInvalidOperationException

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuRecent : KaguyaBase
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [OsuCommand]
        [Command("osuRecent")]
        [Alias("recent", "r")]
        [Summary("Displays the most recent osu! play for the user. If the user has spaces in their " +
                 "username, wrap the name with quotation marks. You may see up to 5 of the user's most " +
                 "recent plays at once via the `limit` property (default limit is 1).")]
        [Remarks("[username, NOT ID] [limit] (Optional if configured with `osuset` already)\n" +
                 "SomeName\n\"Name with spaces 123\" 3")]
        public async Task OsuRecentCommand(string player = null)
        {
            var client = OsuBase.client;
            var gameMode = GameMode.Standard;

            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var osuPlayer = player == null
                ? await client.GetUserByUserIdAsync(user.OsuId, gameMode)
                : await client.GetUserByUsernameAsync(player, gameMode);

            if (osuPlayer == null)
            {
                embed.WithTitle($"osu! Recent");
                embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! " +
                                      $"Please specify a player or set your osu! username with " +
                                      $"`{server.CommandPrefix}osuset`!**");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            var osuRecents = player == null
                ? await client.GetUserRecentsByUserIdAsync(user.OsuId, GameMode.Standard, 1)
                : await client.GetUserRecentsByUsernameAsync(player, GameMode.Standard, 1);

            if (!osuRecents.Any())
            {
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"{osuPlayer.Username} hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + osuPlayer.UserId);
                });
            }
            else
            {
                //Author
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! {gameMode} Play for {osuPlayer.Username}")
                        .WithIconUrl("https://a.ppy.sh/" + osuPlayer.UserId);
                });

                //Description
                var beatmap = await osuRecents[0].GetBeatmapAsync();
                var pp = await beatmap.GetPPAsync((float) osuRecents[0].Accuracy);

                embed.Description += $"▸ **{OsuBase.OsuGrade(osuRecents[0].Rank)}{osuRecents[0].Mods}** ▸ " +
                                     $"**[{beatmap.Title} [{beatmap.Difficulty}]]" +
                                     $"(https://osu.ppy.sh/b/{osuRecents[0].BeatmapId})** by **{beatmap.Artist}**\n" +
                                     $"▸ **☆{beatmap.StarRating:F}** ▸ **{osuRecents[0].Accuracy:N2}%**\n" +
                                     $"▸ **Combo:** `{osuRecents[0].MaxCombo:N0}x / {beatmap.MaxCombo:N0}x`\n" +
                                     $"▸ `[{osuRecents[0].Count300} / {osuRecents[0].Count100} / " +
                                     $"{osuRecents[0].Count50} / {osuRecents[0].Miss}]` ▸ " +
                                     $"`[{osuRecents[0].Geki}激 / {osuRecents[0].Katu}喝]`\n" +
                                     $"▸ **Map Completion:** `{MapCompletionPercent(osuRecents[0], beatmap) * 100:N2}%`\n" +
                                     $"▸ **Max Combo Percentage:** `{(double)osuRecents[0].MaxCombo / beatmap.MaxCombo * 100:N2}%`\n";

                embed.Description += $"▸ **PP for FC**: `{pp.Pp:N0}pp`\n";

                //Footer
                var difference = DateTime.UtcNow - osuRecents.Last().Date.Value.ToLocalTime();

                embed.WithFooter(osuRecents.Count > 1
                    ? $"{osuPlayer.Username} performed these plays {(int)difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago."
                    : $"{osuPlayer.Username} performed this play {(int)difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.");
            }

            await ReplyAsync(embed: embed.Build());
        }

        private double MapCompletionPercent(Score score, Beatmap beatmap)
        {
            if(beatmap.MaxCombo == null)
                throw new KaguyaSupportException("This score does not have a max combo, according to the API.");
            return (double) (score.Count50 + score.Count100 + score.Count300 + score.Miss) / beatmap.MaxCombo.Value;
        }
    }
}
