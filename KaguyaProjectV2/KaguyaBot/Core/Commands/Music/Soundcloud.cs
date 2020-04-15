using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Soundcloud : KaguyaBase
    {
        [MusicCommand]
        [Command("Soundcloud")]
        [Alias("sc")]
        [Summary("Allows either a [Kaguya Supporter](https://the-kaguya-project.myshopify.com/products/kaguya-supporter-tag) " +
                 "or [Kaguya Premium](https://the-kaguya-project.myshopify.com/products/kaguya-premium) server " +
                 "to search Soundcloud for a desired song.")]
        [Remarks("<search>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command([Remainder]string query)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (server.IsPremium || user.IsSupporter)
            {
                var playInstance = new Search();
                var data = await playInstance.SearchAndPlayAsync(Context, query, false, SearchProvider.Soundcloud);

                if (data != null)
                    await InlineReactionReplyAsync(data);
            }
            else
            {
                throw new KaguyaSupportException($"This feature is restricted to `Kaguya Supporters` and " +
                                                 $"servers with `Kaguya Premium` status.");
            }
        }
    }
}
