using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Models;
using Newtonsoft.Json;
using OppaiSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Builders
{
    public class OsuRecentBuilder : OsuBaseBuilder<OsuRecentModel>
    {
        public string UserId; // u
        public int Mode; // m
        public int Limit; // limit

        public override string Build(StringBuilder urlBuilder)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                urlBuilder.Append("&u=").Append(UserId);
            }

            if (!string.IsNullOrEmpty(Mode.ToString()))
            {
                urlBuilder.Append("&m=").Append(Mode);
            }

            if (!string.IsNullOrEmpty(Limit.ToString()))
            {
                urlBuilder.Append("&limit=").Append(Limit);
            }

            return urlBuilder.ToString();
        }

        public OsuRecentBuilder(string userid, int mode = 0, int limit = 1)
        {
            UserId = userid;
            Mode = mode;
            Limit = limit;
        }

        public List<OsuRecentModel> Execute()
        {
            var recentArray = ExecuteJson(OsuRequest.RECENT_PLAYED);
            recentArray = ProcessJson(recentArray);

            return recentArray.ToList();
        }

        private OsuRecentModel[] ProcessJson(OsuRecentModel[] array)
        {
            foreach (var item in array)
            {
                //Calculate accuracy for osu!standard
                item.Accuracy = OsuExtension.OsuAccuracy(item.Count50, item.Count100, item.Count300, item.Countmiss);

                //Get string for mods
                item.ModString = OsuExtension.ModeNames(item.EnabledMods);

                //Fill in Emote of Grade
                item.RankEmote = OsuExtension.OsuGrade(item.Rank);

                //Get Beatmap of recent play
                string mapRecent = "";
                using (WebClient client = new WebClient())
                {
                    mapRecent = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={ConfigProperties.BotConfig.OsuApiKey}&b={item.BeatmapId}");
                }
                item.Beatmap = JsonConvert.DeserializeObject<OsuBeatmapModel[]>(mapRecent)[0];

                //PPv2
                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{item.BeatmapId}");
                var beatmapData = Beatmap.Read(new StreamReader(new MemoryStream(data, false)));
                var diff = new DiffCalc().Calc(beatmapData, (Mods)item.EnabledMods);

                item.PP = new PPv2(new PPv2Parameters(beatmapData, diff, new Accuracy(item.Count300, item.Count100, item.Count50, item.Countmiss).Value(), item.Countmiss, item.MaxCombo, (Mods)item.EnabledMods)).Total;
                item.FullComboPP = new PPv2(new PPv2Parameters(beatmapData, diff, accuracy: (item.Accuracy / 100), mods: (Mods)item.EnabledMods)).Total;

                //Get Beatmap completion rate and total count of hit notes 
                item.CountTotal = (item.Count300 + item.Count100 + item.Count50 + item.Countmiss);
                item.Completion = item.CountTotal / (double)beatmapData.Objects.Count() * 100;
            }

            return array;
        }
    }
}
