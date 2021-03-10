using BooruSharp.Booru;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
			var results = await booru.GetRandomPostsAsync(count, tags.ToArray());

			foreach (var result in results)
			{
				resultUrls.Add(result.FileUrl.AbsoluteUri);
			}

			return resultUrls;
		}
	}
}