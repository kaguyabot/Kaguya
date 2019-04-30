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

namespace Kaguya.Modules.osu
{
    [Group("osutop")]
    public class osuGroup : ModuleBase<SocketCommandContext>
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
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command()]
        public async Task osuTop(int num = 5, [Remainder]string player = null)
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
            if(num < 1 || num > 10)
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
                DateTime date = playerTopObject.date;
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


                PlayData PlayData = new PlayData(mapTitle, mapID, pp, difficultyRating, version, country, count300, count100, count50, countMiss, accuracy, grade, playerMaxCombo, mapMaxCombo, mods, date);
                PlayDataArray[i] = PlayData;
            }

            string jsonPlayer = "";
            using (WebClient client = new WebClient())
            {
                jsonPlayer = client.DownloadString($"https://osu.ppy.sh/api/get_user?k={osuapikey}&u={player}");
            }

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
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

    }
}
