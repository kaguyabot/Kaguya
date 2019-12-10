using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    [Group("osutop")]
    public class OsuTop : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [Command()]
        public async Task TopOsuPlays(int num = 5, [Remainder]string player = null)
        {
            if (num < 1 || num > 7)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Number for top plays must be between 1 and 7!** ");
                await ReplyAsync(embed: embed.Build()); 
                return;
            }

            if (player == null || player == "")
            {
                player = (await UserQueries.GetOrCreateUser(Context.User.Id)).OsuId.ToString();
                if (player == "0")
                {
                    embed.WithTitle($"osu! Top {num}");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{(await ServerQueries.GetOrCreateServer(Context.Guild.Id)).CommandPrefix}osuset`!**");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }

            player.Replace(' ', '_');

            var playerBestObjectList = new OsuBestBuilder(player, limit: num).Execute();
            var playerUserObject = new OsuUserBuilder(player).Execute();

            if (playerUserObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            embed.WithAuthor(author =>
            {
                author.Name = $"{playerUserObject.username}'s Top {num} osu! Standard Play";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerUserObject.country}.png";
            });
            embed.WithTitle($"**Top #{num} play for {playerUserObject.username}:**");
            embed.WithUrl($"https://osu.ppy.sh/u/{playerUserObject.user_id}");

            string TopPlayString = "";
            foreach (var playerBestObject in playerBestObjectList)
            {
                TopPlayString += $"\n{playerBestObject.play_number}: ▸ **{playerBestObject.rankemote}{playerBestObject.string_mods}** ▸ {playerBestObject.beatmap_id} ▸ **[{playerBestObject.beatmap.title} [{playerBestObject.beatmap.version}]](https://osu.ppy.sh/b/{playerBestObject.beatmap_id})** " +
                    $"\n▸ **☆{playerBestObject.beatmap.difficultyrating.ToString("N2")}** ▸ **{playerBestObject.accuracy.ToString("F")}%** for **{playerBestObject.pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {playerBestObject.maxcombo}x / Max: {playerBestObject.beatmap.max_combo}]" +
                    $"\n▸ Play made {OsuExtension.ToTimeAgo(DateTime.Now - playerBestObject.date)} ago\n";
            }
            embed.WithDescription(TopPlayString);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("-n")] //osutop extension for a specific top play. Almost the exact same thing as osutop.
        public async Task SpecificOsuTopPlay(int num, [Remainder]string player = null)
        {
            if (num < 1 || num > 100)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Number for top play must be between 1 and 100!**");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            if (player == null || player == "")
            {
                player = (await UserQueries.GetOrCreateUser(Context.User.Id)).OsuId.ToString();
                if (player == null || player == "")
                {
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{(await ServerQueries.GetOrCreateServer(Context.Guild.Id)).CommandPrefix}osuset`!**");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }

            player.Replace(' ', '_');

            var playerUserObject = new OsuUserBuilder(player).Execute();

            //If the API doesn't return anything, send a response in chat letting the user know what happened.
            if (playerUserObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                embed.SetColor(EmbedColor.RED);
                await ReplyAsync(embed: embed.Build());
                return;
            }

            var playerBestObject = new OsuBestBuilder(player, limit: num).Execute(true).Where(c => c.play_number == num).FirstOrDefault();

            embed.WithTitle($"**Top #{num} play for {playerUserObject.username}:**");
            embed.WithUrl($"https://osu.ppy.sh/u/{playerUserObject.user_id}");
            embed.WithDescription($"\n▸ **{playerBestObject.rankemote}{playerBestObject.string_mods}** ▸ {playerBestObject.beatmap_id} ▸ **[{playerBestObject.beatmap.title} [{playerBestObject.beatmap.version}]](https://osu.ppy.sh/b/{playerBestObject.beatmap_id})** " +
                $"\n▸ **☆{playerBestObject.beatmap.difficultyrating.ToString("N2")}** ▸ **{playerBestObject.accuracy.ToString("F")}%** for **{playerBestObject.pp.ToString("F")}pp** " +
                $"\n▸ [Combo: {playerBestObject.maxcombo}x / Max: {playerBestObject.beatmap.max_combo}]" +
                $"\n▸ Play made {OsuExtension.ToTimeAgo(DateTime.Now - playerBestObject.date)} ago\n");

            //Code to build embedded message that is then sent into chat.

            embed.WithAuthor(author =>
            {
                author.Name = $"{playerUserObject.username}'s Top {num} osu! Standard Play";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerUserObject.country}.png";
            });

            await ReplyAsync(embed: embed.Build());
        }
    }
}
