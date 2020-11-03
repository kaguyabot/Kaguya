using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Soundcloud : KaguyaBase
    {
        [PremiumUserCommand]
        [MusicCommand]
        [Command("Soundcloud")]
        [Alias("sc")]
        [Summary("Allows either a " +
                 "[Kaguya Premium](https://sellix.io/KaguyaStore) server or subscriber " +
                 "to search Soundcloud for a desired song.")]
        [Remarks("<search>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command([Remainder] string query)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (server.IsPremium || user.IsPremium)
            {
                var playInstance = new Search();
                ReactionCallbackData? data = await playInstance.SearchAndPlayAsync(Context, query, false, SearchProvider.SOUNDCLOUD);

                if (data != null)
                    await InlineReactionReplyAsync(data);
            }
            else
            {
                throw new KaguyaSupportException($"This feature is restricted to `Kaguya Premium Subscribers` and " +
                                                 $"servers with `Kaguya Premium` status.");
            }
        }
    }
}