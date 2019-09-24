using Kaguya.Core.Osu.Models;
using Newtonsoft.Json;
using System.Net;

namespace Kaguya.Core.Osu.Builder
{
    public abstract class OsuJsonBaseBuilder<T> where T : OsuBaseModel
    {
        protected T[] ExecuteJson(string url)
        {
            url = new WebClient().DownloadString(url);
            return JsonConvert.DeserializeObject<T[]>(url);
        }
    }
}
