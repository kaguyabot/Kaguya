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
            var bestArray = ExecuteJson(OsuRequest.BEST_PERFORMANCE);
            bestArray = specific ? ProcessJson(bestArray, true) : ProcessJson(bestArray);

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
                item.PlayNumber = i;

                //Calculate accuracy for osu!standard
                item.Accuracy = OsuExtension.OsuAccuracy(item.Count50, item.Count100, item.Count300, item.Countmiss);

                //Get string for mods
                item.StringMods = OsuExtension.ModeNames(item.EnabledMods);

                //Fill in Emote of Grade
                item.RankEmote = OsuExtension.OsuGrade(item.Rank);

                string mapBest;
                using (var client = new WebClient())
                {
                    mapBest = client.DownloadString($"https://osu.ppy.sh/api/get_beatmaps?k={ConfigProperties.BotConfig.OsuApiKey}&b={item.BeatmapId}");
                }

                item.Beatmap = JsonConvert.DeserializeObject<OsuBeatmapModel[]>(mapBest)[0];
                i++;
            }

            return array;
        }
    }
}
