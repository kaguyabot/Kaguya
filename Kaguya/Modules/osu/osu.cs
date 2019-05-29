using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.UserAccounts;
using System.Net;
using Kaguya.Core.Server_Files;
using OppaiSharp;
using Kaguya.Core.Embed;
using EmbedType = Kaguya.Core.Embed.EmbedType;

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
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            string osuapikey = Config.bot.OsuApiKey;
            string jsonProfile;

            if (player == null)
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;

                if (player == null)
                {
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    await BE();
                    // logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "No osu! username specified."); ERROR HANDLER HERE
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
                //logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "osu! API Returned Null"); ERROR HANDLER HERE
                return;
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
                $"\n▸ **{username} joined `{difference.TotalDays.ToString("N0")} days, {difference.Hours} hours, {difference.Minutes} minutes, and {difference.Seconds} seconds ago.`**" +
                $"\n**`That's over {(difference.TotalDays / 31).ToString("N0")} months!`**");
            embed.WithThumbnailUrl($"https://a.ppy.sh/{userID}");
            embed.WithFooter($"Stats accurate as of {DateTime.Now}");
            embed.EmbedType = EmbedType.PINK;
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
                //logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "osu! API did not return any data for the given username."); ERROR HANDLER HERE
                return;
            }
            UserAccounts.SaveAccounts();

            embed.WithTitle("osu! Username Set");
            embed.WithDescription($"{Context.User.Mention} **Your new username has been set! Changed from `{oldUsername}` to `{userAccount.OsuUsername}`.**");
            embed.EmbedType = EmbedType.PINK;
            await BE();
            
        }

        [Command("recent")] //osu
        [Alias("r")]
        public async Task osuRecent(string player = null)
        {
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
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
                jsonRecent = client.DownloadString($"https://osu.ppy.sh/api/get_user_recent?k={osuapikey}&u=" + player);
            }
            if (jsonRecent == "[]")
            {
                string jsonUserData;
                using (WebClient client = new WebClient())
                {
                    jsonUserData = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u=" + player);
                }

                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(jsonUserData)[0];
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("" + mapUserNameObject.username + " hasn't got any recent plays")
                        .WithIconUrl("https://a.ppy.sh/" + mapUserNameObject.user_id);
                });
                embed.EmbedType = EmbedType.PINK;
                await BE();
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
                double maxCombo = playerRecentObject.maxcombo;
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
                double maxPossibleCombo = mapRecentObject.max_combo;
                var modnum = playerRecentObject.enabled_mods;
                mods = ((AllMods)modnum).ToString().Replace(",", "");
                mods = mods.Replace(" ", "");
                mods = mods.Replace("NM", "");
                string date = playerRecentObject.date;
                double starRating = mapRecentObject.difficultyrating;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                string grade = playerRecentObject.rank;

                //Emote codes for grade icons

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

                if(NormalUserName == "[]")
                {
                    embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                    await BE();
                    //logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "osu! API did not return any data for the given username."); ERROR HANDLER HERE
                    return;
                }

                var mapUserNameObject = JsonConvert.DeserializeObject<dynamic>(NormalUserName)[0];

                //PPv2

                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{mapID}");
                var stream = new MemoryStream(data, false);
                var reader = new StreamReader(stream);
                var enabledMods = Mods.NoMod;

                if (mods.Contains("EZ"))
                    enabledMods |= Mods.Easy;
                if (mods.Contains("HD"))
                    enabledMods |= Mods.Hidden;
                if (mods.Contains("HR"))
                    enabledMods |= Mods.Hardrock;
                if (mods.Contains("FL"))
                    enabledMods |= Mods.Flashlight;
                if (mods.Contains("DT") || mods.Contains("NC"))
                    enabledMods |= Mods.DoubleTime;
                if (mods.Contains("NF"))
                    enabledMods |= Mods.NoFail;
                if (mods.Contains("HT"))
                    enabledMods |= Mods.HalfTime;

                var beatmap = Beatmap.Read(reader);
                var diff = new DiffCalc().Calc(beatmap, mods: enabledMods);
                var fullComboPP = new PPv2(new PPv2Parameters(beatmap, diff, accuracy: (accuracy / 100), mods: enabledMods));

                //PPv2 End

                string plus = "+";

                var objectsEncountered = (count300 + count100 + count50 + countMiss);
                var mapCompletion = ((objectsEncountered / beatmap.Objects.Count()) * 100).ToString("N2");

                if (plus == "+" && mods == "")
                    plus = "";
                mods = mods.Replace("576", "NC");
                string playerRecentString = $"▸ **{grade}{plus}{mods}** ▸ **[{mapTitle} [{difficulty}]](https://osu.ppy.sh/b/{mapID})** by **{artist}**\n" +
                    $"▸ **☆{starRating.ToString("F")}** ▸ **{accuracy.ToString("F")}%**\n" +
                    $"▸ **Combo:** `{maxCombo.ToString("N0")}x / {maxPossibleCombo.ToString("N0")}x`\n" +
                    $"▸ [300 / 100 / 50 / X]: `[{count300} / {count100} / {count50} / {countMiss}]`\n" +
                    $"▸ **Map Completion:** `{mapCompletion}`%\n" +
                    $"▸ **Full Combo Percentage:** `{((maxCombo / maxPossibleCombo) * 100).ToString("N2")}%`\n" +
                    $"▸ **PP for FC**: `{fullComboPP.Total.ToString("N0")}pp`";

                var difference = DateTime.UtcNow - (DateTime)playerRecentObject.date;

                string footer = $"{mapUserNameObject.username} performed this play {(int)difference.TotalHours} hours {difference.Minutes} minutes and {difference.Seconds} seconds ago.";

                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"Most Recent osu! Standard Play for " + mapUserNameObject.username)
                        .WithIconUrl("https://a.ppy.sh/" + playerRecentObject.user_id);
                });
                embed.WithDescription($"{playerRecentString}");
                embed.WithFooter(footer);
                embed.EmbedType = EmbedType.PINK;
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
            embed.EmbedType = EmbedType.PINK;
            await BE();
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
            embed.EmbedType = EmbedType.PINK;
            foreach (IRole role in roles)
            {
                if (role.Name.Contains("Team: "))
                {
                    await role.DeleteAsync();
                    embed.WithDescription(embed.Description.ToString() + $"\n`{role}`");
                }
            }
            await BE();
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
