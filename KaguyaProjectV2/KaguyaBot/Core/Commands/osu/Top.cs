using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using OsuSharp;
using OsuSharp.Oppai;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using User = OsuSharp.User;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuTop : KaguyaBase
    {
        public KaguyaEmbedBuilder Embed = new KaguyaEmbedBuilder();

        [OsuCommand]
        [Command("osuTop")]
        [Summary("Displays the reqested amount of top plays for a player. " +
                 "If a user has their osu! username set with the `osuset` command, " +
                 "they do not need to specify a player. The command used " +
                 "by itself returns 5 top plays for the current user. " +
                 "The amount of requested plays must be between 1 and 7.")]
        [Remarks("\n<player>\n<index> <player>\nSomePlayer\n6 SomePlayer")]
        public async Task TopOsuPlaysNoNum([Remainder] string player) => TopOsuPlays(5, player);

        [OsuCommand]
        [Command("osuTop")]
        [Summary("Displays the reqested amount of top plays for a player. " +
                 "If a user has their osu! username set with the `osuset` command, " +
                 "they do not need to specify a player. The command used " +
                 "by itself returns 5 top plays for the current user. " +
                 "The amount of requested plays must be between 1 and 7.")]
        [Remarks("\n<player>\n<index> <player>\nSomePlayer\n6 SomePlayer")]
        public async Task TopOsuPlays(int num = 5, [Remainder] string player = null)
        {
            DataStorage.DbData.Models.User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            User osuUser;

            if (num < 1 || num > 7)
            {
                await SendBasicErrorEmbedAsync("Number of plays must be between 1 and 7.");

                return;
            }

            if (string.IsNullOrEmpty(player))
            {
                osuUser = await OsuBase.Client.GetUserByUserIdAsync(user.OsuId, GameMode.Standard);
                if (osuUser.UserId == 0)
                {
                    Embed.WithTitle($"osu! Top {num}");
                    Embed.WithDescription($"**{Context.User.Mention} Failed to acquire username. " +
                                          $"Please specify a player or set your osu! username with " +
                                          $"`{(await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");

                    await ReplyAsync(embed: Embed.Build());

                    return;
                }
            }
            else
            {
                player = player.Replace(' ', '_');
                osuUser = await OsuBase.Client.GetUserByUsernameAsync(player, GameMode.Standard);
            }

            if (osuUser == null)
            {
                throw new KaguyaSupportException($"Failed to download data for player. If no user was specified, " +
                                                 $"you must set your osu! username or user ID via " +
                                                 $"`{server.CommandPrefix}osuset <name/ID>`.\n\n" +
                                                 $"If a username was specified, it's likely that the user does not exist or " +
                                                 $"you are providing invalid data.");
            }

            IReadOnlyList<Score> playerBestObjectList = await OsuBase.Client.GetUserBestsByUserIdAsync(osuUser.UserId, GameMode.Standard, num);
            playerBestObjectList = playerBestObjectList.OrderByDescending(x => x.PerformancePoints).ToList().AsReadOnly();

            User playerUserObject = await OsuBase.Client.GetUserByUserIdAsync(osuUser.UserId, GameMode.Standard);

            string s = num == 1 ? "" : "s";

            Embed.WithAuthor(author =>
            {
                author.Name = $"{playerUserObject.Username}'s Top {(num == 1 ? "" : num.ToString())} osu! Standard Play{s}";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerUserObject.Country}.png";
            });

            Embed.WithTitle($"**Top {num} play for {playerUserObject.Username}:**");
            Embed.WithUrl($"https://osu.ppy.sh/u/{playerUserObject.UserId}");

            int i = 0;
            string topPlayString = "";
            foreach (Score playerBestObject in playerBestObjectList)
            {
                i++;
                Beatmap beatmap = await playerBestObject.GetBeatmapAsync();
                PerformanceData pp = await beatmap.GetPPAsync(playerBestObject.Mods, (float) playerBestObject.Accuracy);

                Debug.Assert(playerBestObject.Date != null, "playerBestObject.Date != null");
                topPlayString += $"\n{i}: ▸ **{OsuBase.OsuGrade(playerBestObject.Rank)}" +
                                 $@"{playerBestObject.Mods.ToModeString(OsuBase.Client)
                                                     .Replace("No Mode", "No Mod")
                                                     .Replace("DTNC", "NC")}** ▸ " +
                                 $"{beatmap.BeatmapId} ▸ **[{beatmap.Title} " +
                                 $"[{beatmap.Difficulty}]](https://osu.ppy.sh/b/{beatmap.BeatmapId})** " +
                                 $"\n▸ **☆{beatmap.StarRating:N2}** ▸ **{playerBestObject.Accuracy:F}%** " +
                                 $"for **{pp.Pp:F}pp** " +
                                 $"\n▸ [Combo: {playerBestObject.MaxCombo}x / Max: {beatmap.MaxCombo}]" +
                                 $"\n▸ Play made {(DateTime.Now - playerBestObject.Date.Value).Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year, precision: 3)} ago\n";
            }

            Embed.WithDescription(topPlayString);
            await ReplyAsync(embed: Embed.Build());
        }

        [OsuCommand]
        [Command("osuTop -n")]
        [Summary("Displays a specific play for a user out of their top 100 plays. " +
                 "The play number must be between 1 and 100. Specifying 1 will " +
                 "display the user's best play, 100 will return their worst (out of their top 100). " +
                 "If a user has set their osu! username with the `osuset` command, they do not need " +
                 "to specify a player if they want to return their own information.")]
        [Remarks("<index>\n<index> <player>")]
        //osutop extension for a specific top play. Almost the exact same thing as osutop.
        public async Task SpecificOsuTopPlay(int num, [Remainder] string player = null)
        {
            DataStorage.DbData.Models.User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            User osuUser;

            if (num < 1 || num > 100)
            {
                await SendBasicErrorEmbedAsync("Play index must be between 1 and 100.");

                return;
            }

            if (string.IsNullOrEmpty(player))
            {
                osuUser = await OsuBase.Client.GetUserByUserIdAsync(user.OsuId, GameMode.Standard);
                if (osuUser == null)
                {
                    Embed.WithTitle($"osu! Top {num}");
                    Embed.WithDescription($"**{Context.User.Mention} Failed to acquire username. " +
                                          $"Please specify a player or set your osu! username with " +
                                          $"`{(await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id)).CommandPrefix}osuset`!**");

                    await ReplyAsync(embed: Embed.Build());

                    return;
                }
            }
            else
            {
                player = player.Replace(' ', '_');
                osuUser = await OsuBase.Client.GetUserByUsernameAsync(player, GameMode.Standard);

                if (osuUser == null)
                {
                    await SendBasicErrorEmbedAsync($"{Context.User.Mention} Failed to download data for `{player}`.");

                    return;
                }
            }

            IReadOnlyList<Score> playerBestObjectList = await OsuBase.Client.GetUserBestsByUserIdAsync(osuUser.UserId, GameMode.Standard);

            if (playerBestObjectList.Count < num)
            {
                await SendBasicErrorEmbedAsync($"The user does not have `{num}` top plays recorded. " +
                                               $"They only have `{playerBestObjectList.Count}`.");

                return;
            }

            Score playerBestObject = playerBestObjectList[num - 1];

            if (playerBestObject == null)
            {
                await SendBasicErrorEmbedAsync($"A play could not be found for this user at the given index.");

                return;
            }

            Embed.WithAuthor(author =>
            {
                author.Name = $"{osuUser.Username}'s {num.Ordinalize()} Top osu! {playerBestObject.GameMode} Play";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{osuUser.Country}.png";
            });

            Embed.WithTitle($"**Top #{num} play for {osuUser.Username}:**");
            Embed.WithUrl($"https://osu.ppy.sh/u/{osuUser.UserId}");

            Beatmap beatmap = await playerBestObject.GetBeatmapAsync();
            PerformanceData pp = await beatmap.GetPPAsync(playerBestObject.Mods, (float) playerBestObject.Accuracy);

            string topPlayString =
                $"#{num}: ▸ **{OsuBase.OsuGrade(playerBestObject.Rank)}{playerBestObject.Mods.ToModeString(OsuBase.Client).Replace("No Mode", "No Mod")}** ▸ " +
                $"{beatmap.BeatmapId} ▸ **[{beatmap.Title} " +
                $"[{beatmap.Difficulty}]](https://osu.ppy.sh/b/{beatmap.BeatmapId})** " +
                $"\n▸ **☆{beatmap.StarRating:N2}** ▸ **{playerBestObject.Accuracy:F}%** " +
                $"for **{pp.Pp:F}pp** " +
                $"\n▸ [Combo: {playerBestObject.MaxCombo}x / Max: {beatmap.MaxCombo}]" +
                $"\n▸ Play made {(DateTime.Now - playerBestObject.Date.Value).Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year, precision: 3)} ago\n";

            Embed.WithDescription(topPlayString);
            await ReplyAsync(embed: Embed.Build());
        }
    }
}