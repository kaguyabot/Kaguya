using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Embed;
using Kaguya.Core.Osu;
using Kaguya.Core.Osu.Builder;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Threading.Tasks;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

#pragma warning disable

namespace Kaguya.Modules
{
    public class osuStandard : ModuleBase<ShardedCommandContext>
    {
        readonly DiscordSocketClient _client;
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("osu")]
        public async Task osuProfile([Remainder]string player = null)
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;
            string osuapikey = Config.bot.OsuApiKey;
            string jsonProfile;

            if (player == null)
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;

                if (player == null)
                {
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    embed.SetColor(EmbedColor.RED);
                    await BE();
                    return;
                }
            }
            player = player.Replace(' ', '_');

            //Getting user profile object.
            var userProfileObject = new OsuUserBuilder(player).Execute();

            if (userProfileObject == null)
            {
                embed.WithDescription($"**{Context.User.Mention} I couldn't download information for the specified user!**");
                embed.WithFooter($"If this persists, please contact Stage#0001. Error code: OAPI_RETURN_NULL");
                await BE();
                return;
            }

            //Build rich embed and send to Discord
            embed.WithAuthor(author =>
            {
                author.Url = $"https://osu.ppy.sh/u/{userProfileObject.user_id}";
                author.Name = $"osu! Profile For {userProfileObject.username}";
            });
            embed.AddField($"Performance: {userProfileObject.pp_raw.ToString("N2")}pp" +
                $"\nGlobal Rank: #{userProfileObject.pp_rank.ToString("N0")}" +
                $"\n{userProfileObject.country} Rank: #{userProfileObject.pp_country_rank.ToString("N0")}",
                $"▸ **Total Ranked Score:** `{userProfileObject.ranked_score.ToString("N0")}` points" +
                $"\n▸ **Average Hit Accuracy: ** `{(userProfileObject.accuracy / 100).ToString("P")}`" +
                $"\n▸ **Play Time:** `{userProfileObject.total_seconds_played / 3600}` Hours - That's over `{userProfileObject.total_seconds_played / 86400} Days`!" +
                $"\n▸ **Total Play Count:** `{userProfileObject.playcount.ToString("N0")}` plays" +
                $"\n▸ **Current Level:** `{userProfileObject.level.ToString("N0")}` ~ `{(int)((userProfileObject.level - (int)userProfileObject.level) * 100)}% to level {(int)userProfileObject.level + 1}`!" +
                $"\n▸ **Total Circles Clicked:** `{(userProfileObject.count300 + userProfileObject.count100 + userProfileObject.count50).ToString("N0")}`" +
                $"\n▸ {OsuMisc.OsuGrade("XH")} ~ `{userProfileObject.count_rank_ssh}` {OsuMisc.OsuGrade("X")} ~ `{userProfileObject.count_rank_ss}` {OsuMisc.OsuGrade("SH")} ~ `{userProfileObject.count_rank_sh}` {OsuMisc.OsuGrade("S")} ~ `{userProfileObject.count_rank_s}` {OsuMisc.OsuGrade("A")} ~ `{userProfileObject.count_rank_a}`" +
                $"\n▸ **{userProfileObject.username} joined `{userProfileObject.difference.TotalDays.ToString("N0")} days, {userProfileObject.difference.Hours} hours, and {userProfileObject.difference.Minutes} minutes ago.`**" +
                $"\n**`That's over {(userProfileObject.difference.TotalDays / 31).ToString("N0")} months!`**");
            embed.WithThumbnailUrl($"https://a.ppy.sh/{userProfileObject.user_id}");
            embed.WithFooter($"Stats accurate as of {DateTime.Now}");
            await BE();
        }

        [Command("osuset")] //osu
        public async Task osuSet([Remainder]string username)
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            string oldUsername = userAccount.OsuUsername;
            if (oldUsername == null)
                oldUsername = "Null";
            username = username.Replace(" ", "_");
            userAccount.OsuUsername = username;

            //Getting user profile object.
            var playerRecentObject = new OsuUserBuilder(username).Execute();

            if (playerRecentObject == null)
            {
                userAccount.OsuUsername = oldUsername;
                embed.WithDescription($"{Context.User.Mention} **ERROR: This username does not match a valid osu! username!**");
                embed.WithFooter($"I have kept your osu! username as {oldUsername}. If you believe this is a mistake, contact Stage#0001.");
                await BE();
                embed.SetColor(EmbedColor.RED);
                return;
            }

            embed.WithTitle("osu! Username Set");
            embed.WithDescription($"{Context.User.Mention} **Your new username has been set! Changed from `{oldUsername}` to `{userAccount.OsuUsername}`.**");
            await BE();
            
        }

        [Command("recent")] //osu
        [Alias("r")]
        public async Task osuRecent(string player = null)
        {

            string cmdPrefix = Servers.GetServer(Context.Guild).CommandPrefix;
            string osuapikey = Config.bot.OsuApiKey;

            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
                if (player == null || player == "")
                {
                    embed.WithTitle("osu! Recent");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    await BE(); return;
                }
            }

            //Getting recent object.
            var playerRecentObjectList = new OsuRecentBuilder(player).Execute();

            if (playerRecentObjectList.Count == 0)
            {
                //Getting user profile object.
                var userProfileObject = new OsuUserBuilder(player).Execute();
                
                //If user is null send no download data error, else send no recently plays found.
                if (userProfileObject == null)
                {
                    embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                    await BE();
                    embed.SetColor(EmbedColor.RED);
                }
                else
                {
                    embed.WithAuthor(author =>
                    {
                        author
                            .WithName("" + userProfileObject.username + " hasn't got any recent plays")
                            .WithIconUrl("https://a.ppy.sh/" + userProfileObject.user_id);
                    });
                    await BE();
                }
            }
            else
            {
                string playerRecentString = "";
                DateTime date = new DateTime();
                int totalcount = playerRecentObjectList.Count;

                foreach (var playerRecentObject in playerRecentObjectList)
                {
                    playerRecentString += $"▸ **{playerRecentObject.rankemote}{playerRecentObject.string_mods}** ▸ **[{playerRecentObject.beatmap.title} [{playerRecentObject.beatmap.version}]](https://osu.ppy.sh/b/{playerRecentObject.beatmap_id})** by **{playerRecentObject.beatmap.artist}**\n" +
                        $"▸ **☆{playerRecentObject.beatmap.difficultyrating.ToString("F")}** ▸ **{playerRecentObject.accuracy.ToString("F")}%**\n" +
                        $"▸ **Combo:** `{playerRecentObject.maxcombo.ToString("N0")}x / {playerRecentObject.beatmap.max_combo.ToString("N0")}x`\n" +
                        $"▸ [300 / 100 / 50 / X]: `[{playerRecentObject.count300} / {playerRecentObject.count100} / {playerRecentObject.count50} / {playerRecentObject.countmiss}]`\n" +
                        $"▸ **Map Completion:** `{playerRecentObject.completion}%`\n" +
                        $"▸ **Full Combo Percentage:** `{((playerRecentObject.maxcombo / playerRecentObject.beatmap.max_combo) * 100).ToString("N2")}%`\n";

                    if (playerRecentObject == playerRecentObjectList[totalcount - 1])
                    {
                        playerRecentString += $"▸ **PP for FC**: `{playerRecentObject.fullcombopp.ToString("N0")}pp`";
                    }
                    else
                    {
                        playerRecentString += $"▸ **PP for FC**: `{playerRecentObject.fullcombopp.ToString("N0")}pp`\n";
                    }

                    date = playerRecentObject.date;
                }

                //Getting user profile object.
                var userProfileObject = new OsuUserBuilder(player).Execute();

                string footer = "";
                var difference = DateTime.UtcNow - date;

                if (playerRecentObjectList.Count > 1)
                {
                    footer = $"{userProfileObject.username} performed this plays {(int)difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.";
                }
                else
                {
                    footer = $"{userProfileObject.username} performed this play {(int)difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.";
                }

                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + userProfileObject.username)
                        .WithIconUrl("https://a.ppy.sh/" + userProfileObject.user_id);
                });
                embed.WithDescription($"{playerRecentString}");
                embed.WithFooter(footer);
                await BE();
            }
        }

        [Flags]
        public enum AllMods
        {
            NM = 0,
            NF = (1 << 0),
            EZ = (1 << 1),
            //TouchDevice = (1 << 2),
            HD = (1 << 3),
            HR = (1 << 4),
            SD = (1 << 5),
            DT = (1 << 6),
            //Relax = (1 << 7),
            HT = (1 << 8),
            NC = (1 << 9), // Only set along with DoubleTime. i.e: NC only gives 576
            FL = (1 << 10),
            // Autoplay = (1 << 11),
            SO = (1 << 12),
            // Relax2 = (1 << 13),  // Autopilot
            PF = (1 << 14),
        }
    }
}
