using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using OppaiSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using Kaguya.Core;
using Kaguya.Core.Embed;
using EmbedType = Kaguya.Core.Embed.EmbedType;

namespace Kaguya.Modules.osu
{
    public class BeatmapLinkParser
    {
        public string osuapikey = Config.bot.OsuApiKey;
        public string tillerinoapikey = Config.bot.TillerinoApiKey;
        Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();

        public async Task LinkParserMethod(SocketMessage s, KaguyaEmbedBuilder embed, SocketCommandContext context)
        {
            stopWatch.Start();
            string link = $"{s}";
            string mapID = link.Split('/').Last(); //Gets the map's ID from the link.
            if (mapID.Contains('?'))
                mapID = mapID.Replace("?m=0", "");
            string jsonMapParse;
            string jsonOsuMapData;

            using (WebClient client = new WebClient())
            {
                jsonMapParse = client.DownloadString($"https://api.tillerino.org/beatmapinfo?beatmapid={mapID}&k={tillerinoapikey}");
            }

            using (WebClient client = new WebClient())
            {
                jsonOsuMapData = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={osuapikey}&b={mapID}");
            }

            var mapObject = JsonConvert.DeserializeObject<dynamic>(jsonMapParse);
            var mapData = JsonConvert.DeserializeObject<dynamic>(jsonOsuMapData)[0];

            //osu! API Data

            string mapTitle = mapData.title;
            string beatmapSetID = mapData.beatmapset_id;
            string artist = mapData.artist;
            string creator = mapData.creator;
            string creatorID = mapData.creator_id;
            string difficulty = mapData.version;
            double starRating = mapData.difficultyrating;
            double cs = mapData.diff_size;
            double od = mapData.diff_overall;
            double ar = mapData.diff_approach;
            double hp = mapData.diff_drain;
            TimeSpan length = TimeSpan.FromSeconds((int)mapData.total_length);
            double bpm = mapData.bpm;
            int maxCombo = mapData.max_combo;
            int favoriteCount = mapData.favourite_count;
            int playCount = mapData.playcount;
            int passCount = mapData.passcount;
            string status = mapData.approved;

            if (status == "-2" || status == "-1" || status == "0")
            {
                switch (status)
                {
                    case "-2":
                        status = "Graveyard";
                        break;
                    case "-1":
                        status = "Work in Progress";
                        break;
                    case "0":
                        status = "Pending";
                        break;
                }
            }
            else
            {
                DateTime approvedDate = mapData.approved_date;
                switch (status)
                {
                    case "-2":
                        status = "Graveyard"; break;
                    case "-1":
                        status = "Work in Progress"; break;
                    case "0":
                        status = "Pending"; break;
                    case "1":
                        status = $"Ranked on {approvedDate.ToShortDateString()}"; break;
                    case "2":
                        status = $"Approved on {approvedDate.ToShortDateString()}"; break;
                    case "3":
                        status = $"Qualified on {approvedDate.ToShortDateString()}"; break;
                    case "4":
                        status = $"Loved 💙 on {approvedDate.ToShortDateString()}"; break;
                }
            }

            string lengthValue = length.ToString(@"mm\:ss");

            if (status.ToLower() == "graveyard" || status.ToLower() == "work in progress" || status.ToLower() == "pending")
            {
                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{mapID}");
                var stream = new MemoryStream(data, false);
                var reader = new StreamReader(stream);
                var mods = Mods.NoMod;
                var beatmap = Beatmap.Read(reader);

                var diff = new DiffCalc().Calc(beatmap, mods: mods);

                var unranked95 = new PPv2(new PPv2Parameters(beatmap, diff, accuracy: .95, mods: mods));
                var unranked98 = new PPv2(new PPv2Parameters(beatmap, diff, accuracy: .98, mods: mods));
                var unranked99 = new PPv2(new PPv2Parameters(beatmap, diff, accuracy: .99, mods: mods));
                var unranked100 = new PPv2(new PPv2Parameters(beatmap, diff, accuracy: 1, mods: mods));

                embed.WithAuthor(author =>
                {
                    author.Name = $"{mapTitle} by {creator}";
                    author.Url = $"https://osu.ppy.sh/b/{mapID}";
                    author.IconUrl = $"https://a.ppy.sh/{creatorID}";
                });

                embed.WithDescription(
                    $"**{mapTitle} [{difficulty}]** by **{artist}**" +
                    $"\n" +
                    $"\n<:total_length:567812515346513932> **Total Length:** {lengthValue} <:bpm:567813349820071937> **BPM:** {bpm.ToString("N0")}" +
                    $"\n**Star Rating:** `{starRating.ToString("N2")} ☆` **Maximum Combo:** `{maxCombo.ToString("N0")}x`" +
                    $"\n**Download:** [[Beatmap]](https://osu.ppy.sh/beatmapsets/{beatmapSetID}/download)" +
                    $"[(without video)](https://osu.ppy.sh/d/{beatmapSetID}n)" +
                    $"\n" +
                    $"\n**CS:** `{(float)cs} ` **AR:** `{(float)ar}` **OD:** `{(float)od}` **HP:** `{(float)hp}`" +
                    $"\n" +
                    $"\n**95% FC:** `{unranked95.Total.ToString("N0")}pp` **98% FC:** `{unranked98.Total.ToString("N0")}pp`" +
                    $"\n**99% FC:** `{unranked99.Total.ToString("N0")}pp` **100% FC (SS):** `{unranked100.Total.ToString("N0")}pp`");
                embed.WithFooter($"Status: {status} | 💙 Amount: {favoriteCount}");
                embed.EmbedType = EmbedType.PINK;
                await context.Channel.SendMessageAsync(embed: embed.Build());
                stopWatch.Stop();
                logger.ConsoleCommandLog(context, stopWatch.ElapsedMilliseconds);
                return;
            }

            double passRate = (playCount / passCount);

            //Tillerino API Data: Performance Point values for full combos on the map with the given accuracy.

            double value75 = mapObject.ppForAcc.entry[0].value;
            double value90 = mapObject.ppForAcc.entry[3].value;
            double value95 = mapObject.ppForAcc.entry[5].value;
            double value98 = mapObject.ppForAcc.entry[9].value;
            double value99 = mapObject.ppForAcc.entry[11].value;
            double value100 = mapObject.ppForAcc.entry[13].value;

            embed.WithAuthor(author =>
            {
                author.Name = $"{mapTitle} by {creator}";
                author.Url = $"https://osu.ppy.sh/b/{mapID}";
                author.IconUrl = $"https://a.ppy.sh/{creatorID}";
            });
            embed.WithDescription($"**{mapTitle} [{difficulty}]** by **{artist}**" +
                $"\n" +
                $"\n<:total_length:567812515346513932> **Total Length:** {lengthValue} <:bpm:567813349820071937> **BPM:** {bpm.ToString("N0")}" +
                $"\n**Star Rating:** `{starRating.ToString("N2")} ☆` **Maximum Combo:** `{maxCombo}x`" +
                $"\n**Download:** [[Beatmap]](https://osu.ppy.sh/beatmapsets/{beatmapSetID}/download)" +
                $"[(without video)](https://osu.ppy.sh/d/{beatmapSetID}n) [[Bloodcat Mirror]](https://bloodcat.com/osu/s/{beatmapSetID})" +
                $"\n" +
                $"\n**CS:** `{(float)cs} ` **AR:** `{(float)ar}` **OD:** `{(float)od}` **HP:** `{(float)hp}`" +
                $"\n" +
                $"\n**75% FC:** `{(int)value75}pp` **90% FC:** `{(int)value90}pp`" +
                $"\n**95% FC:** `{(int)value95}pp` **98% FC:** `{(int)value98}pp`" +
                $"\n**99% FC:** `{(int)value99}pp` **100% FC (SS):** `{(int)value100}pp`");
            embed.WithFooter($"Status: {status} | 💙 Amount: {favoriteCount} | Pass Rate: {passRate.ToString("N2")}%");
            embed.EmbedType = EmbedType.PINK;
            await context.Channel.SendMessageAsync(embed: embed.Build());
            stopWatch.Stop();
            logger.ConsoleCommandLog(context, stopWatch.ElapsedMilliseconds);
            return;
        }
    }


}
