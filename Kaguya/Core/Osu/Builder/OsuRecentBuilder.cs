using Kaguya.Core.Osu.Models;
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
        private readonly StringBuilder BaseUrl = new StringBuilder();
        private string GeneratedUrl;

        public OsuRecentBuilder(string userid, int mode = 0, int limit = 5)
        {
            BaseUrl.Append($"https://osu.ppy.sh/api/get_user_recent?k={Config.bot.OsuApiKey}");

            if (!string.IsNullOrEmpty(userid))
            {
                BaseUrl.Append("&u=").Append(userid);
            }

            if (!string.IsNullOrEmpty(mode.ToString()))
            {
                BaseUrl.Append("&m=").Append(mode);
            }

            if (!string.IsNullOrEmpty(limit.ToString()))
            {
                BaseUrl.Append("&limit=").Append(limit);
            }

            GeneratedUrl = BaseUrl.ToString();
        }

        public List<OsuRecentModel> Execute()
        {
            var recentArray = ExecuteJson(GeneratedUrl);
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

                //Set NM string if no mods are enabled
                if (item.enabled_mods == 0)
                {
                    item.string_mods = "NM";
                }

                //PPv2
                byte[] data = new WebClient().DownloadData($"https://osu.ppy.sh/osu/{item.beatmap_id}");
                var beatmapData = Beatmap.Read(new StreamReader(new MemoryStream(data, false)));
                var diff = new DiffCalc().Calc(beatmapData, (Mods)item.enabled_mods);

                item.pp = new PPv2(new PPv2Parameters(beatmapData, diff, new Accuracy(item.count300, item.count100, item.count50, item.countmiss).Value(), item.countmiss, item.maxcombo, (Mods)item.enabled_mods)).Total;
                item.fullcombopp = new PPv2(new PPv2Parameters(beatmapData, diff, accuracy: (item.accuracy / 100), mods: (Mods)item.enabled_mods)).Total;
            }

            return array;
        }
    }
}
