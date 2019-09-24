using Kaguya.Core.Osu.Models;
using Newtonsoft.Json;
using OppaiSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Kaguya.Core.Osu.Builder
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

            Execute();
        }

        public List<OsuRecentModel> Execute()
        {
            var recentArray = ExecuteJson(OsuRequest.RecentPlayed);
            recentArray = ProcessJson(recentArray);

            return recentArray.ToList();
        }

        private OsuRecentModel[] ProcessJson(OsuRecentModel[] array)
        {
            foreach (var item in array)
            {
                //Calculate accuracy for osu!standard
                item.accuracy = OsuMisc.OsuAccuracy(item.count50, item.count100, item.count300, item.countmiss);

                //Get string for mods
                item.string_mods = OsuMisc.ModeNames(item.enabled_mods);

                //Fill in Emote of Grade
                item.rankemote = OsuMisc.OsuGrade(item.rank);

                //Get Beatmap of recent play
                string mapRecent = "";
                using (WebClient client = new WebClient())
                {
                    mapRecent = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={Config.bot.OsuApiKey}&b={item.beatmap_id}");
                }
                item.beatmap = JsonConvert.DeserializeObject<OsuBeatmapModel[]>(mapRecent)[0];

                //PPv2
                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{item.beatmap_id}");
                var beatmapData = Beatmap.Read(new StreamReader(new MemoryStream(data, false)));
                var diff = new DiffCalc().Calc(beatmapData, (Mods)item.enabled_mods);

                item.pp = new PPv2(new PPv2Parameters(beatmapData, diff, new Accuracy(item.count300, item.count100, item.count50, item.countmiss).Value(), item.countmiss, item.maxcombo, (Mods)item.enabled_mods)).Total;
                item.fullcombopp = new PPv2(new PPv2Parameters(beatmapData, diff, accuracy: (item.accuracy / 100), mods: (Mods)item.enabled_mods)).Total;

                //Get Beatmap completion rate and total count of hit notes 
                item.counttotal = (item.count300 + item.count100 + item.count50 + item.countmiss);
                item.completion = (item.counttotal / beatmapData.Objects.Count()) * 100;
            }

            return array;
        }
    }
}
