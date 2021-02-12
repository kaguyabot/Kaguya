using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Post;

namespace Kaguya.Internal.Services
{
    public static class NsfwService
    {
        public static async Task<IList<string>> GetHentaiUrlsAsync(int count, List<string> tags)
        {
            var booru = new Konachan();
            
            // Force to-lower in all tags.
            tags.ConvertAll(x => x.ToLower());
            
            if (!tags.Any() || !tags.Contains("sex"))
            {
                tags.Add("sex");
            }

            List<string> resultUrls = new(count);
            SearchResult[] results = await booru.GetRandomPostsAsync(count, tags.ToArray());

            foreach (SearchResult result in results)
            {
                resultUrls.Add(result.FileUrl.AbsoluteUri);
            }

            return resultUrls;
        }
    }
}