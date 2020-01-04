using KaguyaProjectV2.KaguyaBot.Core.Osu.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Osu.Builders
{
    public abstract class OsuBaseBuilder<T> where T : OsuBaseModel
    {
        private readonly string _apiKey = ConfigProperties.BotConfig.OsuApiKey;
        private const string BaseUrl = "https://osu.ppy.sh/api/";

        public string Build(OsuRequest request)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl);

            switch (request)
            {
                case OsuRequest.BEST_PERFORMANCE:
                    urlBuilder.Append("get_user_best");
                    break;
                case OsuRequest.RECENT_PLAYED:
                    urlBuilder.Append("get_user_recent");
                    break;
                case OsuRequest.USER:
                    urlBuilder.Append("get_user");
                    break;
            }

            urlBuilder.Append("?");
            urlBuilder.Append("k=");
            urlBuilder.Append(_apiKey);
            return Build(urlBuilder);
        }

        public abstract string Build(StringBuilder urlBuilder);

        protected T[] ExecuteJson(OsuRequest request)
        {
            var html = Build(request);
            html = new WebClient().DownloadString(html);
            return JsonConvert.DeserializeObject<T[]>(html);
        }
    }
}