using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Core.UserAccounts;
using Kaguya.Core.Server_Files;
using System.Diagnostics;
using Discord.Addons.Interactive;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.Embed;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;
using Kaguya.Core.Osu.Builder;

namespace Kaguya.Modules.osu
{
    [Group("osutop")]
    public class osuTop : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command()]
        public async Task TopOsuPlays(int num = 5, [Remainder]string player = null)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            if (num < 1 || num > 7)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Number for top plays must be between 1 and 10!** ");
                await BE(); return;
            }

            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
                if (player == null || player == "")
                {
                    embed.WithTitle($"osu! Top {num}");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    await BE();
                    return;
                }
            }

            player.Replace(' ', '_');

            var playerBestObjectList = new OsuBestBuilder(player, limit: num).Execute();
            var playerUserObject = new OsuUserBuilder(player).Execute();

            if (playerUserObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                await BE();
                return;
            }

            string TopPlayString = ""; //Country images to come later.
            foreach (var playerBestObject in playerBestObjectList)
            {
                TopPlayString += $"\n{playerBestObject.play_number}: ▸ **{playerBestObject.rankemote}{playerBestObject.string_mods}** ▸ {playerBestObject.beatmap_id} ▸ **[{playerBestObject.beatmap.title} [{playerBestObject.beatmap.version}]](https://osu.ppy.sh/b/{playerBestObject.beatmap_id})** " +
                    $"\n▸ **☆{playerBestObject.beatmap.difficultyrating.ToString("N2")}** ▸ **{playerBestObject.accuracy.ToString("F")}%** for **{playerBestObject.pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {playerBestObject.maxcombo}x / Max: {playerBestObject.beatmap.max_combo}]" +
                    $"\n▸ Play made {Core.Osu.OsuMisc.ToTimeAgo(DateTime.Now - playerBestObject.date)} ago\n";
            }

            await GlobalCommandResponses.CreateCommandResponse(Context,
                $"**Top {num} osu! standard plays for {playerUserObject.username}:**",
                $"osu! Stats for player **{playerUserObject.username}**:\n" + TopPlayString,
                thumbnailURL: $"https://osu.ppy.sh/u/{playerUserObject.user_id}");
        }

        [Command("-n")] //osutop extension for a specific top play. Almost the exact same thing as osutop.
        public async Task SpecificOsuTopPlay(int num, [Remainder]string player = null)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;

            if (num < 1 || num > 100)
            {
                stopWatch.Stop();
                await GlobalCommandResponses.CreateCommandError(Context,
                    stopWatch.ElapsedMilliseconds,
                    CommandError.Unsuccessful,
                    "Number for top play must be between 1 and 100.",
                    description: $"{Context.User.Mention} **ERROR: Number for top play must be between 1 and 100!**");
                return;
            }

            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
                if (player == null || player == "")
                {
                    await GlobalCommandResponses.CreateCommandError(Context,
                        stopWatch.ElapsedMilliseconds,
                        CommandError.UnmetPrecondition,
                        "Failed to acquire osu! username.",
                        $"osu! Top {num}",
                        $"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                }
            }
            player.Replace(' ', '_');

            var playerUserObject = new OsuUserBuilder(player).Execute();

            //If the API doesn't return anything, send a response in chat letting the user know what happened.
            if (playerUserObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            var playerBestObject = new OsuBestBuilder(player, limit: num).Execute(true).Where(c => c.play_number == num).FirstOrDefault();

            string TopPlayString = $"\n▸ **{playerBestObject.rankemote}{playerBestObject.string_mods}** ▸ {playerBestObject.beatmap_id} ▸ **[{playerBestObject.beatmap.title} [{playerBestObject.beatmap.version}]](https://osu.ppy.sh/b/{playerBestObject.beatmap_id})** " +
                $"\n▸ **☆{playerBestObject.beatmap.difficultyrating.ToString("N2")}** ▸ **{playerBestObject.accuracy.ToString("F")}%** for **{playerBestObject.pp.ToString("F")}pp** " +
                $"\n▸ [Combo: {playerBestObject.maxcombo}x / Max: {playerBestObject.beatmap.max_combo}]" +
                $"\n▸ Play made {Core.Osu.OsuMisc.ToTimeAgo(DateTime.Now - playerBestObject.date)} ago\n";

            //Code to build embedded message that is then sent into chat.

            embed.WithAuthor(author =>
            {
                author.Name = $"{playerUserObject.username}'s Top {num} osu! Standard Play";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerUserObject.country}.png";
            });

            embed.WithTitle($"**Top #{num} play for {playerUserObject.username}:**");
            embed.WithUrl($"https://osu.ppy.sh/u/{playerUserObject.user_id}");
            embed.WithDescription($"{TopPlayString}");
            await BE();
        }
    }
}
