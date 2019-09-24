using Kaguya.Core.Osu.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kaguya.Core.Osu.Builder
{
    public class OsuRecentBuilder : OsuJsonBaseBuilder<OsuRecentModel>
    {
        private readonly StringBuilder BaseUrl = new StringBuilder();
        private string GeneratedUrl;

        public OsuRecentBuilder(string userid, int mode = 0, int limit = 10)
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
                //Process json data here :D.
            }

            return array;
        }
    }
}
