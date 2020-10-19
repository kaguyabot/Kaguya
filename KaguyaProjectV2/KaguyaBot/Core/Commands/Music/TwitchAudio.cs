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
    public class TwitchAudio : KaguyaBase
    {
        [DisabledCommand]
        [PremiumUserCommand]
        [MusicCommand]
        [Command("TwitchAudio")]
        [Alias("ta")]
        [Summary("Allows either a [Kaguya Supporter](https://sellix.io/KaguyaStoreproducts/kaguya-supporter-tag) " +
            "or [Kaguya Premium](https://sellix.io/KaguyaStore) server " +
            "to stream audio live from Twitch.")]
        [Remarks("<search>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder] string query)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (server.IsPremium || user.IsPremium)
            {
                var playInstance = new Search();
                ReactionCallbackData? data = await playInstance.SearchAndPlayAsync(Context, query, false, SearchProvider.TWITCH);

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