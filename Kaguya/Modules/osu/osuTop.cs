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
using Kaguya.Core.Commands;
using OppaiSharp;
using Kaguya.Core;
using System.Diagnostics;
using static Kaguya.Modules.osuStandard;
using Discord.Addons.Interactive;
using Kaguya.Core.Command_Handler.EmbedHandlers;

namespace Kaguya.Modules.osu
{
    [Group("osutop")]
    public class osuTop : InteractiveBase<ShardedCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        readonly Color Pink = new Color(252, 132, 255);
        readonly Color Red = new Color(255, 0, 0);
        readonly Color Gold = new Color(255, 223, 0);
        readonly string version = Utilities.GetAlert("VERSION");
        readonly string botToken = Config.bot.Token;
        readonly string osuapikey = Config.bot.OsuApiKey;
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command()]
        public async Task TopOsuPlays(int num = 5, [Remainder]string player = null)
        {
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            if (num.ToString().Count() > 2)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Failed to parse number! Numbers must be between 1 and 10!** ");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "Failed to parse Int32");
            }
            if (num < 1 || num > 10)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Number for top plays must be between 1 and 10!** ");
                embed.WithColor(Red);
                await BE(); return;
            }

            if (player == null || player == "")
            {
                player = UserAccounts.GetAccount(Context.User).OsuUsername;
                if (player == null || player == "")
                {
                    embed.WithTitle($"osu! Top {num}");
                    embed.WithDescription($"**{Context.User.Mention} Failed to acquire username! Please specify a player or set your osu! username with `{cmdPrefix}osuset`!**");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "Failed to acquire username."); return;
                }
            }

            player.Replace(' ', '_');

            string osuapikey = Config.bot.OsuApiKey;

            if (num > 10)
            {
                embed.WithDescription($"{Context.User.Mention} You may not request more than 10 top plays.");
                embed.WithColor(Red);
                stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User attempted to request more than 10 top plays.");
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

                //Create StreamReader for OppaiSharp beatmap parser

                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{mapID}");
                var stream = new MemoryStream(data, false);
                var reader = new StreamReader(stream);

                //Read the beatmap
                var beatmap = Beatmap.Read(reader);

                //Gets the map's enabled mods and turns it into a string (referencing from Kaguya.Modules.osuStandard.AllMods).
                //modnum here is directly from the osu!API.

                var modnum = playerTopObject.enabled_mods;
                string mods = ((AllMods)modnum).ToString().Replace(",", "");
                mods = mods.Replace(" ", "");
                mods = mods.Replace("NM", "");
                mods = mods.Replace("576", "NC");
                mods = mods.Replace("DTNC", "NC");

                var enabledMods = Mods.NoMod; //Default enabled mod.

                /*If any of the possibly enabled mods (excluding sudden death/perfect) are enabled, 
                add it to the enum enabledMods that we use to calculate the map's star rating.*/

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

                //API Data on the player we're getting the top plays for.

                double pp = playerTopObject.pp;
                string mapTitle = mapObject.title;
                DiffCalc difficultyRating = new DiffCalc().Calc(beatmap, enabledMods);
                string version = mapObject.version;
                double count300 = playerTopObject.count300;
                double count100 = playerTopObject.count100;
                double count50 = playerTopObject.count50;
                double countMiss = playerTopObject.countmiss;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                double playerMaxCombo = playerTopObject.maxcombo;
                double mapMaxCombo = mapObject.max_combo;
                string grade = playerTopObject.rank;
                DateTime date = playerTopObject.date;
                switch (grade)
                {
                    //Switching grade earned on map and turning it into an emoji to be sent into Discord. This helps make the embed look pretty.

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

                PlayData PlayData = new PlayData(mapTitle, mapID, pp, difficultyRating, version, count300, count100, count50, countMiss, accuracy, grade, playerMaxCombo, mapMaxCombo, mods, date);
                PlayDataArray[i] = PlayData;
            }

            //Download user's information from the osu! API.

            string jsonPlayer = "";
            using (WebClient client = new WebClient())
            {
                jsonPlayer = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}");
            }

            //If the API doesn't return anything, send a response in chat letting the user know what happened.

            if (jsonPlayer == "[]")
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "osu! API did not return any data for the given username."); return;
            }

            var playerObject = JsonConvert.DeserializeObject<dynamic>(jsonPlayer)[0];
            string username = playerObject.username;
            string playerID = playerObject.user_id;
            string playerCountry = playerObject.country;
            string TopPlayString = ""; //Country images to come later.
            for (var j = 0; j < num; j++)
            {
                string plus = "+"; //This is in its own variable so that in the case of there not being any mods, we can null it out (helps with embed formatting).
                char[] splitter = { 's' };
                double.TryParse(PlayDataArray[j].difficultyRating.ToString().Split(splitter)[0], out double starRating); //Give us a starRating variable that we can then format into a number with two decimals.

                if (plus == "+" && PlayDataArray[j].mods == "")
                    plus = "";
                TopPlayString = TopPlayString + $"\n{j + 1}: ▸ **{PlayDataArray[j].grade}{plus}{PlayDataArray[j].mods}** ▸ {PlayDataArray[j].mapID} ▸ **[{PlayDataArray[j].mapTitle} [{PlayDataArray[j].version}]](https://osu.ppy.sh/b/{PlayDataArray[j].mapID})** " +
                    $"\n▸ **☆{starRating.ToString("N2")}** ▸ **{PlayDataArray[j].accuracy.ToString("F")}%** for **{PlayDataArray[j].pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {PlayDataArray[j].playerMaxCombo}x / Max: {PlayDataArray[j].mapMaxCombo}]\n";
            }

            //Code to build embedded message that is then sent into chat.

            embed.WithAuthor(author =>
            {
                author.Name = $"{username}'s Top {num} osu! Standard Plays";
                author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerCountry}.png";
            });

            stopWatch.Stop();
            await GlobalCommandResponses.CreateCommandResponse(Context,
                stopWatch.ElapsedMilliseconds,
                $"**Top {num} osu! standard plays for {username}:**",
                $"osu! Stats for player **{username}**:\n" + TopPlayString,
                thumbnailURL: $"https://osu.ppy.sh/u/{playerID}");

        }

        [Command("-n")] //osutop extension for a specific top play. Almost the exact same thing as osutop.
        public async Task SpecificOsuTopPlay(int num, [Remainder]string player = null)
        {
            stopWatch.Start();

            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            if (num.ToString().Count() > 3)
            {
                stopWatch.Stop();
                await GlobalCommandResponses.CreateCommandError(Context,
                    stopWatch.ElapsedMilliseconds,
                    CommandError.Unsuccessful,
                    "User's number was greater than 3 digits.",
                    description: $"{Context.User.Mention} **ERROR: Failed to parse number! The number must be between 1 and 100!**");
                return;
            }
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

            string osuapikey = Config.bot.OsuApiKey;
            string jsonTop = "";

            using (WebClient client = new WebClient())
            {
                jsonTop = client.DownloadString($"https://osu.ppy.sh/api/get_user_best?k={osuapikey}&u=" + player + "&limit=" + num); //Downloads all user top plays.
            }
            PlayData[] PlayDataArray = new PlayData[num];

            for (var i = num - 1; i < num; i++)
            {
                var playerTopObject = JsonConvert.DeserializeObject<dynamic>(jsonTop)[i];
                string jsonMap = "";

                string mapID = playerTopObject.beatmap_id.ToString();
                using (WebClient client = new WebClient())
                {
                    jsonMap = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={osuapikey}&b=" + mapID);
                }

                var mapObject = JsonConvert.DeserializeObject<dynamic>(jsonMap)[0];

                //Create StreamReader for OppaiSharp beatmap parser

                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{mapID}");
                var stream = new MemoryStream(data, false);
                var reader = new StreamReader(stream);

                //Read the beatmap
                var beatmap = Beatmap.Read(reader);

                //Gets the map's enabled mods and turns it into a string (referencing from Kaguya.Modules.osuStandard.AllMods).
                //modnum here is directly from the osu!API.

                var modnum = playerTopObject.enabled_mods;
                string mods = ((AllMods)modnum).ToString().Replace(",", "");
                mods = mods.Replace(" ", "");
                mods = mods.Replace("NM", "");
                mods = mods.Replace("576", "NC");
                mods = mods.Replace("DTNC", "NC");

                var enabledMods = Mods.NoMod; //Default enabled mod.

                /*If any of the possibly enabled mods (excluding sudden death/perfect) are enabled, 
                add it to the enum enabledMods that we use to calculate the map's star rating.*/

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

                //API Data on the player we're getting the top plays for.

                double pp = playerTopObject.pp;
                string mapTitle = mapObject.title;
                DiffCalc difficultyRating = new DiffCalc().Calc(beatmap, enabledMods);
                string version = mapObject.version;
                double count300 = playerTopObject.count300;
                double count100 = playerTopObject.count100;
                double count50 = playerTopObject.count50;
                double countMiss = playerTopObject.countmiss;
                double accuracy = 100 * ((50 * count50) + (100 * count100) + (300 * count300)) / ((300 * (countMiss + count50 + count100 + count300)));
                double playerMaxCombo = playerTopObject.maxcombo;
                double mapMaxCombo = mapObject.max_combo;
                string grade = playerTopObject.rank;
                DateTime date = playerTopObject.date;
                switch (grade)
                {
                    //Switching grade earned on map and turning it into an emoji to be sent into Discord. This helps make the embed look pretty.

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

                PlayData PlayData = new PlayData(mapTitle, mapID, pp, difficultyRating, version, count300, count100, count50, countMiss, accuracy, grade, playerMaxCombo, mapMaxCombo, mods, date);
                PlayDataArray[num - 1] = PlayData;

                //Download user's information from the osu! API.

                string jsonPlayer = "";
                using (WebClient client = new WebClient())
                {
                    jsonPlayer = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}");
                }

                //If the API doesn't return anything, send a response in chat letting the user know what happened.

                if (jsonPlayer == "[]")
                {
                    embed.WithDescription($"{Context.User.Mention} **ERROR: Could not download data for {player}!**");
                    embed.WithColor(Red);
                    await BE(); stopWatch.Stop();
                    logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "osu! API did not return any data for the given username."); return;
                }

                var playerObject = JsonConvert.DeserializeObject<dynamic>(jsonPlayer)[0];
                string username = playerObject.username;
                string playerID = playerObject.user_id;
                string playerCountry = playerObject.country;
                string TopPlayString = "";
                string plus = "+"; //This is in its own variable so that in the case of there not being any mods, we can null it out (helps with embed formatting).
                char[] splitter = { 's' };


                double.TryParse(PlayDataArray[num - 1].difficultyRating.ToString().Split(splitter)[0], out double starRating); //Give us a starRating variable that we can then format into a num - 1ber with two decimals.

                if (plus == "+" && PlayDataArray[num - 1].mods == "")
                    plus = "";

                TopPlayString = TopPlayString + $"\n{num}: ▸ **{PlayDataArray[num - 1].grade}{plus}{PlayDataArray[num - 1].mods}** ▸ {PlayDataArray[num - 1].mapID} ▸ **[{PlayDataArray[num - 1].mapTitle} [{PlayDataArray[num - 1].version}]](https://osu.ppy.sh/b/{PlayDataArray[num - 1].mapID})** " +
                    $"\n▸ **☆{starRating.ToString("N2")}** ▸ **{PlayDataArray[num - 1].accuracy.ToString("F")}%** for **{PlayDataArray[num - 1].pp.ToString("F")}pp** " +
                    $"\n▸ [Combo: {PlayDataArray[num - 1].playerMaxCombo}x / Max: {PlayDataArray[num - 1].mapMaxCombo}]\n";

                //Code to build embedded message that is then sent into chat.

                embed.WithAuthor(author =>
                {
                    author.Name = $"{username}'s Top {num} osu! Standard Play";
                    author.IconUrl = $"https://osu.ppy.sh/images/flags/{playerCountry}.png";
                });

                embed.WithTitle($"**Top #{num} play for {username}:**");
                embed.WithUrl($"https://osu.ppy.sh/u/{playerID}");
                embed.WithDescription($"{TopPlayString}");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
        }
    }
}
