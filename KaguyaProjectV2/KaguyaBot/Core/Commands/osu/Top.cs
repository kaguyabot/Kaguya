using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuTop : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [OsuCommand]
        [Command("osuTop")]
        [Summary("Displays the reqested amount of top plays for a player. " +
                 "If a user has their osu! username set with the `osuset` command, " +
                 "they do not need to specify a player. The command used " +
                 "by itself returns 5 top plays for the current user. " +
                 "The amount of requested plays must be between 1 and 7.")]
        [Remarks("\n5 someUser\n<n> <player>")]
        public async Task TopOsuPlays(int num = 5, [Remainder]string player = null)
        {
            if (num < 1 || num > 7)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Number for top plays must be between 1 and 7!** ");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            if (string.IsNullOrEmpty(player))
            {
                player = (await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id)).OsuId.ToString();
                if (player == "0")
                {
                    embed.WithTitle($"osu! Top {num}");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{(await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }

            player = player.Replace(' ', '_');

            var playerBestObjectList = new OsuBestBuilder(player, limit: num).Execute();
            var playerUserObject = new OsuUserBuilder(player).Execute();

            if (playerUserObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} Failed to download data for {player}.");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            embed.WithAuthor(author =>
            {
                author.Name = $"{playerUserObject.Username}'s Top {num} osu! Standard Play";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerUserObject.Country}.png";
            });
            embed.WithTitle($"**Top #{num} play for {playerUserObject.Username}:**");
            embed.WithUrl($"https://osu.ppy.sh/u/{playerUserObject.UserId}");

            string topPlayString = "";
            foreach (var playerBestObject in playerBestObjectList)
            {
                topPlayString += $"\n{playerBestObject.PlayNumber}: ▸ **{playerBestObject.RankEmote}{playerBestObject.StringMods}** ▸ " +
                                 $"{playerBestObject.BeatmapId} ▸ **[{playerBestObject.Beatmap.Title} " +
                                 $"[{playerBestObject.Beatmap.Version}]](https://osu.ppy.sh/b/{playerBestObject.BeatmapId})** " +
                    $"\n▸ **☆{playerBestObject.Beatmap.Difficultyrating:N2}** ▸ **{playerBestObject.Accuracy:F}%** for **{playerBestObject.PP:F}pp** " +
                    $"\n▸ [Combo: {playerBestObject.MaxCombo}x / Max: {playerBestObject.Beatmap.MaxCombo}]" +
                    $"\n▸ Play made {OsuExtension.ToTimeAgo(DateTime.Now - playerBestObject.Date)} ago\n";
            }
            embed.WithDescription(topPlayString);

            await ReplyAsync(embed: embed.Build());
        }

        [OsuCommand]
        [Command("osuTop -n")] //osutop extension for a specific top play. Almost the exact same thing as osutop.
        [Summary("Displays a specific play for a user out of their top 100 plays. " +
                 "The play number must be between 1 and 100. Specifying 1 will " +
                 "display the user's best play, 100 will return their worst (out of their top 100). " +
                 "If a user has set their osu! username with the `osuset` command, they do not need " +
                 "to specify a player if they want to return their own information.")]
        [Remarks("<n>\n<n> <player>")]
        public async Task SpecificOsuTopPlay(int num, [Remainder]string player = null)
        {
            if (num < 1 || num > 100)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Number for top play must be between 1 and 100!**");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            if (string.IsNullOrEmpty(player))
            {
                player = (await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id)).OsuId.ToString();
                if (player == "")
                {
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{(await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }

            player = player.Replace(' ', '_');

            var playerUserObject = new OsuUserBuilder(player).Execute();

            //If the API doesn't return anything, send a response in chat letting the user know what happened.
            if (playerUserObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                embed.SetColor(EmbedColor.RED);
                await ReplyAsync(embed: embed.Build());
                return;
            }

            var playerBestObject = new OsuBestBuilder(player, limit: num).Execute(true).FirstOrDefault(c => c.PlayNumber == num);

            embed.WithTitle($"**Top #{num} play for {playerUserObject.Username}:**");
            embed.WithUrl($"https://osu.ppy.sh/u/{playerUserObject.UserId}");
            embed.WithDescription($"\n▸ **{playerBestObject.RankEmote}{playerBestObject.StringMods}** ▸ {playerBestObject.BeatmapId} ▸ **[{playerBestObject.Beatmap.Title} [{playerBestObject.Beatmap.Version}]](https://osu.ppy.sh/b/{playerBestObject.BeatmapId})** " +
                $"\n▸ **☆{playerBestObject.Beatmap.Difficultyrating.ToString("N2")}** ▸ **{playerBestObject.Accuracy.ToString("F")}%** for **{playerBestObject.PP.ToString("F")}pp** " +
                $"\n▸ [Combo: {playerBestObject.MaxCombo}x / Max: {playerBestObject.Beatmap.MaxCombo}]" +
                $"\n▸ Play made {OsuExtension.ToTimeAgo(DateTime.Now - playerBestObject.Date)} ago\n");

            //Code to build embedded message that is then sent into chat.

            embed.WithAuthor(author =>
            {
                author.Name = $"{playerUserObject.Username}'s Top {num} osu! Standard Play";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerUserObject.Country}.png";
            });

            await ReplyAsync(embed: embed.Build());
        }
    }
}
