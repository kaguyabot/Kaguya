﻿using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Embed;
using Kaguya.Core.Osu;
using Kaguya.Core.Osu.Builder;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using Newtonsoft.Json;
using OppaiSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
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

            using (WebClient client = new WebClient())
            {
                jsonProfile = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}"); //Downloads user data
            }

            if(jsonProfile == "[]")
            {
                embed.WithDescription($"**{Context.User.Mention} I couldn't download information for the specified user!**");
                embed.WithFooter($"If this persists, please contact Stage#0001. Error code: OAPI_RETURN_NULL");
                await BE();
                return;
            }

            var userProfileObject = JsonConvert.DeserializeObject<dynamic>(jsonProfile)[0];

            string userID = userProfileObject.user_id;
            string username = userProfileObject.username;
            DateTime joinDate = userProfileObject.join_date;
            uint count300 = userProfileObject.count300;
            uint count100 = userProfileObject.count100;
            uint count50 = userProfileObject.count50;
            int playcount = userProfileObject.playcount;
            ulong rankedScore = userProfileObject.ranked_score;
            int globalRank = userProfileObject.pp_rank; //Player's global rank
            double pp = userProfileObject.pp_raw; //Will display zero pp for inactive users
            double level = userProfileObject.level;
            double accuracy = userProfileObject.accuracy; //Total overall accuracy
            int countSS = userProfileObject.count_rank_ss; //Count SS's
            int countSSH = userProfileObject.count_rank_ssh; //Count Silver SS's
            int countS = userProfileObject.count_rank_s; //S's
            int countSH = userProfileObject.count_rank_sh; //Silver S's
            int countA = userProfileObject.count_rank_a;
            string country = userProfileObject.country; //Player's Country
            int totalSecondsPlayed = userProfileObject.total_seconds_played; //Playtime in seconds
            int countryRank = userProfileObject.pp_country_rank;

            var difference = DateTime.Now - joinDate;

            //Emote codes for grading letters

            string gradeSSH = "<:XH:553119188089176074>";
            string gradeSS = "<:X_:553119217109565470>";
            string gradeSH = "<:SH:553119233463025691>";
            string gradeS = "<:S_:553119252329267240>";
            string gradeA = "<:A_:553119274256826406>";

            //Build rich embed and send to Discord

            embed.WithAuthor(author =>
            {
                author.Url = $"https://osu.ppy.sh/u/{userID}";
                author.Name = $"osu! Profile For {username}";
            });
            embed.AddField($"Performance: {pp.ToString("N2")}pp" +
                $"\nGlobal Rank: #{globalRank.ToString("N0")}" +
                $"\n{country} Rank: #{countryRank.ToString("N0")}",
                $"▸ **Total Ranked Score:** `{rankedScore.ToString("N0")}` points" +
                $"\n▸ **Average Hit Accuracy: ** `{(accuracy / 100).ToString("P")}`" +
                $"\n▸ **Play Time:** `{totalSecondsPlayed / 3600}` Hours - That's over `{totalSecondsPlayed / 86400} Days`!" +
                $"\n▸ **Total Play Count:** `{playcount.ToString("N0")}` plays" +
                $"\n▸ **Current Level:** `{level.ToString("N0")}` ~ `{(int)((level - (int)level) * 100)}% to level {(int)level + 1}`!" +
                $"\n▸ **Total Circles Clicked:** `{(count300 + count100 + count50).ToString("N0")}`" +
                $"\n▸ {gradeSSH} ~ `{countSSH}` {gradeSS} ~ `{countSS}` {gradeSH} ~ `{countSH}` {gradeS} ~ `{countS}` {gradeA} ~ `{countA}`" +
                $"\n▸ **{username} joined `{difference.TotalDays.ToString("N0")} days, {difference.Hours} hours, and {difference.Minutes} minutes ago.`**" +
                $"\n**`That's over {(difference.TotalDays / 31).ToString("N0")} months!`**");
            embed.WithThumbnailUrl($"https://a.ppy.sh/{userID}");
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

            string jsonProfile;

            using (WebClient client = new WebClient())
            {
                jsonProfile = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={Config.bot.OsuApiKey}&u={username}"); //Downloads user data
            }

            if(jsonProfile == "[]")
            {
                userAccount.OsuUsername = oldUsername;
                embed.WithDescription($"{Context.User.Mention} **ERROR: This username does not match a valid osu! username!**");
                embed.WithFooter($"I have kept your osu! username as {oldUsername}. If you believe this is a mistake, contact Stage#0001.");
                await BE();
                embed.SetColor(EmbedColor.RED);
                //logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "osu! API did not return any data for the given username."); ERROR HANDLER HERE
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

            string jsonRecent;

            using (WebClient client = new WebClient())
            {
                jsonRecent = client.DownloadString($"https://osu.ppy.sh/api/get_user_recent?k={Config.bot.OsuApiKey}&u=" + player);
            }
            if (jsonRecent == "[]")
            {
                string jsonUserData;
                using (WebClient client = new WebClient())
                {
                    jsonUserData = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={Config.bot.OsuApiKey}&u=" + player);
                }

                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(jsonUserData)[0];
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("" + mapUserNameObject.username + " hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + mapUserNameObject.user_id);
                });
                await BE();
            }
            else
            {
                var playerRecentObject = new OsuRecentBuilder(player).Execute()[0];

                string NormalUserName = "";
                using (WebClient client = new WebClient())
                {
                    NormalUserName = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={Config.bot.OsuApiKey}&u=" + player);
                }

                if(NormalUserName == "[]")
                {
                    embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                    await BE();
                    embed.SetColor(EmbedColor.RED);
                    return;
                }

                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(NormalUserName)[0];

                string playerRecentString = $"▸ **{playerRecentObject.rankemote}{playerRecentObject.string_mods}** ▸ **[{playerRecentObject.beatmap.title} [{playerRecentObject.beatmap.version}]](https://osu.ppy.sh/b/{playerRecentObject.beatmap_id})** by **{playerRecentObject.beatmap.artist}**\n" +
                    $"▸ **☆{playerRecentObject.beatmap.difficultyrating.ToString("F")}** ▸ **{playerRecentObject.accuracy.ToString("F")}%**\n" +
                    $"▸ **Combo:** `{playerRecentObject.maxcombo.ToString("N0")}x / {playerRecentObject.beatmap.max_combo.ToString("N0")}x`\n" +
                    $"▸ [300 / 100 / 50 / X]: `[{playerRecentObject.count300} / {playerRecentObject.count100} / {playerRecentObject.count50} / {playerRecentObject.countmiss}]`\n" +
                    $"▸ **Map Completion:** `{playerRecentObject.completion}%`\n" +
                    $"▸ **Full Combo Percentage:** `{((playerRecentObject.maxcombo / playerRecentObject.beatmap.max_combo) * 100).ToString("N2")}%`\n" +
                    $"▸ **PP for FC**: `{playerRecentObject.fullcombopp.ToString("N0")}pp`";

                var difference = DateTime.UtcNow - playerRecentObject.date;

                string footer = $"{mapUserNameObject.username} performed this play {(int)difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.";

                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + mapUserNameObject.username)
                        .WithIconUrl("https://a.ppy.sh/" + playerRecentObject.user_id);
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
