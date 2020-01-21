using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuRecent : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [OsuCommand]
        [Command("osuRecent")]
        [Alias("recent", "r")]
        [Summary("Displays the most recent osu! play for the user.")]
        [Remarks("<username or ID> (Optional if configured with osuset already)")]
        public async Task OsuRecentCommand([Remainder]string player = null)
        {
            OsuUserModel userProfileObject = null;
            if (player != null)
                userProfileObject = new OsuUserBuilder(player).Execute();
            
            if (userProfileObject == null)
            {
                userProfileObject = new OsuUserBuilder((await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id)).OsuId.ToString()).Execute();
                if (userProfileObject == null)
                {
                    embed.WithTitle($"osu! Recent");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! " +
                                          $"Please specify a player or set your osu! username with " +
                                          $"`{(await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }

            //Getting recent object.
            var playerRecentObjectList = new OsuRecentBuilder(userProfileObject.UserId.ToString()).Execute();

            if (!playerRecentObjectList.Any())
            {
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("" + userProfileObject.Username + " hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + userProfileObject.UserId);
                });
            }
            else
            {
                //Author
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + userProfileObject.Username)
                        .WithIconUrl("https://a.ppy.sh/" + userProfileObject.UserId);
                });

                //Description
                foreach (var playerRecentObject in playerRecentObjectList)
                {
                    embed.Description += $"▸ **{playerRecentObject.RankEmote}{playerRecentObject.ModString}** ▸ **[{playerRecentObject.Beatmap.Title} [{playerRecentObject.Beatmap.Version}]](https://osu.ppy.sh/b/{playerRecentObject.BeatmapId})** by **{playerRecentObject.Beatmap.Artist}**\n" +
                        $"▸ **☆{playerRecentObject.Beatmap.Difficultyrating:F}** ▸ **{playerRecentObject.Accuracy:F}%**\n" +
                        $"▸ **Combo:** `{playerRecentObject.MaxCombo:N0}x / {playerRecentObject.Beatmap.MaxCombo:N0}x`\n" +
                        $"▸ [300 / 100 / 50 / X]: `[{playerRecentObject.Count300} / {playerRecentObject.Count100} / {playerRecentObject.Count50} / {playerRecentObject.Countmiss}]`\n" +
                        $"▸ **Map Completion:** `{Math.Round(playerRecentObject.Completion, 2)}%`\n" +
                        $"▸ **Full Combo Percentage:** `{(((double)playerRecentObject.MaxCombo / (double)playerRecentObject.Beatmap.MaxCombo) * 100):N2}%`\n";

                    if (playerRecentObject == playerRecentObjectList[^1])
                        embed.Description += $"▸ **PP for FC**: `{playerRecentObject.FullComboPP:N0}pp`";
                    else
                        embed.Description += $"▸ **PP for FC**: `{playerRecentObject.FullComboPP:N0}pp`\n";
                }

                //Footer
                var difference = DateTime.UtcNow - playerRecentObjectList.LastOrDefault().Date;

                embed.WithFooter(playerRecentObjectList.Count > 1
                    ? $"{userProfileObject.Username} performed this plays {(int) difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago."
                    : $"{userProfileObject.Username} performed this play {(int) difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.");
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
