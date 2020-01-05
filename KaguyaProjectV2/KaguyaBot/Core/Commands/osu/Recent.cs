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
            var playerRecentObjectList = new OsuRecentBuilder(userProfileObject.user_id.ToString()).Execute();

            if (!playerRecentObjectList.Any())
            {
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("" + userProfileObject.username + " hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + userProfileObject.user_id);
                });
            }
            else
            {
                //Author
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + userProfileObject.username)
                        .WithIconUrl("https://a.ppy.sh/" + userProfileObject.user_id);
                });

                //Description
                foreach (var playerRecentObject in playerRecentObjectList)
                {
                    embed.Description += $"▸ **{playerRecentObject.rankemote}{playerRecentObject.string_mods}** ▸ **[{playerRecentObject.beatmap.title} [{playerRecentObject.beatmap.version}]](https://osu.ppy.sh/b/{playerRecentObject.beatmap_id})** by **{playerRecentObject.beatmap.artist}**\n" +
                        $"▸ **☆{playerRecentObject.beatmap.difficultyrating:F}** ▸ **{playerRecentObject.accuracy:F}%**\n" +
                        $"▸ **Combo:** `{playerRecentObject.maxcombo:N0}x / {playerRecentObject.beatmap.max_combo:N0}x`\n" +
                        $"▸ [300 / 100 / 50 / X]: `[{playerRecentObject.count300} / {playerRecentObject.count100} / {playerRecentObject.count50} / {playerRecentObject.countmiss}]`\n" +
                        $"▸ **Map Completion:** `{Math.Round(playerRecentObject.completion, 2)}%`\n" +
                        $"▸ **Full Combo Percentage:** `{(((double)playerRecentObject.maxcombo / (double)playerRecentObject.beatmap.max_combo) * 100):N2}%`\n";

                    if (playerRecentObject == playerRecentObjectList[^1])
                        embed.Description += $"▸ **PP for FC**: `{playerRecentObject.fullcombopp:N0}pp`";
                    else
                        embed.Description += $"▸ **PP for FC**: `{playerRecentObject.fullcombopp:N0}pp`\n";
                }

                //Footer
                var difference = DateTime.UtcNow - playerRecentObjectList.LastOrDefault().date;

                embed.WithFooter(playerRecentObjectList.Count > 1
                    ? $"{userProfileObject.username} performed this plays {(int) difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago."
                    : $"{userProfileObject.username} performed this play {(int) difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.");
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
