using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuTop : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [OsuCommand]
        [Command("osutop")]
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

            if (player == null || player == "")
            {
                player = (await UserQueries.GetOrCreateUserAsync(Context.User.Id)).OsuId.ToString();
                if (player == "0")
                {
                    embed.WithTitle($"osu! Top {num}");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{(await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
            }

            player = player.Replace(' ', '_');

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

            string topPlayString = "";
            foreach (var playerBestObject in playerBestObjectList)
            {
                topPlayString += $"\n{playerBestObject.play_number}: ▸ **{playerBestObject.rankemote}{playerBestObject.string_mods}** ▸ {playerBestObject.beatmap_id} ▸ **[{playerBestObject.beatmap.title} [{playerBestObject.beatmap.version}]](https://osu.ppy.sh/b/{playerBestObject.beatmap_id})** " +
                    $"\n▸ **☆{playerBestObject.beatmap.difficultyrating.ToString("N2")}** ▸ **{playerBestObject.accuracy.ToString("F")}%** for **{playerBestObject.pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {playerBestObject.maxcombo}x / Max: {playerBestObject.beatmap.max_combo}]" +
                    $"\n▸ Play made {OsuExtension.ToTimeAgo(DateTime.Now - playerBestObject.date)} ago\n";
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

            if (player == null || player == "")
            {
                player = (await UserQueries.GetOrCreateUserAsync(Context.User.Id)).OsuId.ToString();
                if (player == null || player == "")
                {
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{(await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");
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

            var playerBestObject = new OsuBestBuilder(player, limit: num).Execute(true).FirstOrDefault(c => c.play_number == num);

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
