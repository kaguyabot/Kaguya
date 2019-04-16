using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.UserAccounts;
using System.Net;
using System.Timers;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;

#pragma warning disable

namespace Kaguya.Modules
{
    public class osu : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.token;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

       /* [Command("osu")]
        public async Task osuProfile([Remainder]string player = null)
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            string osuapikey = Config.bot.osuapikey;
            string jsonProfile;

            if (player == null)
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;

                if (player == null)
                {
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    embed.WithColor(Red);
                    BE(); return;
                }
            }

            player = player.Replace(' ', '_');

            using (WebClient client = new WebClient())
            {
                jsonProfile = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}"); //Downloads user data
            }

            var userProfileObject = JsonConvert.DeserializeObject<dynamic>(jsonProfile)[0];

            string userID = userProfileObject.user_id;
            string username = userProfileObject.username;
            DateTime joinDate = userProfileObject.join_date; //May throw an error, idk
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
            embed.WithTitle($"Statistics");
            embed.AddField($"Performance: {pp}pp  Global Rank: #{globalRank}   Country Rank: #{countryRank}",
                $"▸ **Total Ranked Score:** `{rankedScore.ToString("N0")}` points" +
                $"\n▸ **Average Hit Accuracy: ** `{(accuracy / 100).ToString("P")}`" +
                $"\n▸ **Play Time:** `{totalSecondsPlayed / 3600}` Hours - That's over `{totalSecondsPlayed / 86400} Days`!" +
                $"\n▸ **Total Play Count:** `{playcount.ToString("N0")}` plays" +
                $"\n▸ **Current Level:** `{level.ToString("N0")}` ~ `{(int)((level - (int)level) * 100)}%` to level {(int)level + 1}!" +
                $"\n▸ **Total Circles Clicked:** `{(count300 + count100 + count50).ToString("N0")}`" +
                $"\n▸ {gradeSSH} ~ `{countSSH}` {gradeSS} ~ `{countSS}` {gradeSH} ~ `{countSH}` {gradeS} ~ `{countS}` {gradeA} ~ `{countA}`" +
                $"\n▸ **Average play value:** `{(pp / 100).ToString("N0")}`pp");
            embed.WithThumbnailUrl($"https://a.ppy.sh/{userID}");
            embed.WithFooter($"Stats accurate as of {DateTime.Now}");
            embed.WithColor(Pink);
            BE();
        }
        */
        [Command("osuset")] //osu
        public async Task osuSet([Remainder]string username)
        {
            var userAccount = UserAccounts.GetAccount(Context.User);
            string oldUsername = userAccount.OsuUsername;
            if (oldUsername == null)
                oldUsername = "Null";
            userAccount.OsuUsername = username.Replace(" ", "_");
            UserAccounts.SaveAccounts();

            embed.WithTitle("osu! Username Set");
            embed.WithDescription($"{Context.User.Mention} **Your new username has been set! Changed from `{oldUsername}` to `{userAccount.OsuUsername}`.**");
            embed.WithFooter("Ensure your username is spelled properly, otherwise all osu! related commands will not work for you!");
            embed.WithColor(Pink);
            BE();
        }

        [Command("recent")] //osu
        [Alias("r")]
        public async Task osuRecent(string player = null, int mode = 0)
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            string osuapikey = Config.bot.osuapikey;

            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
                if (player == null || player == "")
                {
                    embed.WithTitle("osu! Recent");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    embed.WithColor(Red);
                    BE(); return;
                }
            }

            if(mode != 0)
            {
                embed.WithTitle("osu! Recent");
                embed.WithDescription($"**{Context.User.Mention} I'm sorry, but I don't have support for modes other than osu! Standard yet :(**");
                embed.WithColor(Red);
                BE(); return;
            }

            string jsonRecent;

            using (WebClient client = new WebClient())
            {
                jsonRecent = client.DownloadString($"https://osu.ppy.sh/api/get_user_recent?k={osuapikey}&u=" + player);
            }
            if (jsonRecent == "[]")
            {
                string NormalUserName;
                using (WebClient client = new WebClient())
                {
                    NormalUserName = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u=" + player);
                }

                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(NormalUserName)[0];
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("" + mapUserNameObject.username + " hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + mapUserNameObject.user_id);
                });
                embed.WithColor(Pink);
                BE();
            }
            else
            {
                var playerRecentObject = JsonConvert.DeserializeObject<dynamic>(jsonRecent)[0];
                string mapID = playerRecentObject.beatmap_id;

                string mapRecent = "";
                using (WebClient client = new WebClient())
                {
                    mapRecent = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={osuapikey}&b={mapID}");
                }
                var mapRecentObject = JsonConvert.DeserializeObject<dynamic>(mapRecent)[0];

                string mapTitle = mapRecentObject.title;
                string difficulty = mapRecentObject.version;
                string score = playerRecentObject.score;
                string maxCombo = playerRecentObject.maxcombo;
                string artist = mapRecentObject.artist;
                double count50 = playerRecentObject.count50;
                double count100 = playerRecentObject.count100;
                double count300 = playerRecentObject.count300;
                double countMiss = playerRecentObject.countmiss;
                string fullCombo = playerRecentObject.perfect;
                if (fullCombo == "1")
                    fullCombo = " **Full Combo!**";
                else fullCombo = null;
                string mods = playerRecentObject.enabled_mods;
                string maxPossibleCombo = mapRecentObject.max_combo;
                var modnum = playerRecentObject.enabled_mods;
                mods = ((AllMods)modnum).ToString().Replace(",", "");
                mods = mods.Replace(" ", "");
                mods = mods.Replace("NM", "");
                string date = playerRecentObject.date;
                double starRating = mapRecentObject.difficultyrating;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                string grade = playerRecentObject.rank;
                switch (grade)
                {
                    case "XH":
                        grade = "<:XH:553119188089176074>"; break;
                    case "X":
                        grade = "<:X_:553119217109565470>"; break;
                    case "SH":
                        grade = "<:SH:553119233463025691>"; break;
                    case "S":
                        grade = "<:S_:553119252329267240>"; break;
                    case "A":
                        grade = "<:A_:553119274256826406>"; break;
                    case "B":
                        grade = "<:B_:553119304925577228>"; break;
                    case "C":
                        grade = "<:C_:553119325565878272>"; break;
                    case "D":
                        grade = "<:D_:553119338035675138>"; break;
                    case "F":
                        grade = "<:F_:557297028263051288>"; break;
                }

                string NormalUserName = "";
                using (WebClient client = new WebClient())
                {
                    NormalUserName = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u=" + player);
                }
                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(NormalUserName)[0];

                string plus = "+";
                if (plus == "+" && mods == "")
                    plus = "";
                mods = mods.Replace("576", "NC");
                string playerRecentString = $"▸ **{grade}{plus}{mods}** ▸ **[{mapTitle} [{difficulty}]](https://osu.ppy.sh/b/{mapID})** by ***{artist}***\n" +
                    $"▸ **☆{starRating.ToString("F")}** ▸ **{accuracy.ToString("F")}%**\n" +
                    $"▸ [Combo: {maxCombo}x / Max: {maxPossibleCombo}] {fullCombo}\n" +
                    $"▸ [{count300} / {count100} / {count50} / {countMiss}]";

                var difference = DateTime.UtcNow - (DateTime)playerRecentObject.date;

                string footer = $"{mapUserNameObject.username} preformed this play {difference.Days} days {difference.Hours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.";

                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + mapUserNameObject.username)
                        .WithIconUrl("https://a.ppy.sh/" + playerRecentObject.user_id);
                });
                embed.WithDescription($"{playerRecentString}");
                embed.WithFooter(footer);
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("osutop")] //osu
        public async Task osuTop(int num = 5, string player = null)
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
                if (player == null || player == "")
                {
                    embed.WithTitle($"osu! Top {num}");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    embed.WithColor(Red);
                    BE(); return;
                }
            }

            player.Replace(' ', '_');

            string osuapikey = Config.bot.osuapikey;

            if (num > 10)
            {
                embed.WithDescription($"{Context.User.Mention} You may not request more than 10 top plays.");
                return;
            }
            string jsonTop = "";
            using (WebClient client = new WebClient())
            {
                jsonTop = client.DownloadString($"https://osu.ppy.sh/api/get_user_best?k={osuapikey}&u=" + player + "&limit=" + num);
            }
            PlayData[] PlayDataArray = new PlayData[num];

            for (var i = 0; i < num; i++)
            {
                var playerTopObject = JsonConvert.DeserializeObject<dynamic>(jsonTop)[i];
                string jsonMap = "";

                string mapID = playerTopObject.beatmap_id.ToString();
                using (WebClient client = new WebClient())
                {
                    jsonMap = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={osuapikey}&b=" + mapID);
                }
                var mapObject = JsonConvert.DeserializeObject<dynamic>(jsonMap)[0];
                double pp = playerTopObject.pp;
                string mapTitle = mapObject.title;
                double difficultyRating = mapObject.difficultyrating;
                string version = mapObject.version;
                string country = playerTopObject.country;
                double count300 = playerTopObject.count300;
                double count100 = playerTopObject.count100;
                double count50 = playerTopObject.count50;
                double countMiss = playerTopObject.countmiss;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                double playerMaxCombo = playerTopObject.maxcombo;
                double mapMaxCombo = mapObject.max_combo;
                string grade = playerTopObject.rank;
                switch (grade)
                {
                    case "XH":
                        grade = "<:XH:553119188089176074>"; break;
                    case "X":
                        grade = "<:X_:553119217109565470>"; break;
                    case "SH":
                        grade = "<:SH:553119233463025691>"; break;
                    case "S":
                        grade = "<:S_:553119252329267240>"; break;
                    case "A":
                        grade = "<:A_:553119274256826406>"; break;
                    case "B":
                        grade = "<:B_:553119304925577228>"; break;
                    case "C":
                        grade = "<:C_:553119325565878272>"; break;
                    case "D":
                        grade = "<:D_:553119338035675138>"; break;
                }

                var modnum = playerTopObject.enabled_mods;
                string mods = ((AllMods)modnum).ToString().Replace(",", "");
                mods = mods.Replace(" ", "");
                mods = mods.Replace("NM", "");
                mods = mods.Replace("576", "NC");
                mods = mods.Replace("DTNC", "NC");


                PlayData PlayData = new PlayData(mapTitle, mapID, pp, difficultyRating, version, country, count300, count100, count50, countMiss, accuracy, grade, playerMaxCombo, mapMaxCombo, mods);
                PlayDataArray[i] = PlayData;
            }

            string jsonPlayer = "";
            using (WebClient client = new WebClient())
            {
                jsonPlayer = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}");
            }

            var playerObject = JsonConvert.DeserializeObject<dynamic>(jsonPlayer)[0];
            string username = playerObject.username;
            string playerID = playerObject.user_id;
            string TopPlayString = ""; //Country images to come later.
            for (var j = 0; j < num; j++)
            {
                string plus = "+";

                if (plus == "+" && PlayDataArray[j].mods == "")
                    plus = "";
                TopPlayString = TopPlayString + $"\n{j + 1}: ▸ **{PlayDataArray[j].grade}{plus}{PlayDataArray[j].mods}** ▸ {PlayDataArray[j].mapID} ▸ **[{PlayDataArray[j].mapTitle} [{PlayDataArray[j].version}]](https://osu.ppy.sh/b/{PlayDataArray[j].mapID})** " +
                    $"\n▸ **☆{PlayDataArray[j].difficultyRating.ToString("F")}** ▸ **{PlayDataArray[j].accuracy.ToString("F")}%** for **{PlayDataArray[j].pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {PlayDataArray[j].playerMaxCombo}x / Max: {PlayDataArray[j].mapMaxCombo}]\n";
            }
            embed.WithAuthor($"{username}'s Top osu! Standard Plays");
            embed.WithTitle($"**Top {num} osu! standard plays for {username}:**");
            embed.WithUrl($"https://osu.ppy.sh/u/{playerID}");
            embed.WithDescription($"osu! Stats for player **{username}**:\n" + TopPlayString);
            embed.WithColor(Pink);
            BE();
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

        [Command("createteamrole")] //osu
        [Alias("ctr")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task CreateTeamRoles(string teamName, [Remainder]List<SocketGuildUser> users)
        {
            var participantRole = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "participant" || x.Name == "Participant");
            var roleName = "Team: " + teamName;
            var teamRole = await Context.Guild.CreateRoleAsync(roleName);
            foreach (var user in users)
            {
                await user.AddRoleAsync(teamRole);
                await user.AddRoleAsync(participantRole);
                embed.AddField("Participant Added", $"**{user}** has been added to {teamRole.Mention} and {participantRole.Mention}.");
            }
            embed.WithColor(Pink);
            BE();
        }

        [Command("delteams")] //osu
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireOwner]
        public async Task DeleteTeams()
        {
            var roles = Context.Guild.Roles;
            embed.WithTitle("Teams Deleted");
            embed.WithDescription("The following teams have been deleted: ");
            embed.WithColor(Pink);
            foreach (IRole role in roles)
            {
                if (role.Name.Contains("Team: "))
                {
                    role.DeleteAsync();
                    embed.WithDescription(embed.Description.ToString() + $"\n`{role}`");
                }
            }
            BE();
        }

        [Command("sttreflog")] //Secret STT Only Cmd owo
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task STTRefLog(string WinnerTeam, string LoserTeam, string WinnerTeamScore, string LoserTeamScore, string Team1BanMod1,
            string Team1Ban1, string Team1BanMod2, string Team1Ban2, string Team2BanMod1, string Team2Ban1, string Team2BanMod2, string Team2Ban2, string MPLink)
        {
            if(Context.Guild.Id != 461347676148072448)
            {
                embed.WithDescription($"**{Context.User.Mention} I'm sorry, but this command can only be executed inside of the Spring Tranquility " +
                    $"osu! Tournament server!**");
                embed.WithColor(Red);
                BE(); return;
            }
            ISocketMessageChannel channel = (ISocketMessageChannel)Context.Guild.GetChannel(554453952125599745);
           
            embed.WithTitle($"STT2 Match Result: **{WinnerTeam}** Vs. {LoserTeam}");
            embed.WithDescription($"**{WinnerTeam}** has defeated {LoserTeam}!" +
                $"\n**Score:** **{WinnerTeamScore}** - {LoserTeamScore}" +
                $"\n**Bans:**");
            embed.AddField($"{WinnerTeam}", $"{Team1BanMod1}: {Team1Ban1}", false);
            embed.AddField($"{WinnerTeam}", $"{Team1BanMod2}: {Team1Ban2}", false);
            embed.AddField($"{LoserTeam}", $"{Team2BanMod1}: {Team2Ban1}", false);
            embed.AddField($"{LoserTeam}", $"{Team2BanMod2}: {Team2Ban2}", false);
            embed.WithUrl($"{MPLink}");
            embed.WithColor(Pink);
            await channel.SendMessageAsync("", false, embed.Build());
        }

        private bool UserIsAdmin(SocketGuildUser user)
        {
            string targetRoleName = "Administrator";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
    }
    
}
