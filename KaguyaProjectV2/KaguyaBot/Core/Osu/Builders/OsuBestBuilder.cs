using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Builders
{
    public class OsuBestBuilder : OsuBaseBuilder<OsuBestModel>
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

        public OsuBestBuilder(string userid, int mode = 0, int limit = 5)
        {
            UserId = userid;
            Mode = mode;
            Limit = limit;
        }

        public List<OsuBestModel> Execute(bool specific = false)
        {
            var bestArray = ExecuteJson(OsuRequest.BestPerformance);
            bestArray = specific ? ProcessJson(bestArray, specific) : ProcessJson(bestArray);

            return bestArray.ToList();
        }

        private OsuBestModel[] ProcessJson(OsuBestModel[] array, bool specific = false)
        {
            int i = 1;
            foreach (var item in array)
            {
                if (specific && Limit != i)
                {
                    i++;
                    continue;
                }

                //Sets playnumber of this play.
                item.play_number = i;

                //Calculate accuracy for osu!standard
                item.accuracy = OsuExtension.OsuAccuracy(item.count50, item.count100, item.count300, item.countmiss);

                //Get string for mods
                item.string_mods = OsuExtension.ModeNames(item.enabled_mods);

                //Fill in Emote of Grade
                item.rankemote = OsuExtension.OsuGrade(item.rank);

                string mapBest = "";
                using (WebClient client = new WebClient())
                {
                    mapBest = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={ConfigProperties.osuApiKey}&b={item.beatmap_id}");
                }
                item.beatmap = JsonConvert.DeserializeObject<OsuBeatmapModel[]>(mapBest)[0];

                i++;
            }

            return array;
        }
    }
}
